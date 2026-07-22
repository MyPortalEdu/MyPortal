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
import { DatePipe } from '@angular/common';
import { FormField, applyEach, form, maxLength, required, submit, validate } from '@angular/forms/signals';
import { MpButton, MpDatePicker, MpTextarea } from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StaffMembersDataService } from '../../../../../../shared/services/staff-members-data.service';
import { LookupSelect } from '../../../../../../shared/components/lookup-select/lookup-select';
import { Loading } from '../../../../../../shared/components/loading/loading';
import { EmptyState } from '../../../../../../shared/components/empty-state/empty-state';
import { Field } from '../../../../../../shared/components/field/field';
import {
  StaffResponsibilitiesResponse,
  StaffResponsibilitiesUpsertRequest,
} from '../../../../../../shared/types/staff-responsibilities';
import { StaffAreaPanel } from './staff-area-panel';

interface ResponsibilityFormRow {
  id: string | null;
  responsibilityTypeId: string | null;
  startDate: Date | null;
  endDate: Date | null;
  notes: string;
}

@Component({
  selector: 'mp-staff-responsibilities-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    FormField,
    MpButton,
    MpDatePicker,
    MpTextarea,
    LookupSelect,
    Loading,
    EmptyState,
    Field,
    TranslocoDirective,
  ],
  providers: [
    provideTranslocoScope('staff-members'),
    { provide: StaffAreaPanel, useExisting: forwardRef(() => StaffResponsibilitiesPanel) },
  ],
  templateUrl: './staff-responsibilities-panel.html',
})
export class StaffResponsibilitiesPanel extends StaffAreaPanel implements OnInit {
  private readonly data = inject(StaffMembersDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly staffMemberId = input.required<string>();
  readonly permissions = input.required<Set<string>>();

  protected readonly loading = signal(false);
  override readonly editing = signal(false);

  protected readonly area = signal<StaffResponsibilitiesResponse | null>(null);
  protected readonly model = signal<{ responsibilities: ResponsibilityFormRow[] }>({ responsibilities: [] });
  protected readonly f = form(this.model, path => {
    applyEach(path.responsibilities, item => {
      required(item.responsibilityTypeId);
      required(item.startDate);
      maxLength(item.notes, 256);
      validate(item.endDate, ({ value, valueOf }) => {
        const end = value();
        const start = valueOf(item.startDate);
        return !end || !start || end.getTime() >= start.getTime()
          ? undefined
          : { kind: 'endBeforeStart', message: 'staff-members.responsibilities.endBeforeStart' };
      });
    });
  });
  private readonly snapshot = signal<string>('');

  protected readonly responsibilityTypes = computed(() => this.area()?.responsibilityTypes ?? []);

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Staff.EditAllStaffResponsibilities),
  );

  override readonly saving = computed(() => this.f().submitting());
  override readonly valid = computed(() => this.f().valid());

  private readonly formJson = computed(() => JSON.stringify(this.model()));

  override readonly dirty = computed(
    () => this.area() != null && this.snapshot() !== this.formJson(),
  );

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getResponsibilities(this.staffMemberId()).subscribe({
      next: row => {
        this.area.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.responsibilities.loadError'));
      },
    });
  }

  override startEdit(): void {
    this.editing.set(true);
  }

  override cancel(): void {
    this.apply(this.area());
    this.editing.set(false);
  }

  override async save(): Promise<void> {
    if (!this.canEdit() || !this.dirty()) return;
    await submit(this.f, async () => {
      const payload: StaffResponsibilitiesUpsertRequest = {
        responsibilities: this.model().responsibilities.map(r => ({
          id: r.id,
          responsibilityTypeId: r.responsibilityTypeId,
          startDate: r.startDate?.toISOString() ?? null,
          endDate: r.endDate?.toISOString() ?? null,
          notes: this.normalise(r.notes),
        })),
      };
      try {
        await firstValueFrom(this.data.updateResponsibilities(this.staffMemberId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('staff-members.responsibilities.saveError'));
        return;
      }
      this.notify.success(this.transloco.translate('staff-members.responsibilities.savedToast'));
      this.editing.set(false);
      this.load();
    });
  }

  private apply(row: StaffResponsibilitiesResponse | null): void {
    this.model.set({
      responsibilities: (row?.responsibilities ?? []).map(r => ({
        id: r.id,
        responsibilityTypeId: r.responsibilityTypeId,
        startDate: r.startDate ? new Date(r.startDate) : null,
        endDate: r.endDate ? new Date(r.endDate) : null,
        notes: r.notes ?? '',
      })),
    });
    this.f().reset();
    this.snapshot.set(this.formJson());
  }

  protected addResponsibility(): void {
    this.model.update(m => ({
      responsibilities: [
        ...m.responsibilities,
        { id: null, responsibilityTypeId: null, startDate: null, endDate: null, notes: '' },
      ],
    }));
  }

  protected removeResponsibility(index: number): void {
    this.model.update(m => ({ responsibilities: m.responsibilities.filter((_, i) => i !== index) }));
  }

  protected isCurrent(r: ResponsibilityFormRow): boolean {
    return !r.endDate || r.endDate.getTime() >= Date.now();
  }
}
