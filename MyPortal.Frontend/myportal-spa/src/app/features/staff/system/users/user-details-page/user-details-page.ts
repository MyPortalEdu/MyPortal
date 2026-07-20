import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  OnInit,
  computed,
  effect,
  inject,
  signal,
  untracked,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FormField, applyWhen, disabled, form, required, submit, validate } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { DatePipe } from '@angular/common';
import { MpButton, MpCard, MpCheckbox, MpFormField, MpInput, MpBadge, MpSelect, MpSkeleton } from '@myportal/ui';
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

interface UserFormModel {
  username: string;
  email: string;
  password: string;
  userType: UserType;
  isEnabled: boolean;
}

@Component({
  selector: 'mp-user-details-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    FormField,
    DatePipe,
    MpButton,
    MpCard,
    MpInput,
    MpCheckbox,
    MpFormField,
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

  private readonly canEditUsers = signal(false);

  readonly isCreate = signal(false);
  protected userId = '';

  protected readonly model = signal<UserFormModel>({
    username: '',
    email: '',
    password: '',
    userType: UserType.Staff,
    isEnabled: true,
  });
  protected readonly f = form(this.model, path => {
    disabled(path, () => this.readOnly());
    required(path.username);
    validate(path.username, ({ value }) =>
      value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
    );
    applyWhen(path, () => this.isCreate(), createPath => {
      required(createPath.password);
      validate(createPath.password, ({ value }) =>
        value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
      );
    });
  });

  readonly personId = signal<string | null>(null);
  readonly personName = signal<string | null>(null);
  readonly selectedRoleIds = signal<ReadonlySet<string>>(new Set());
  readonly allRoles = signal<RoleSummaryResponse[]>([]);
  readonly isSystem = signal(false);
  readonly createdAt = signal<string | null>(null);

  readonly effectiveTreeNodes = signal<PermissionTreeNode[]>([]);
  readonly effectiveSelectedIds = signal<ReadonlySet<string>>(new Set());

  readonly loading = signal(true);
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
      .filter(r => r.userType === this.model().userType)
      .slice()
      .sort((a, b) => (a.name ?? '').localeCompare(b.name ?? '')),
  );

  readonly readOnly = computed(() => this.isSystem() || !this.canEditUsers());

  readonly title = computed(() => {
    if (this.isCreate()) return this.transloco.translate('users.form.titleCreate');
    return this.model().username || this.transloco.translate('users.form.titleEdit');
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
      loading: this.f().submitting(),
      command: () => this.save(),
    });
    return actions;
  });

  private readonly snapshot = signal<string | null>(null);
  private readonly currentForm = computed(() => {
    const m = this.model();
    return JSON.stringify({
      username: m.username,
      email: m.email,
      password: m.password,
      userType: m.userType,
      isEnabled: m.isEnabled,
      personId: this.personId(),
      roles: [...this.selectedRoleIds()].sort(),
    });
  });
  readonly isDirty = computed(() => this.snapshot() !== null && this.snapshot() !== this.currentForm());

  constructor() {
    effect(() => {
      const type = this.model().userType;
      untracked(() => {
        const valid = new Set(this.allRoles().filter(r => r.userType === type).map(r => r.id));
        this.selectedRoleIds.update(set => {
          const next = new Set([...set].filter(id => valid.has(id)));
          return next.size === set.size ? set : next;
        });
      });
    });
  }

  ngOnInit(): void {
    this.me.me().subscribe(me => {
      this.canEditUsers.set(me.permissions?.includes(Permissions.SystemAdmin.EditUsers) ?? false);
    });

    this.isCreate.set(this.route.snapshot.data['create'] === true);
    this.userId = this.route.snapshot.paramMap.get('id') ?? '';
    this.load();
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

  save(): Promise<boolean> | void {
    if (this.readOnly() || (!this.isCreate() && !this.isDirty())) return;
    return submit(this.f, {
      action: async () => {
        const m = this.model();
        const roleIds = [...this.selectedRoleIds()].filter(id =>
          this.audienceRoles().some(r => r.id === id),
        );

        if (this.isCreate()) {
          const payload: UserUpsertRequest = {
            personId: this.personId(),
            userType: m.userType,
            isEnabled: m.isEnabled,
            username: m.username.trim(),
            email: m.email.trim() || null,
            password: m.password,
            roleIds,
          };
          let res: { id: string };
          try {
            res = await firstValueFrom(this.usersData.create(payload));
          } catch (err) {
            this.notify.apiError(err, this.transloco.translate('users.form.errorCreate'));
            return;
          }
          this.snapshot.set(this.currentForm());
          this.notify.success(this.transloco.translate('users.form.createdToast'));
          await this.router.navigate(['/staff/system/users', res.id]);
          return;
        }

        const update: UserUpdateRequest = {
          personId: this.personId(),
          userType: m.userType,
          isEnabled: m.isEnabled,
          username: m.username.trim(),
          email: m.email.trim() || null,
          roleIds,
        };
        try {
          await firstValueFrom(this.usersData.update(this.userId, update));
        } catch (err) {
          this.notify.apiError(err, this.transloco.translate('users.form.errorUpdate'));
          return;
        }
        this.snapshot.set(this.currentForm());
        this.notify.success(this.transloco.translate('users.form.updatedToast'));
        this.loadEffectivePermissions();
      },
      onInvalid: () => focusFirstInvalid(this.host.nativeElement),
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
        this.model.set({
          username: user.username ?? '',
          email: user.email ?? '',
          password: '',
          userType: user.userType,
          isEnabled: user.isEnabled,
        });
        this.personId.set(user.personId);
        this.personName.set(user.personFullName);
        this.selectedRoleIds.set(new Set(user.roleIds));
        this.isSystem.set(user.isSystem);
        this.createdAt.set(user.createdAt);
        this.f().reset();
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
    this.rolesData.permissions(this.model().userType).subscribe({
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
