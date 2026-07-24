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
import {
  FormField,
  form,
  max,
  maxLength,
  min,
  required,
  submit,
  validate,
} from '@angular/forms/signals';
import {
  MpButton,
  MpCheckbox,
  MpDialog,
  MpDialogFooter,
  MpFormField,
  MpInput,
  MpInputNumber,
  MpSelect,
} from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { NotificationService } from '../../../../../core/services/notification.service';
import { ServiceTermsDataService } from '../../../../../shared/services/service-terms-data.service';
import { LookupResponse } from '../../../../../shared/types/lookup';
import {
  ServiceTermResponse,
  ServiceTermUpsertRequest,
} from '../../../../../shared/types/staff-setup';

const MONTH_KEYS = [
  'january', 'february', 'march', 'april', 'may', 'june',
  'july', 'august', 'september', 'october', 'november', 'december',
];

interface ServiceTermForm {
  code: string;
  description: string;
  active: boolean;
  isTeacher: boolean;
  salaried: boolean;
  spinalProgression: boolean;
  termTimeOnlyPossible: boolean;
  incrementMonth: number | null;
  incrementDay: number | null;
  hoursPerWeek: number | null;
  weeksPerYear: number | null;
}

function blank(): ServiceTermForm {
  return {
    code: '',
    description: '',
    active: true,
    isTeacher: false,
    salaried: true,
    spinalProgression: false,
    termTimeOnlyPossible: false,
    incrementMonth: null,
    incrementDay: null,
    hoursPerWeek: null,
    weeksPerYear: null,
  };
}

@Component({
  selector: 'mp-service-term-editor-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    FormField,
    MpButton,
    MpCheckbox,
    MpDialog,
    MpDialogFooter,
    MpFormField,
    MpInput,
    MpInputNumber,
    MpSelect,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-setup')],
  templateUrl: './service-term-editor-dialog.html',
})
export class ServiceTermEditorDialog {
  private readonly data = inject(ServiceTermsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly open = input(false);
  readonly serviceTerm = input<ServiceTermResponse | null>(null);
  readonly schemes = input<LookupResponse[]>([]);

  readonly closed = output<void>();
  readonly saved = output<void>();

  protected readonly isEdit = computed(() => this.serviceTerm() != null);

  // Scheme membership is button-driven, so it lives outside the form and feeds a snapshot-based
  // dirty check rather than f().dirty().
  protected readonly selectedSchemeIds = signal<string[]>([]);
  protected readonly mainSchemeId = signal<string | null>(null);

  protected readonly monthOptions = computed(() =>
    MONTH_KEYS.map((key, i) => ({
      value: i + 1,
      label: this.transloco.translate(`staff-setup.months.${key}`),
    })),
  );

  protected readonly model = signal<ServiceTermForm>(blank());
  protected readonly f = form(this.model, path => {
    required(path.code);
    maxLength(path.code, 16);
    required(path.description);
    maxLength(path.description, 256);

    min(path.hoursPerWeek, 0);
    max(path.hoursPerWeek, 168);
    min(path.weeksPerYear, 0);
    max(path.weeksPerYear, 52.14);
    validate(path.incrementMonth, ({ value, valueOf }) =>
      !valueOf(path.spinalProgression) || value() != null
        ? undefined
        : { kind: 'incrementRequired', message: 'staff-setup.serviceTerms.incrementRequired' },
    );
  });

  constructor() {
    effect(() => {
      if (!this.open()) return;
      const row = this.serviceTerm();

      this.model.set(
        row
          ? {
              code: row.code,
              description: row.description,
              active: row.active,
              isTeacher: row.isTeacher,
              salaried: row.salaried,
              spinalProgression: row.spinalProgression,
              termTimeOnlyPossible: row.termTimeOnlyPossible,
              incrementMonth: row.incrementMonth ?? null,
              incrementDay: row.incrementDay ?? null,
              hoursPerWeek: row.hoursPerWeek ?? null,
              weeksPerYear: row.weeksPerYear ?? null,
            }
          : blank(),
      );

      this.selectedSchemeIds.set(row?.superannuationSchemes.map(s => s.superannuationSchemeId) ?? []);
      this.mainSchemeId.set(row?.superannuationSchemes.find(s => s.isMain)?.superannuationSchemeId ?? null);
      this.f().reset();
    });

    // Turning spinal progression off clears the increment timing it governs.
    effect(() => {
      if (this.model().spinalProgression) return;
      untracked(() =>
        this.model.update(m =>
          m.incrementMonth == null && m.incrementDay == null
            ? m
            : { ...m, incrementMonth: null, incrementDay: null },
        ),
      );
    });
  }

  protected isSchemeSelected(id: string): boolean {
    return this.selectedSchemeIds().includes(id);
  }

  protected toggleScheme(id: string, selected: boolean): void {
    this.selectedSchemeIds.update(ids =>
      selected ? [...new Set([...ids, id])] : ids.filter(x => x !== id),
    );

    // A scheme that is no longer offered cannot remain the default.
    if (!selected && this.mainSchemeId() === id) {
      this.mainSchemeId.set(null);
    }
  }

  protected setMain(id: string): void {
    this.mainSchemeId.update(current => (current === id ? null : id));
  }

  protected onClose(): void {
    this.closed.emit();
  }

  protected save(): Promise<boolean> {
    return submit(this.f, async () => {
      const m = this.model();
      const main = this.mainSchemeId();

      const payload: ServiceTermUpsertRequest = {
        code: m.code.trim().toUpperCase(),
        description: m.description.trim(),
        active: m.active,
        isTeacher: m.isTeacher,
        salaried: m.salaried,
        spinalProgression: m.spinalProgression,
        termTimeOnlyPossible: m.termTimeOnlyPossible,
        incrementMonth: m.spinalProgression ? m.incrementMonth : null,
        incrementDay: m.spinalProgression ? m.incrementDay : null,
        hoursPerWeek: m.hoursPerWeek,
        weeksPerYear: m.weeksPerYear,
        superannuationSchemes: this.selectedSchemeIds().map(id => ({
          superannuationSchemeId: id,
          isMain: id === main,
        })),
      };

      const existing = this.serviceTerm();

      try {
        if (existing) {
          await firstValueFrom(this.data.update(existing.id, payload));
        } else {
          await firstValueFrom(this.data.create(payload));
        }
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('staff-setup.serviceTerms.saveError'));
        return;
      }

      this.notify.success(this.transloco.translate('staff-setup.serviceTerms.savedToast'));
      this.saved.emit();
    });
  }
}
