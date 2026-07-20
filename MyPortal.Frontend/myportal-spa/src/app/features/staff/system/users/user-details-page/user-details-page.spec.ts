import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj, type SpyObj } from '@testing/spy';

import { UserDetailsPage } from './user-details-page';
import { UsersDataService } from '../../../../../shared/services/users-data.service';
import { RolesDataService } from '../../../../../shared/services/roles-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { MeService } from '../../../../../core/services/me-service';
import { Permissions } from '../../../../../core/constants/permissions';
import { UserType } from '../../../../../core/types/user-type';
import { UserDetailsResponse, UserUpsertRequest, UserUpdateRequest } from '../../../../../shared/types/user';
import { RoleSummaryResponse } from '../../../../../shared/types/role';
import { Me } from '../../../../../core/types/me';

interface UserFormModel {
  username: string;
  email: string;
  password: string;
  userType: UserType;
  isEnabled: boolean;
}
interface FieldLike {
  valid(): boolean;
  invalid(): boolean;
  submitting(): boolean;
  touched(): boolean;
}
interface Internals {
  model: { (): UserFormModel; set(v: UserFormModel): void; update(fn: (m: UserFormModel) => UserFormModel): void };
  f: (() => FieldLike) & { username: () => FieldLike; password: () => FieldLike };
}

function makeRole(overrides: Partial<RoleSummaryResponse> = {}): RoleSummaryResponse {
  return {
    id: 'r1',
    name: 'Role',
    description: null,
    userType: UserType.Staff,
    isSystem: false,
    userCount: 0,
    ...overrides,
  } as RoleSummaryResponse;
}

function makeUser(overrides: Partial<UserDetailsResponse> = {}): UserDetailsResponse {
  return {
    id: 'u1',
    createdAt: '2026-01-01T00:00:00Z',
    personId: null,
    userType: UserType.Staff,
    isEnabled: true,
    isSystem: false,
    personFullName: null,
    username: 'existing',
    email: 'e@x.com',
    phoneNumber: null,
    twoFactorEnabled: false,
    lockoutEnabled: false,
    roleIds: ['r1'],
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
    permissions: [Permissions.SystemAdmin.EditUsers],
    ...overrides,
  };
}

describe('UserDetailsPage', () => {
  let fixture: ComponentFixture<UserDetailsPage>;
  let component: UserDetailsPage;
  let internals: Internals;
  let users: SpyObj<UsersDataService>;
  let roles: SpyObj<RolesDataService>;
  let notify: SpyObj<NotificationService>;
  let me$: SpyObj<MeService>;
  let router: SpyObj<Router>;

  const allRoles = [
    makeRole({ id: 'r1', name: 'Staff role', userType: UserType.Staff }),
    makeRole({ id: 'r2', name: 'Parent role', userType: UserType.Parent }),
  ];

  function configure(create: boolean, id: string | null) {
    users = createSpyObj<UsersDataService>(['getById', 'create', 'update', 'effectivePermissions']);
    roles = createSpyObj<RolesDataService>(['all', 'permissions']);
    notify = createSpyObj<NotificationService>(['success', 'apiError']);
    me$ = createSpyObj<MeService>(['me']);
    router = createSpyObj<Router>(['navigate']);

    roles.all.mockReturnValue(of({ items: allRoles, totalItems: 2 }));
    roles.permissions.mockReturnValue(of([]));
    users.getById.mockReturnValue(of(makeUser()));
    users.create.mockReturnValue(of({ id: 'new-id' }));
    users.update.mockReturnValue(of(void 0));
    users.effectivePermissions.mockReturnValue(of([]));
    me$.me.mockReturnValue(of(makeMe()));
    router.navigate.mockResolvedValue(true);

    const route = {
      snapshot: { data: { create }, paramMap: { get: () => id } },
    } as unknown as ActivatedRoute;

    const translocoStub = {
      translate: (k: string) => k,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(UserDetailsPage, { set: { template: '' } });

    TestBed.configureTestingModule({
      providers: [
        { provide: UsersDataService, useValue: users },
        { provide: RolesDataService, useValue: roles },
        { provide: NotificationService, useValue: notify },
        { provide: MeService, useValue: me$ },
        { provide: ConfirmationDialog, useValue: createSpyObj<ConfirmationDialog>(['confirm']) },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: route },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    });

    fixture = TestBed.createComponent(UserDetailsPage);
    component = fixture.componentInstance;
    internals = component as unknown as Internals;
    fixture.detectChanges();
  }

  it('create mode: form is invalid until both username and password are provided', () => {
    configure(true, null);
    expect(component.isCreate()).toBe(true);
    expect(internals.f().invalid()).toBe(true);

    internals.model.update(m => ({ ...m, username: 'newuser' }));
    expect(internals.f().invalid()).toBe(true);

    internals.model.update(m => ({ ...m, password: 'secret' }));
    expect(internals.f().valid()).toBe(true);
  });

  it('edit mode: password is not required, hydrates from the loaded user', () => {
    configure(false, 'u1');
    expect(internals.model().username).toBe('existing');
    expect(internals.model().email).toBe('e@x.com');
    expect(internals.f().valid()).toBe(true);
  });

  it('changing userType prunes roles that do not belong to the new audience', () => {
    configure(true, null);
    (component as unknown as { selectedRoleIds: { set(v: ReadonlySet<string>): void } }).selectedRoleIds.set(
      new Set(['r1', 'r2']),
    );
    internals.model.update(m => ({ ...m, userType: UserType.Parent }));
    fixture.detectChanges();
    expect(component.isRoleSelected('r1')).toBe(false);
    expect(component.isRoleSelected('r2')).toBe(true);
  });

  it('save() in create mode posts the trimmed upsert payload and navigates', async () => {
    configure(true, null);
    internals.model.set({
      username: '  newuser  ',
      email: '  n@x.com  ',
      password: 'secret',
      userType: UserType.Staff,
      isEnabled: true,
    });

    await component.save();

    const payload = users.create.mock.calls.at(-1)![0] as UserUpsertRequest;
    expect(payload.username).toBe('newuser');
    expect(payload.email).toBe('n@x.com');
    expect(payload.password).toBe('secret');
    expect(notify.success).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/staff/system/users', 'new-id']);
  });

  it('save() in edit mode calls update with the current field values', async () => {
    configure(false, 'u1');
    internals.model.update(m => ({ ...m, username: 'renamed' }));

    await component.save();

    const [id, payload] = users.update.mock.calls.at(-1)! as [string, UserUpdateRequest];
    expect(id).toBe('u1');
    expect(payload.username).toBe('renamed');
    expect(users.create).not.toHaveBeenCalled();
  });

  it('save() surfaces an apiError toast when create fails', async () => {
    configure(true, null);
    users.create.mockReturnValue(throwError(() => new Error('boom')));
    internals.model.set({
      username: 'newuser',
      email: '',
      password: 'secret',
      userType: UserType.Staff,
      isEnabled: true,
    });

    await component.save();

    expect(notify.apiError).toHaveBeenCalled();
  });

  it('read-only (system user) blocks save entirely', async () => {
    configure(false, 'u1');
    (component as unknown as { isSystem: { set(v: boolean): void } }).isSystem.set(true);
    expect(component.readOnly()).toBe(true);

    await component.save();

    expect(users.update).not.toHaveBeenCalled();
  });
});
