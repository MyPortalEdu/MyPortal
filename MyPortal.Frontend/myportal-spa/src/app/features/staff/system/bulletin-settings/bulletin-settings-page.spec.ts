import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';

import { BulletinSettingsPage } from './bulletin-settings-page';
import { BulletinsDataService } from '../../../../shared/services/bulletins-data.service';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AcademicYearService } from '../../../../core/services/academic-year-service';
import { SelectedAcademicYearService } from '../../../../core/services/selected-academic-year-service';
import {
  BulletinAllowedGroupResponse,
  BulletinCategoryResponse,
  BulletinSettingsResponse,
} from '../../../../shared/types/bulletin';
import { StudentGroupSummaryResponse } from '../../../../shared/types/student-group';

const categories: BulletinCategoryResponse[] = [
  { id: 'c1', name: 'Notices',  icon: 'i', colourCode: '#000000', displayOrder: 1, active: true,  isSystem: false, version: 1 },
  { id: 'c2', name: 'Inactive', icon: 'i', colourCode: '#000000', displayOrder: 2, active: false, isSystem: false, version: 1 },
];
const allowed: BulletinAllowedGroupResponse[] = [
  { studentGroupId: 'g1', code: '7A', name: 'Year 7A' },
];

describe('BulletinSettingsPage', () => {
  let component: BulletinSettingsPage;
  let data: jasmine.SpyObj<BulletinsDataService>;
  let notify: jasmine.SpyObj<NotificationService>;
  let confirm: jasmine.SpyObj<ConfirmationDialog>;
  let academic: jasmine.SpyObj<AcademicYearService>;

  beforeEach(async () => {
    data = jasmine.createSpyObj<BulletinsDataService>('BulletinsDataService',
      ['listCategories', 'deleteCategory', 'getSettings', 'updateSettings']);
    notify = jasmine.createSpyObj<NotificationService>('NotificationService', ['success', 'apiError']);
    confirm = jasmine.createSpyObj<ConfirmationDialog>('ConfirmationDialog', ['danger']);
    academic = jasmine.createSpyObj<AcademicYearService>('AcademicYearService', ['getCurrent']);

    data.listCategories.and.returnValue(of(categories));
    data.getSettings.and.returnValue(of({ allowedAudienceGroups: allowed } as BulletinSettingsResponse));
    data.updateSettings.and.returnValue(of(void 0));
    data.deleteCategory.and.returnValue(of(void 0));
    academic.getCurrent.and.returnValue(of({ id: 'ay-current', name: '2026/27', startDate: '', endDate: '' } as any));

    const translocoStub = {
      translate: (key: string) => key,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(BulletinSettingsPage, { set: { template: '' } });

    await TestBed.configureTestingModule({
      imports: [BulletinSettingsPage],
      providers: [
        { provide: BulletinsDataService, useValue: data },
        { provide: NotificationService, useValue: notify },
        { provide: ConfirmationDialog, useValue: confirm },
        { provide: AcademicYearService, useValue: academic },
        // academicYearId is driven by SelectedAcademicYearService.selectedId (a signal), not getCurrent.
        { provide: SelectedAcademicYearService, useValue: { selectedId: signal('ay-current') } },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(BulletinSettingsPage);
    component = fixture.componentInstance;
    fixture.detectChanges(); // triggers ngOnInit
  });

  it('ngOnInit loads categories (including inactive), settings, and current academic year', () => {
    expect(data.listCategories).toHaveBeenCalledWith(true);
    expect(data.getSettings).toHaveBeenCalled();
    expect(component.categories().length).toBe(2);
    expect(component.allowedGroups().length).toBe(1);
    expect(component.academicYearId()).toBe('ay-current');
  });

  it('excludeIds reflects current allowlist (used by the picker to hide already-added rows)', () => {
    expect(component.excludeIds()).toEqual(['g1']);
  });

  it('openNewCategory clears editing state then opens the form', () => {
    component.editingCategory.set(categories[0]);
    component.openNewCategory();
    expect(component.editingCategory()).toBeNull();
    expect(component.categoryFormOpen()).toBeTrue();
  });

  it('openEditCategory selects a category and opens the form', () => {
    component.openEditCategory(categories[0]);
    expect(component.editingCategory()).toBe(categories[0]);
    expect(component.categoryFormOpen()).toBeTrue();
  });

  it('onCategorySaved closes the form and re-fetches categories', () => {
    data.listCategories.calls.reset();
    component.onCategorySaved();
    expect(component.categoryFormOpen()).toBeFalse();
    expect(component.editingCategory()).toBeNull();
    expect(data.listCategories).toHaveBeenCalledWith(true);
  });

  it('deleteCategory prompts to confirm, deletes, toasts, and refreshes', async () => {
    confirm.danger.and.resolveTo(true);
    data.listCategories.calls.reset();

    await component.deleteCategory(categories[0]);

    expect(data.deleteCategory).toHaveBeenCalledWith('c1');
    expect(notify.success).toHaveBeenCalled();
    expect(data.listCategories).toHaveBeenCalled();
  });

  it('deleteCategory does nothing when the user cancels the confirm prompt', async () => {
    confirm.danger.and.resolveTo(false);
    await component.deleteCategory(categories[0]);
    expect(data.deleteCategory).not.toHaveBeenCalled();
  });

  it('deleteCategory surfaces an apiError toast on failure', async () => {
    confirm.danger.and.resolveTo(true);
    data.deleteCategory.and.returnValue(throwError(() => new Error('boom')));

    await component.deleteCategory(categories[0]);

    expect(notify.apiError).toHaveBeenCalled();
  });

  it('onGroupsPicked merges new ids onto the existing allowlist and posts the union', () => {
    const picks: StudentGroupSummaryResponse[] = [
      { id: 'g1', code: '7A', description: 'Year 7A', studentCount: 0, academicYearId: 'ay-current', isSystem: false, isActive: true, version: 1 } as unknown as StudentGroupSummaryResponse,
      { id: 'g2', code: '7B', description: 'Year 7B', studentCount: 0, academicYearId: 'ay-current', isSystem: false, isActive: true, version: 1 } as unknown as StudentGroupSummaryResponse,
    ];

    component.onGroupsPicked(picks);

    // g1 is already in the allowlist; only g2 is genuinely new — but the payload
    // is the FULL list, not a diff, so both ids end up in the update.
    expect(data.updateSettings).toHaveBeenCalledWith({ allowedAudienceGroupIds: ['g1', 'g2'] });
  });

  it('onGroupsPicked no-ops when every picked group is already in the allowlist', () => {
    component.onGroupsPicked([
      { id: 'g1', code: '7A', description: 'Year 7A', studentCount: 0, academicYearId: 'ay-current', isSystem: false, isActive: true, version: 1 } as unknown as StudentGroupSummaryResponse,
    ]);

    expect(data.updateSettings).not.toHaveBeenCalled();
  });

  it('removeGroup prompts to confirm and posts the allowlist minus the removed id', async () => {
    confirm.danger.and.resolveTo(true);

    await component.removeGroup({ studentGroupId: 'g1', code: '7A', name: 'Year 7A' });

    expect(data.updateSettings).toHaveBeenCalledWith({ allowedAudienceGroupIds: [] });
  });

  it('removeGroup does nothing when the user cancels the confirm prompt', async () => {
    confirm.danger.and.resolveTo(false);

    await component.removeGroup({ studentGroupId: 'g1', code: '7A', name: 'Year 7A' });

    expect(data.updateSettings).not.toHaveBeenCalled();
  });

  it('saveAllowlist (via onGroupsPicked) toasts success and refreshes after a save', () => {
    data.getSettings.calls.reset();

    component.onGroupsPicked([
      { id: 'g2', code: '7B', description: 'Year 7B', studentCount: 0, academicYearId: 'ay-current', isSystem: false, isActive: true, version: 1 } as unknown as StudentGroupSummaryResponse,
    ]);

    expect(notify.success).toHaveBeenCalled();
    expect(data.getSettings).toHaveBeenCalled();
  });

  it('saveAllowlist toasts apiError when the server rejects', () => {
    data.updateSettings.and.returnValue(throwError(() => new Error('boom')));

    component.onGroupsPicked([
      { id: 'g2', code: '7B', description: 'Year 7B', studentCount: 0, academicYearId: 'ay-current', isSystem: false, isActive: true, version: 1 } as unknown as StudentGroupSummaryResponse,
    ]);

    expect(notify.apiError).toHaveBeenCalled();
  });
});
