import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  OnInit,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule, NgForm } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { MpButton, MpCard, MpCheckbox, MpInput, MpBadge, MpSelect, MpSkeleton } from '@myportal/ui';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { Field } from '../../../../../shared/components/field/field';
import { SectionHeader } from '../../../../../shared/components/section-header/section-header';
import { EmptyState } from '../../../../../shared/components/empty-state/empty-state';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { PersonPicker } from '../../../../../shared/components/pickers/person-picker/person-picker';
import { UsersDataService } from '../../../../../shared/services/users-data.service';
import { RolesDataService } from '../../../../../shared/services/roles-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { MeService } from '../../../../../core/services/me-service';
import { Permissions } from '../../../../../core/constants/permissions';
import { CanComponentDeactivate } from '../../../../../core/guards/can-deactivate.guard';
import { UserUpsertRequest, UserUpdateRequest, PersonSearchResponse } from '../../../../../shared/types/user';
import { RoleSummaryResponse } from '../../../../../shared/types/role';
import { UserType } from '../../../../../core/types/user-type';
import {
  PermissionTree,
  PermissionTreeNode,
  buildPermissionTree,
} from '../../roles/permission-tree/permission-tree';
import { UserSetPasswordDialog } from '../user-set-password-dialog/user-set-password-dialog';
import { focusFirstInvalid } from '../../../../../shared/utils/focus-first-invalid';

interface AudienceOption {
  label: string;
  value: UserType;
}

@Component({
  selector: 'mp-user-details-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    DatePipe,
    MpButton,
    MpCard,
    MpInput,
    MpCheckbox,
    MpSelect,
    MpBadge,
    MpSkeleton,
    PageHeader,
    Field,
    SectionHeader,
    EmptyState,
    PersonPicker,
    PermissionTree,
    UserSetPasswordDialog,
    TranslocoDirective,
    TranslocoPipe,
  ],
  providers: [provideTranslocoScope('users')],
  templateUrl: './user-details-page.html',
})
export class UserDetailsPage implements OnInit, CanComponentDeactivate {
  private readonly usersData = inject(UsersDataService);
  private readonly rolesData = inject(RolesDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly me = inject(MeService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly host = inject(ElementRef<HTMLElement>);

  private readonly form = viewChild<NgForm>('f');

  private readonly canEditUsers = signal(false);

  readonly isCreate = signal(false);
  protected userId = '';

  readonly username = signal('');
  readonly email = signal('');
  readonly password = signal('');
  readonly userType = signal<UserType>(UserType.Staff);
  readonly isEnabled = signal(true);
  readonly personId = signal<string | null>(null);
  readonly personName = signal<string | null>(null);
  readonly selectedRoleIds = signal<ReadonlySet<string>>(new Set());
  readonly allRoles = signal<RoleSummaryResponse[]>([]);
  readonly isSystem = signal(false);
  readonly createdAt = signal<string | null>(null);

  readonly effectiveTreeNodes = signal<PermissionTreeNode[]>([]);
  readonly effectiveSelectedIds = signal<ReadonlySet<string>>(new Set());

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly notFound = signal(false);
  readonly setPasswordOpen = signal(false);

  readonly skeletonRows = [1, 2, 3, 4, 5];

  readonly audienceOptions = computed<AudienceOption[]>(() => [
    { label: this.transloco.translate('users.userType.staff'), value: UserType.Staff },
    { label: this.transloco.translate('users.userType.student'), value: UserType.Student },
    { label: this.transloco.translate('users.userType.parent'), value: UserType.Parent },
  ]);

  readonly audienceRoles = computed(() =>
    this.allRoles()
      .filter(r => r.userType === this.userType())
      .slice()
      .sort((a, b) => (a.name ?? '').localeCompare(b.name ?? '')),
  );

  readonly readOnly = computed(() => this.isSystem() || !this.canEditUsers());
  readonly isValid = computed(() => {
    if (this.username().trim().length === 0) return false;
    if (this.isCreate() && this.password().trim().length === 0) return false;
    return true;
  });

  readonly title = computed(() => {
    if (this.isCreate()) return this.transloco.translate('users.form.titleCreate');
    return this.username() || this.transloco.translate('users.form.titleEdit');
  });

  readonly headerActions = computed<HeaderAction[]>(() => {
    if (this.loading()) return [];

    const back: HeaderAction = {
      label: this.transloco.translate('common.back'),
      icon: 'fa-solid fa-arrow-left',
      severity: 'secondary',
      text: true,
      command: () => this.goBack(),
    };

    if (this.notFound() || this.readOnly()) return [back];

    const actions: HeaderAction[] = [back];
    if (!this.isCreate()) {
      actions.push({
        label: this.transloco.translate('users.form.setPassword'),
        icon: 'fa-solid fa-key',
        severity: 'secondary',
        outlined: true,
        command: () => this.openSetPassword(),
      });
    }
    actions.push({
      label: this.isCreate() ? this.transloco.translate('users.form.create') : this.transloco.translate('users.form.save'),
      icon: 'fa-solid fa-check',
      disabled: !this.isCreate() && !this.isDirty(),
      loading: this.saving(),
      command: () => this.save(),
    });
    return actions;
  });

  private readonly snapshot = signal<string | null>(null);
  private readonly currentForm = computed(() =>
    JSON.stringify({
      username: this.username(),
      email: this.email(),
      password: this.password(),
      userType: this.userType(),
      isEnabled: this.isEnabled(),
      personId: this.personId(),
      roles: [...this.selectedRoleIds()].sort(),
    }),
  );
  readonly isDirty = computed(() => this.snapshot() !== null && this.snapshot() !== this.currentForm());

  ngOnInit(): void {
    this.me.me().subscribe(me => {
      this.canEditUsers.set(me.permissions?.includes(Permissions.SystemAdmin.EditUsers) ?? false);
    });

    this.isCreate.set(this.route.snapshot.data['create'] === true);
    this.userId = this.route.snapshot.paramMap.get('id') ?? '';
    this.load();
  }

  onUserTypeChange(userType: UserType): void {
    this.userType.set(userType);
    const valid = new Set(this.audienceRoles().map(r => r.id));
    this.selectedRoleIds.update(set => new Set([...set].filter(id => valid.has(id))));
  }

  onPersonPicked(person: PersonSearchResponse): void {
    this.personId.set(person.personId);
    const first = person.preferredFirstName ?? person.firstName;
    const last = person.preferredLastName ?? person.lastName;
    this.personName.set(`${first} ${last}`.trim());
  }

  clearPerson(): void {
    this.personId.set(null);
    this.personName.set(null);
  }

  isRoleSelected(roleId: string): boolean {
    return this.selectedRoleIds().has(roleId);
  }

  toggleRole(roleId: string, checked: boolean): void {
    this.selectedRoleIds.update(set => {
      const next = new Set(set);
      if (checked) next.add(roleId);
      else next.delete(roleId);
      return next;
    });
  }

  save(): void {
    if (this.readOnly() || this.saving() || (!this.isCreate() && !this.isDirty())) return;
    if (!this.isValid()) {
      this.form()?.form.markAllAsTouched();
      focusFirstInvalid(this.host.nativeElement);
      return;
    }
    this.saving.set(true);

    if (this.isCreate()) {
      const payload: UserUpsertRequest = {
        personId: this.personId(),
        userType: this.userType(),
        isEnabled: this.isEnabled(),
        username: this.username().trim(),
        email: this.email().trim() || null,
        password: this.password(),
        roleIds: [...this.selectedRoleIds()],
      };
      this.usersData.create(payload).subscribe({
        next: res => {
          this.saving.set(false);
          this.snapshot.set(this.currentForm());
          this.notify.success(this.transloco.translate('users.form.createdToast'));
          void this.router.navigate(['/staff/system/users', res.id]);
        },
        error: err => {
          this.saving.set(false);
          this.notify.apiError(err, this.transloco.translate('users.form.errorCreate'));
        },
      });
      return;
    }

    const update: UserUpdateRequest = {
      personId: this.personId(),
      userType: this.userType(),
      isEnabled: this.isEnabled(),
      username: this.username().trim(),
      email: this.email().trim() || null,
      roleIds: [...this.selectedRoleIds()],
    };
    this.usersData.update(this.userId, update).subscribe({
      next: () => {
        this.saving.set(false);
        this.snapshot.set(this.currentForm());
        this.notify.success(this.transloco.translate('users.form.updatedToast'));
        this.loadEffectivePermissions();
      },
      error: err => {
        this.saving.set(false);
        this.notify.apiError(err, this.transloco.translate('users.form.errorUpdate'));
      },
    });
  }

  openSetPassword(): void {
    this.setPasswordOpen.set(true);
  }

  closeSetPassword(): void {
    this.setPasswordOpen.set(false);
  }

  goBack(): void {
    void this.router.navigate(['/staff/system/users']);
  }

  canDeactivate(): boolean | Promise<boolean> {
    if (!this.isDirty()) return true;
    return this.confirm.confirm({
      header: this.transloco.translate('common.discardChanges'),
      message: this.transloco.translate('common.discardConfirm'),
      acceptLabel: this.transloco.translate('common.discard'),
      acceptSeverity: 'danger',
    });
  }

  @HostListener('window:beforeunload', ['$event'])
  onBeforeUnload(event: BeforeUnloadEvent): void {
    if (this.isDirty()) {
      event.preventDefault();
      event.returnValue = '';
    }
  }

  private load(): void {
    this.loading.set(true);
    this.rolesData.all().subscribe({
      next: page => {
        this.allRoles.set(page?.items ?? []);
        if (this.isCreate()) {
          this.loading.set(false);
          this.snapshot.set(this.currentForm());
        } else {
          this.loadUser();
        }
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('users.form.errorLoad'));
      },
    });
  }

  private loadUser(): void {
    this.usersData.getById(this.userId).subscribe({
      next: user => {
        this.username.set(user.username ?? '');
        this.email.set(user.email ?? '');
        this.userType.set(user.userType);
        this.isEnabled.set(user.isEnabled);
        this.personId.set(user.personId);
        this.personName.set(user.personFullName);
        this.selectedRoleIds.set(new Set(user.roleIds));
        this.isSystem.set(user.isSystem);
        this.createdAt.set(user.createdAt);
        this.loading.set(false);
        this.snapshot.set(this.currentForm());
        this.loadEffectivePermissions();
      },
      error: err => {
        this.loading.set(false);
        this.notFound.set(true);
        this.notify.apiError(err, this.transloco.translate('users.form.errorLoad'));
      },
    });
  }

  private loadEffectivePermissions(): void {
    this.rolesData.permissions(this.userType()).subscribe({
      next: catalogue => {
        this.effectiveTreeNodes.set(buildPermissionTree(catalogue ?? []));
        this.usersData.effectivePermissions(this.userId).subscribe({
          next: eff => this.effectiveSelectedIds.set(new Set((eff ?? []).map(p => p.id))),
          error: () => this.effectiveSelectedIds.set(new Set()),
        });
      },
      error: () => this.effectiveTreeNodes.set([]),
    });
  }
}
