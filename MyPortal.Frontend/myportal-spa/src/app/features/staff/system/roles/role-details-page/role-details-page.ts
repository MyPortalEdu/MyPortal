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
import { MpCard, MpInput, MpTextarea, MpBadge, MpSkeleton } from '@myportal/ui';
import {
  TranslocoDirective,
  TranslocoPipe,
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
    FormsModule,
    MpCard,
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
    TranslocoPipe,
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

  private readonly form = viewChild<NgForm>('f');

  private roleId = '';

  // The route admits ViewRoles holders too, so the page decides its own mode.
  private readonly canEditRoles = signal(false);

  readonly name = signal('');
  readonly description = signal('');
  readonly userType = signal<UserType>(UserType.Staff);
  readonly selectedPermissionIds = signal<ReadonlySet<string>>(new Set());
  readonly treeNodes = signal<PermissionTreeNode[]>([]);
  readonly isSystem = signal(false);
  readonly isDefault = signal(false);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly notFound = signal(false);

  readonly skeletonRows = [1, 2, 3, 4, 5, 6];

  readonly readOnly = computed(() => this.isSystem() || !this.canEditRoles());
  readonly nameLocked = computed(() => this.readOnly() || this.isDefault());
  readonly isValid = computed(() => this.name().trim().length > 0);

  readonly audienceLabel = computed(() =>
    this.transloco.translate('roles.audience.' + UserType[this.userType()].toLowerCase()),
  );

  private readonly snapshot = signal<string | null>(null);
  private readonly currentForm = computed(() =>
    JSON.stringify({
      name: this.name(),
      description: this.description(),
      perms: [...this.selectedPermissionIds()].sort(),
    }),
  );
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
        // Only "nothing to save" disables Save; an invalid form lets the click through so save()
        // can mark the fields touched and point at what's missing.
        disabled: !this.isDirty(),
        loading: this.saving(),
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

  save(): void {
    if (this.readOnly() || this.saving() || !this.isDirty()) return;
    if (!this.isValid()) {
      this.form()?.form.markAllAsTouched();
      focusFirstInvalid(this.host.nativeElement);
      return;
    }
    this.saving.set(true);

    const payload: RoleUpsertRequest = {
      name: this.name().trim(),
      description: this.description().trim() || null,
      userType: this.userType(),
      permissionIds: [...this.selectedPermissionIds()],
    };

    this.data.update(this.roleId, payload).subscribe({
      next: () => {
        this.saving.set(false);
        // Re-baseline so leaving the page doesn't re-prompt.
        this.snapshot.set(this.currentForm());
        this.notify.success(this.transloco.translate('roles.form.updatedToast'));
      },
      error: err => {
        this.saving.set(false);
        this.notify.apiError(err, this.transloco.translate('roles.form.errorUpdate'));
      },
    });
  }

  goBack(): void {
    // The canDeactivate guard handles the dirty prompt during navigation.
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
        this.name.set(role.name ?? '');
        this.description.set(role.description ?? '');
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
