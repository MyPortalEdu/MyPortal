import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { MpButton, MpSkeleton } from '@myportal/ui';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { StudentGroupPicker } from '../../../../shared/components/pickers/student-group-picker/student-group-picker';
import { BulletinCategoryFormDialog } from './bulletin-category-form-dialog/bulletin-category-form-dialog';
import { BulletinsDataService } from '../../../../shared/services/bulletins-data.service';
import { BreakpointService } from '../../../../shared/services/breakpoint-service';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { SelectedAcademicYearService } from '../../../../core/services/selected-academic-year-service';
import {
  BulletinAllowedGroupResponse,
  BulletinCategoryResponse,
} from '../../../../shared/types/bulletin';
import { StudentGroupSummaryResponse } from '../../../../shared/types/student-group';

@Component({
  selector: 'mp-bulletin-settings-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MpButton,
    MpSkeleton,
    PageHeader,
    EmptyState,
    StudentGroupPicker,
    BulletinCategoryFormDialog,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('bulletin-settings')],
  templateUrl: './bulletin-settings-page.html',
})
export class BulletinSettingsPage implements OnInit {
  private readonly data = inject(BulletinsDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);
  protected readonly bp = inject(BreakpointService);
  private readonly selectedYear = inject(SelectedAcademicYearService);

  readonly academicYearId = this.selectedYear.selectedId;

  readonly categories = signal<BulletinCategoryResponse[]>([]);
  readonly categoriesLoading = signal(false);
  readonly categoryFormOpen = signal(false);
  readonly editingCategory = signal<BulletinCategoryResponse | null>(null);

  readonly allowedGroups = signal<BulletinAllowedGroupResponse[]>([]);
  readonly allowedGroupsLoading = signal(false);

  readonly excludeIds = computed(() => this.allowedGroups().map(g => g.studentGroupId));

  ngOnInit(): void {
    this.refreshCategories();
    this.refreshSettings();
  }

  refreshCategories(): void {
    this.categoriesLoading.set(true);
    this.data.listCategories(true).subscribe({
      next: cats => {
        this.categories.set(cats ?? []);
        this.categoriesLoading.set(false);
      },
      error: err => {
        this.categoriesLoading.set(false);
        this.notify.apiError(err, this.transloco.translate('bulletin-settings.categories.errorLoad'));
      },
    });
  }

  openNewCategory(): void {
    this.editingCategory.set(null);
    this.categoryFormOpen.set(true);
  }

  openEditCategory(category: BulletinCategoryResponse): void {
    this.editingCategory.set(category);
    this.categoryFormOpen.set(true);
  }

  closeCategoryForm(): void {
    this.categoryFormOpen.set(false);
    this.editingCategory.set(null);
  }

  onCategorySaved(): void {
    this.closeCategoryForm();
    this.refreshCategories();
  }

  async deleteCategory(category: BulletinCategoryResponse): Promise<void> {
    const ok = await this.confirm.danger({
      message: this.transloco.translate('bulletin-settings.categories.deleteConfirm', {
        name: category.name,
      }),
    });
    if (!ok) return;

    this.data.deleteCategory(category.id).subscribe({
      next: () => {
        this.notify.success(this.transloco.translate('bulletin-settings.categories.deletedToast'));
        this.refreshCategories();
      },
      error: err => this.notify.apiError(err,
        this.transloco.translate('bulletin-settings.categories.errorDelete')),
    });
  }

  refreshSettings(): void {
    this.allowedGroupsLoading.set(true);
    this.data.getSettings().subscribe({
      next: s => {
        this.allowedGroups.set(s.allowedAudienceGroups ?? []);
        this.allowedGroupsLoading.set(false);
      },
      error: err => {
        this.allowedGroupsLoading.set(false);
        this.notify.apiError(err, this.transloco.translate('bulletin-settings.audiences.errorLoad'));
      },
    });
  }

  onGroupsPicked(groups: StudentGroupSummaryResponse[]): void {
    const existing = this.allowedGroups().map(g => g.studentGroupId);
    const additions = groups.map(g => g.id).filter(id => !existing.includes(id));
    if (additions.length === 0) return;
    this.saveAllowlist([...existing, ...additions]);
  }

  async removeGroup(group: BulletinAllowedGroupResponse): Promise<void> {
    const ok = await this.confirm.danger({
      message: this.transloco.translate('bulletin-settings.audiences.removeConfirm', {
        name: group.name,
      }),
      acceptLabel: this.transloco.translate('bulletin-settings.audiences.remove'),
    });
    if (!ok) return;

    const next = this.allowedGroups()
      .map(g => g.studentGroupId)
      .filter(id => id !== group.studentGroupId);
    this.saveAllowlist(next);
  }

  private saveAllowlist(ids: string[]): void {
    this.data.updateSettings({ allowedAudienceGroupIds: ids }).subscribe({
      next: () => {
        this.notify.success(this.transloco.translate('bulletin-settings.audiences.updatedToast'));
        this.refreshSettings();
      },
      error: err => this.notify.apiError(err,
        this.transloco.translate('bulletin-settings.audiences.errorUpdate')),
    });
  }
}
