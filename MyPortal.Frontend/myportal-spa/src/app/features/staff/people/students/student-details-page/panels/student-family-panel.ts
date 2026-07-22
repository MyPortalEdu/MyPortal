import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  forwardRef,
  inject,
  input,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { MpBadge, MpButton } from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../../core/services/confirmation.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StudentsDataService } from '../../../../../../shared/services/students-data.service';
import { Loading } from '../../../../../../shared/components/loading/loading';
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import {
  StudentContactRelationshipResponse,
  StudentFamilyResponse,
} from '../../../../../../shared/types/student-family';
import { StudentAreaPanel } from './student-area-panel';
import { StudentFamilyRelationshipDialog } from './student-family-relationship-dialog';

@Component({
  selector: 'mp-student-family-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink,
    MpBadge,
    MpButton,
    Loading,
    SectionHeader,
    StudentFamilyRelationshipDialog,
    TranslocoDirective,
  ],
  providers: [
    provideTranslocoScope('students'),
    { provide: StudentAreaPanel, useExisting: forwardRef(() => StudentFamilyPanel) },
  ],
  templateUrl: './student-family-panel.html',
})
export class StudentFamilyPanel extends StudentAreaPanel implements OnInit {
  private readonly data = inject(StudentsDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  readonly studentId = input.required<string>();
  readonly permissions = input.required<Set<string>>();

  override readonly selfManaged = true;
  override readonly editing = signal(false);
  override readonly dirty = signal(false);
  override readonly valid = signal(true);
  override readonly saving = signal(false);

  protected readonly loading = signal(false);
  protected readonly family = signal<StudentFamilyResponse | null>(null);
  protected readonly dialogOpen = signal(false);
  protected readonly editTarget = signal<StudentContactRelationshipResponse | null>(null);

  protected readonly contacts = computed(() => this.family()?.contacts ?? []);
  protected readonly siblings = computed(() => this.family()?.siblings ?? []);
  protected readonly relationshipTypes = computed(() => this.family()?.relationshipTypes ?? []);

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Student.EditStudentFamily),
  );

  override startEdit(): void {}
  override cancel(): void {}
  override async save(): Promise<void> {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getFamily(this.studentId()).subscribe({
      next: row => {
        this.family.set(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('students.family.loadError'));
      },
    });
  }

  protected openAdd(): void {
    this.editTarget.set(null);
    this.dialogOpen.set(true);
  }

  protected openEdit(relationship: StudentContactRelationshipResponse): void {
    this.editTarget.set(relationship);
    this.dialogOpen.set(true);
  }

  protected onDialogClosed(): void {
    this.dialogOpen.set(false);
  }

  protected onSaved(): void {
    this.dialogOpen.set(false);
    this.load();
  }

  protected async remove(relationship: StudentContactRelationshipResponse): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('students.family.removeHeader'),
      message: this.transloco.translate('students.family.removeConfirm', {
        name: relationship.contactName,
      }),
      acceptLabel: this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });
    if (!ok) return;

    try {
      await firstValueFrom(
        this.data.removeContactRelationship(this.studentId(), relationship.id),
      );
      this.notify.success(this.transloco.translate('students.family.removedToast'));
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.family.removeError'));
    }
  }
}
