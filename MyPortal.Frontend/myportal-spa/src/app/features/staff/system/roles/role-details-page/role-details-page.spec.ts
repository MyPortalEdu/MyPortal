import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj, type SpyObj } from '@testing/spy';

import { RoleDetailsPage } from './role-details-page';
import { RolesDataService } from '../../../../../shared/services/roles-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { MeService } from '../../../../../core/services/me-service';
import { Permissions } from '../../../../../core/constants/permissions';
import { UserType } from '../../../../../core/types/user-type';
import { RoleDetailsResponse, RoleUpsertRequest } from '../../../../../shared/types/role';
import { Me } from '../../../../../core/types/me';

interface RoleFormModel {
  name: string;
  description: string;
}
interface FieldLike {
  valid(): boolean;
  invalid(): boolean;
  submitting(): boolean;
}
interface Internals {
  model: { (): RoleFormModel; set(v: RoleFormModel): void; update(fn: (m: RoleFormModel) => RoleFormModel): void };
  f: () => FieldLike;
}

function makeRole(overrides: Partial<RoleDetailsResponse> = {}): RoleDetailsResponse {
  return {
    id: 'role-1',
    name: 'Head of Year',
    description: 'desc',
    isSystem: false,
    isDefault: false,
    userType: UserType.Staff,
    permissionIds: ['p1'],
    ...overrides,
  };
}

function makeMe(overrides: Partial<Me> = {}): Me {
  return {
    id: 'me',
    username: 'me',
    userType: UserType.Staff,
    isEnabled: true,
    isSystem: false,
    displayName: 'Me',
    permissions: [Permissions.SystemAdmin.EditRoles],
    ...overrides,
  };
}

describe('RoleDetailsPage', () => {
  let fixture: ComponentFixture<RoleDetailsPage>;
  let component: RoleDetailsPage;
  let internals: Internals;
  let roles: SpyObj<RolesDataService>;
  let notify: SpyObj<NotificationService>;
  let me$: SpyObj<MeService>;

  function configure(role: RoleDetailsResponse = makeRole(), me: Me = makeMe()) {
    roles = createSpyObj<RolesDataService>(['getById', 'permissions', 'update']);
    notify = createSpyObj<NotificationService>(['success', 'apiError']);
    me$ = createSpyObj<MeService>(['me']);

    roles.getById.mockReturnValue(of(role));
    roles.permissions.mockReturnValue(of([]));
    roles.update.mockReturnValue(of(void 0));
    me$.me.mockReturnValue(of(me));

    const route = {
      snapshot: { paramMap: { get: () => role.id } },
    } as unknown as ActivatedRoute;

    const translocoStub = {
      translate: (k: string) => k,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(RoleDetailsPage, { set: { template: '' } });

    TestBed.configureTestingModule({
      providers: [
        { provide: RolesDataService, useValue: roles },
        { provide: NotificationService, useValue: notify },
        { provide: MeService, useValue: me$ },
        { provide: ConfirmationDialog, useValue: createSpyObj<ConfirmationDialog>(['confirm']) },
        { provide: Router, useValue: createSpyObj<Router>(['navigate']) },
        { provide: ActivatedRoute, useValue: route },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    });

    fixture = TestBed.createComponent(RoleDetailsPage);
    component = fixture.componentInstance;
    internals = component as unknown as Internals;
    fixture.detectChanges();
  }

  it('hydrates the model from the loaded role and is valid', () => {
    configure();
    expect(internals.model().name).toBe('Head of Year');
    expect(internals.model().description).toBe('desc');
    expect(internals.f().valid()).toBe(true);
  });

  it('goes invalid when the name is cleared to whitespace', () => {
    configure();
    internals.model.update(m => ({ ...m, name: '   ' }));
    expect(internals.f().invalid()).toBe(true);
  });

  it('save() posts the trimmed upsert payload and toasts on success', async () => {
    configure();
    internals.model.set({ name: '  Renamed  ', description: '  d  ' });

    await component.save();

    const [id, payload] = roles.update.mock.calls.at(-1)! as [string, RoleUpsertRequest];
    expect(id).toBe('role-1');
    expect(payload.name).toBe('Renamed');
    expect(payload.description).toBe('d');
    expect(payload.permissionIds).toEqual(['p1']);
    expect(notify.success).toHaveBeenCalled();
  });

  it('save() is a no-op when nothing is dirty', async () => {
    configure();
    await component.save();
    expect(roles.update).not.toHaveBeenCalled();
  });

  it('save() surfaces an apiError toast on failure', async () => {
    configure();
    roles.update.mockReturnValue(throwError(() => new Error('boom')));
    internals.model.update(m => ({ ...m, name: 'Changed' }));

    await component.save();

    expect(notify.apiError).toHaveBeenCalled();
  });

  it('read-only (system role) blocks save', async () => {
    configure(makeRole({ isSystem: true }));
    internals.model.update(m => ({ ...m, name: 'Changed' }));
    expect(component.readOnly()).toBe(true);

    await component.save();

    expect(roles.update).not.toHaveBeenCalled();
  });
});
