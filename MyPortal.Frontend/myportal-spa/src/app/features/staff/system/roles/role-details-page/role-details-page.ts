import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormField, disabled, form, required, submit, validate } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { MpCard, MpFormField, MpInput, MpTextarea, MpBadge, MpSkeleton } from '@myportal/ui';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { Field } from '../../../../../shared/components/field/field';
import { SectionHeader } from '../../../../../shared/components/section-header/section-header';
import { EmptyState } from '../../../../../shared/components/empty-state/empty-state';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { RolesDataService } from '../../../../../shared/services/roles-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { MeService } from '../../../../../core/services/me-service';
import { Permissions } from '../../../../../core/constants/permissions';
import { CanComponentDeactivate } from '../../../../../core/guards/can-deactivate.guard';
import { RoleUpsertRequest } from '../../../../../shared/types/role';
import { UserType } from '../../../../../core/types/user-type';
import { PermissionTree, PermissionTreeNode, buildPermissionTree } from '../permission-tree/permission-tree';
import { focusFirstInvalid } from '../../../../../shared/utils/focus-first-invalid';

@Component({
  selector: 'mp-role-details-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormField,
    MpCard,
    MpFormField,
    MpInput,
    MpTextarea,
    MpBadge,
    MpSkeleton,
    PageHeader,
    Field,
    SectionHeader,
    EmptyState,
    PermissionTree,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('roles')],
  templateUrl: './role-details-page.html',
})
export class RoleDetailsPage implements OnInit, CanComponentDeactivate {
  private readonly data = inject(RolesDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly me = inject(MeService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly host = inject(ElementRef<HTMLElement>);

  private roleId = '';

  private readonly canEditRoles = signal(false);

  protected readonly model = signal({ name: '', description: '' });
  protected readonly f = form(this.model, path => {
    disabled(path.name, () => this.nameLocked());
    disabled(path.description, () => this.readOnly());
    required(path.name);
    validate(path.name, ({ value }) =>
      value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
    );
  });

  readonly userType = signal<UserType>(UserType.Staff);
  readonly selectedPermissionIds = signal<ReadonlySet<string>>(new Set());
  readonly treeNodes = signal<PermissionTreeNode[]>([]);
  readonly isSystem = signal(false);
  readonly isDefault = signal(false);
  readonly loading = signal(true);
  readonly notFound = signal(false);

  readonly skeletonRows = [1, 2, 3, 4, 5, 6];

  readonly readOnly = computed(() => this.isSystem() || !this.canEditRoles());
  readonly nameLocked = computed(() => this.readOnly() || this.isDefault());

  readonly audienceLabel = computed(() =>
    this.transloco.translate('roles.audience.' + UserType[this.userType()].toLowerCase()),
  );

  private readonly snapshot = signal<string | null>(null);
  private readonly currentForm = computed(() => {
    const m = this.model();
    return JSON.stringify({
      name: m.name,
      description: m.description,
      perms: [...this.selectedPermissionIds()].sort(),
    });
  });
  readonly isDirty = computed(() => this.snapshot() !== null && this.snapshot() !== this.currentForm());

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

    return [
      back,
      {
        label: this.transloco.translate('roles.form.save'),
        icon: 'fa-solid fa-check',
        disabled: !this.isDirty(),
        loading: this.f().submitting(),
        command: () => this.save(),
      },
    ];
  });

  ngOnInit(): void {
    this.me.me().subscribe(me => {
      this.canEditRoles.set(me.permissions?.includes(Permissions.SystemAdmin.EditRoles) ?? false);
    });

    this.roleId = this.route.snapshot.paramMap.get('id') ?? '';
    this.load();
  }

  onToggle(change: { ids: string[]; checked: boolean }): void {
    if (this.readOnly()) return;
    this.selectedPermissionIds.update(set => {
      const next = new Set(set);
      for (const id of change.ids) {
        if (change.checked) next.add(id);
        else next.delete(id);
      }
      return next;
    });
  }

  save(): Promise<boolean> | void {
    if (this.readOnly() || !this.isDirty()) return;
    return submit(this.f, {
      action: async () => {
        const m = this.model();
        const payload: RoleUpsertRequest = {
          name: m.name.trim(),
          description: m.description.trim() || null,
          userType: this.userType(),
          permissionIds: [...this.selectedPermissionIds()],
        };
        try {
          await firstValueFrom(this.data.update(this.roleId, payload));
        } catch (err) {
          this.notify.apiError(err, this.transloco.translate('roles.form.errorUpdate'));
          return;
        }
        this.snapshot.set(this.currentForm());
        this.notify.success(this.transloco.translate('roles.form.updatedToast'));
      },
      onInvalid: () => focusFirstInvalid(this.host.nativeElement),
    });
  }

  goBack(): void {
    void this.router.navigate(['/staff/system/roles']);
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
    this.data.getById(this.roleId).subscribe({
      next: role => {
        this.model.set({ name: role.name ?? '', description: role.description ?? '' });
        this.f().reset();
        this.userType.set(role.userType);
        this.selectedPermissionIds.set(new Set(role.permissionIds));
        this.isSystem.set(role.isSystem);
        this.isDefault.set(role.isDefault);

        this.data.permissions(role.userType).subscribe({
          next: perms => {
            this.treeNodes.set(buildPermissionTree(perms ?? []));
            this.loading.set(false);
            this.snapshot.set(this.currentForm());
          },
          error: err => {
            this.loading.set(false);
            this.snapshot.set(this.currentForm());
            this.notify.apiError(err, this.transloco.translate('roles.form.errorLoad'));
          },
        });
      },
      error: err => {
        this.loading.set(false);
        this.notFound.set(true);
        this.notify.apiError(err, this.transloco.translate('roles.form.errorLoad'));
      },
    });
  }
}
