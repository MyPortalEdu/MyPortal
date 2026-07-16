import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  untracked,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Button } from 'primeng/button';
import { Checkbox } from 'primeng/checkbox';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { Select } from 'primeng/select';
import { Tag } from 'primeng/tag';
import { Skeleton } from 'primeng/skeleton';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { RolesDataService } from '../../../../../shared/services/roles-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { RoleUpsertRequest, PermissionResponse } from '../../../../../shared/types/role';
import { UserType } from '../../../../../core/types/user-type';

type FormSnapshot = { name: string; description: string; userType: UserType; permissionIds: string };

interface AudienceOption {
  label: string;
  value: UserType;
}

interface PermissionGroup {
  area: string;
  permissions: PermissionResponse[];
}

@Component({
  selector: 'mp-role-form-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, Button, Checkbox, Dialog, InputText, Textarea, Select, Tag, Skeleton, TranslocoDirective, TranslocoPipe],
  providers: [provideTranslocoScope('roles')],
  templateUrl: './role-form-dialog.html',
})
export class RoleFormDialog {
  private readonly data = inject(RolesDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly confirmDialog = inject(ConfirmationDialog);

  readonly visible = input.required<boolean>();
  // Non-null = edit an existing role; null = create.
  readonly editRoleId = input<string | null>(null);

  readonly closed = output<void>();
  readonly saved = output<void>();

  readonly name = signal('');
  readonly description = signal('');
  readonly userType = signal<UserType>(UserType.Staff);
  readonly selectedPermissionIds = signal<ReadonlySet<string>>(new Set());
  readonly permissions = signal<PermissionResponse[]>([]);
  readonly isSystem = signal(false);
  readonly isDefault = signal(false);

  readonly loading = signal(false);
  readonly submitting = signal(false);
  readonly touchedFields = signal<ReadonlySet<string>>(new Set());

  readonly audienceOptions = computed<AudienceOption[]>(() => [
    { label: this.transloco.translate('roles.audience.staff'), value: UserType.Staff },
    { label: this.transloco.translate('roles.audience.student'), value: UserType.Student },
    { label: this.transloco.translate('roles.audience.parent'), value: UserType.Parent },
  ]);

  readonly isEdit = computed(() => this.editRoleId() !== null);
  // System roles are fully locked; the dialog opens read-only to view their permissions.
  readonly readOnly = computed(() => this.isSystem());
  // Default (built-in) roles can't be renamed, but their grants stay editable.
  readonly nameLocked = computed(() => this.readOnly() || this.isDefault());
  readonly isValid = computed(() => this.name().trim().length > 0);

  readonly dialogTitle = computed(() => {
    if (!this.isEdit()) return this.transloco.translate('roles.form.titleCreate');
    return this.readOnly()
      ? this.transloco.translate('roles.form.titleView')
      : this.transloco.translate('roles.form.titleEdit');
  });

  readonly audienceLabel = computed(() =>
    this.transloco.translate('roles.audience.' + UserType[this.userType()].toLowerCase()),
  );

  readonly groupedPermissions = computed<PermissionGroup[]>(() => {
    const groups = new Map<string, PermissionResponse[]>();
    for (const p of this.permissions()) {
      const list = groups.get(p.area) ?? [];
      list.push(p);
      groups.set(p.area, list);
    }
    return [...groups.entries()]
      .map(([area, permissions]) => ({
        area,
        permissions: permissions.slice().sort((a, b) => a.friendlyName.localeCompare(b.friendlyName)),
      }))
      .sort((a, b) => a.area.localeCompare(b.area));
  });

  private readonly snapshot = signal<FormSnapshot | null>(null);

  private readonly currentForm = computed<FormSnapshot>(() => ({
    name: this.name(),
    description: this.description(),
    userType: this.userType(),
    permissionIds: [...this.selectedPermissionIds()].sort().join(','),
  }));

  readonly isDirty = computed(() => {
    const s = this.snapshot();
    if (!s) return false;
    return JSON.stringify(s) !== JSON.stringify(this.currentForm());
  });

  constructor() {
    effect(() => {
      if (this.visible()) {
        untracked(() => this.open());
      }
    });
  }

  markTouched(field: string): void {
    if (this.touchedFields().has(field)) return;
    this.touchedFields.update(s => new Set(s).add(field));
  }

  wasTouched(field: string): boolean {
    return this.touchedFields().has(field);
  }

  isSelected(permissionId: string): boolean {
    return this.selectedPermissionIds().has(permissionId);
  }

  togglePermission(permissionId: string, checked: boolean): void {
    if (this.readOnly()) return;
    this.selectedPermissionIds.update(set => {
      const next = new Set(set);
      if (checked) next.add(permissionId);
      else next.delete(permissionId);
      return next;
    });
  }

  onAudienceChange(userType: UserType): void {
    this.userType.set(userType);
    // Permissions are audience-specific — clear the selection and reload the catalogue.
    this.selectedPermissionIds.set(new Set());
    this.loadPermissions(userType);
  }

  async onCancel(): Promise<void> {
    await this.requestClose();
  }

  onHide(): void {
    // PrimeNG fires onHide both on user-close and when the parent flips `visible` to false in
    // response to our own closed/saved events. The dirty prompt lives in onCancel; just keep the
    // parent in sync here.
    this.closed.emit();
  }

  save(): void {
    if (this.readOnly() || !this.isValid() || this.submitting()) return;
    this.submitting.set(true);

    const payload: RoleUpsertRequest = {
      name: this.name().trim(),
      description: this.description().trim() || null,
      userType: this.userType(),
      permissionIds: [...this.selectedPermissionIds()],
    };

    const id = this.editRoleId();
    const t = (key: string) => this.transloco.translate(`roles.form.${key}`);
    const onSuccess = () => {
      this.submitting.set(false);
      // Re-baseline before emitting saved so the parent-driven close doesn't see isDirty=true.
      this.snapshot.set(this.currentForm());
      this.notify.success(t(id ? 'updatedToast' : 'createdToast'));
      this.saved.emit();
    };
    const onError = (err: unknown) => {
      this.submitting.set(false);
      this.notify.apiError(err, t(id ? 'errorUpdate' : 'errorCreate'));
    };

    if (id) this.data.update(id, payload).subscribe({ next: onSuccess, error: onError });
    else this.data.create(payload).subscribe({ next: onSuccess, error: onError });
  }

  private open(): void {
    this.touchedFields.set(new Set());
    this.submitting.set(false);
    const id = this.editRoleId();
    if (id) this.loadForEdit(id);
    else this.resetForCreate();
  }

  private resetForCreate(): void {
    this.name.set('');
    this.description.set('');
    this.userType.set(UserType.Staff);
    this.selectedPermissionIds.set(new Set());
    this.isSystem.set(false);
    this.isDefault.set(false);
    this.loading.set(true);
    this.data.permissions(UserType.Staff).subscribe({
      next: perms => {
        this.permissions.set(perms ?? []);
        this.loading.set(false);
        this.snapshot.set(this.currentForm());
      },
      error: err => {
        this.loading.set(false);
        this.snapshot.set(this.currentForm());
        this.notify.apiError(err, this.transloco.translate('roles.form.errorLoad'));
      },
    });
  }

  private loadForEdit(id: string): void {
    this.loading.set(true);
    this.data.getById(id).subscribe({
      next: role => {
        this.name.set(role.name ?? '');
        this.description.set(role.description ?? '');
        this.userType.set(role.userType);
        this.selectedPermissionIds.set(new Set(role.permissionIds));
        this.isSystem.set(role.isSystem);
        this.isDefault.set(role.isDefault);
        // The catalogue depends on the audience we just learned, so it's a second, sequential fetch.
        this.data.permissions(role.userType).subscribe({
          next: perms => {
            this.permissions.set(perms ?? []);
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
        this.notify.apiError(err, this.transloco.translate('roles.form.errorLoad'));
      },
    });
  }

  private loadPermissions(userType: UserType): void {
    this.loading.set(true);
    this.data.permissions(userType).subscribe({
      next: perms => {
        this.permissions.set(perms ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('roles.form.errorLoad'));
      },
    });
  }

  private async requestClose(): Promise<void> {
    if (this.isDirty()) {
      const ok = await this.confirmDialog.confirm({
        header: this.transloco.translate('common.discardChanges'),
        message: this.transloco.translate('common.discardConfirm'),
        acceptLabel: this.transloco.translate('common.discard'),
        acceptSeverity: 'danger',
      });
      if (!ok) return;
    }
    this.closed.emit();
  }
}
