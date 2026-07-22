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
import { FormField, form, submit } from '@angular/forms/signals';
import { DatePipe } from '@angular/common';
import { MpDatePicker } from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StudentsDataService } from '../../../../../../shared/services/students-data.service';
import { LookupSelect } from '../../../../../../shared/components/lookup-select/lookup-select';
import { Loading } from '../../../../../../shared/components/loading/loading';
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { Field } from '../../../../../../shared/components/field/field';
import {
  StudentCulturalDetailsResponse,
  StudentCulturalDetailsUpsertRequest,
} from '../../../../../../shared/types/student-cultural';
import { StudentAreaPanel } from './student-area-panel';

interface CulturalModel {
  ethnicityId: string | null;
  firstLanguageId: string | null;
  religionId: string | null;
  nationalityId: string | null;
  englishProficiencyId: string | null;
  englishProficiencyDate: Date | null;
}

@Component({
  selector: 'mp-student-cultural-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField, DatePipe, MpDatePicker, LookupSelect, Loading, SectionHeader, Field, TranslocoDirective],
  providers: [
    provideTranslocoScope('students'),
    { provide: StudentAreaPanel, useExisting: forwardRef(() => StudentCulturalPanel) },
  ],
  templateUrl: './student-cultural-panel.html',
})
export class StudentCulturalPanel extends StudentAreaPanel implements OnInit {
  private readonly data = inject(StudentsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly studentId = input.required<string>();
  readonly permissions = input.required<Set<string>>();

  protected readonly loading = signal(false);
  override readonly editing = signal(false);

  protected readonly cultural = signal<StudentCulturalDetailsResponse | null>(null);

  protected readonly model = signal<CulturalModel>({
    ethnicityId: null,
    firstLanguageId: null,
    religionId: null,
    nationalityId: null,
    englishProficiencyId: null,
    englishProficiencyDate: null,
  });
  protected readonly f = form(this.model);
  private readonly snapshot = signal<string>('');

  protected readonly ethnicities = computed(() => this.cultural()?.ethnicities ?? []);
  protected readonly languages = computed(() => this.cultural()?.languages ?? []);
  protected readonly religions = computed(() => this.cultural()?.religions ?? []);
  protected readonly nationalities = computed(() => this.cultural()?.nationalities ?? []);
  protected readonly englishProficiencies = computed(() => this.cultural()?.englishProficiencies ?? []);

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Student.EditStudentCultural),
  );

  override readonly saving = computed(() => this.f().submitting());
  override readonly valid = computed(() => this.f().valid());

  private readonly formState = computed(() => {
    const m = this.model();
    return JSON.stringify({
      ethnicityId: m.ethnicityId,
      firstLanguageId: m.firstLanguageId,
      religionId: m.religionId,
      nationalityId: m.nationalityId,
      englishProficiencyId: m.englishProficiencyId,
      englishProficiencyDate: m.englishProficiencyDate?.toISOString() ?? null,
    });
  });

  override readonly dirty = computed(
    () => this.cultural() != null && this.snapshot() !== this.formState(),
  );

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getCulturalDetails(this.studentId()).subscribe({
      next: row => {
        this.cultural.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('students.cultural.loadError'));
      },
    });
  }

  override startEdit(): void {
    this.editing.set(true);
  }

  override cancel(): void {
    this.apply(this.cultural());
    this.editing.set(false);
  }

  override async save(): Promise<void> {
    if (!this.canEdit() || !this.dirty()) return;
    await submit(this.f, async () => {
      const m = this.model();
      const payload: StudentCulturalDetailsUpsertRequest = {
        ethnicityId: m.ethnicityId,
        firstLanguageId: m.firstLanguageId,
        religionId: m.religionId,
        nationalityId: m.nationalityId,
        englishProficiencyId: m.englishProficiencyId,
        englishProficiencyDate: m.englishProficiencyDate?.toISOString() ?? null,
      };
      try {
        await firstValueFrom(this.data.updateCulturalDetails(this.studentId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('students.cultural.saveError'));
        return;
      }
      this.notify.success(this.transloco.translate('students.cultural.savedToast'));
      this.editing.set(false);
      this.load();
    });
  }

  private apply(row: StudentCulturalDetailsResponse | null): void {
    this.model.set({
      ethnicityId: row?.ethnicityId ?? null,
      firstLanguageId: row?.firstLanguageId ?? null,
      religionId: row?.religionId ?? null,
      nationalityId: row?.nationalityId ?? null,
      englishProficiencyId: row?.englishProficiencyId ?? null,
      englishProficiencyDate: row?.englishProficiencyDate ? new Date(row.englishProficiencyDate) : null,
    });
    this.f().reset();
    this.snapshot.set(this.formState());
  }
}
