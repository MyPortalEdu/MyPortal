import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { Tag } from 'primeng/tag';
import { Skeleton } from 'primeng/skeleton';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { RolesDataService } from '../../../../../shared/services/roles-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { CanComponentDeactivate } from '../../../../../core/guards/can-deactivate.guard';
import { RoleUpsertRequest } from '../../../../../shared/types/role';
import { UserType } from '../../../../../core/types/user-type';
import { PermissionTree, PermissionTreeNode, buildPermissionTree } from '../permission-tree/permission-tree';

@Component({
  selector: 'mp-role-details-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    Button,
    Card,
    InputText,
    Textarea,
    Tag,
    Skeleton,
    PageHeader,
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
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private roleId = '';

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

  readonly readOnly = computed(() => this.isSystem());
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

  ngOnInit(): void {
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
    if (this.readOnly() || !this.isValid() || this.saving() || !this.isDirty()) return;
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
