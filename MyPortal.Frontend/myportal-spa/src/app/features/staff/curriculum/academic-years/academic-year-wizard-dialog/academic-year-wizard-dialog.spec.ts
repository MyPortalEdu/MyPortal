import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj } from '@testing/spy';

import { AcademicYearWizardDialog } from './academic-year-wizard-dialog';
import { AcademicYearsDataService } from '../../../../../shared/services/academic-years-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { AcademicYearService } from '../../../../../core/services/academic-year-service';
import { AcademicYearUpsertRequest } from '../../../../../shared/types/academic-year';

function d(iso: string): Date {
  return new Date(iso);
}

function baseModel(): AcademicYearUpsertRequest {
  return {
    timetableCycleLength: 5,
    schoolWeekLength: 5,
    firstWeekOffset: 0,
    copyPeriodsFromAcademicYearId: null,
    copyPastoralStructureFromAcademicYearId: null,
    academicTerms: [
      { academicTermId: null, name: 'Autumn', startDate: d('2026-09-01'), endDate: d('2026-12-18') },
      { academicTermId: null, name: 'Spring', startDate: d('2027-01-05'), endDate: d('2027-03-31') },
    ],
    attendancePeriods: [],
    schoolHolidays: [],
  } as unknown as AcademicYearUpsertRequest;
}

describe('AcademicYearWizardDialog validation', () => {
  function make() {
    TestBed.configureTestingModule({
      providers: [
        { provide: AcademicYearsDataService, useValue: createSpyObj<AcademicYearsDataService>(['getById', 'create', 'update']) },
        { provide: NotificationService, useValue: createSpyObj<NotificationService>(['success', 'apiError']) },
        { provide: ConfirmationDialog, useValue: createSpyObj<ConfirmationDialog>(['confirm']) },
        { provide: AcademicYearService, useValue: createSpyObj<AcademicYearService>(['clearCache']) },
        { provide: TranslocoService, useValue: { translate: (k: string) => k, getActiveLang: () => 'en', selectTranslate: () => of('') } },
      ],
    });
    TestBed.overrideComponent(AcademicYearWizardDialog, { set: { template: '' } });
    const fixture = TestBed.createComponent(AcademicYearWizardDialog);
    fixture.componentRef.setInput('visible', false);
    return fixture.componentInstance;
  }

  it('setup step rejects overlapping academic terms', () => {
    const c = make();
    c.currentStep.set(0);
    c.model.set(baseModel());
    expect(c.termsOverlap()).toBe(false);
    expect(c.canAdvance()).toBe(true);

    c.model.update(m => ({
      ...m,
      academicTerms: [
        { academicTermId: null, name: 'Autumn', startDate: d('2026-09-01'), endDate: d('2026-12-18') },
        { academicTermId: null, name: 'Spring', startDate: d('2026-12-10'), endDate: d('2027-03-31') },
      ],
    }));
    expect(c.termsOverlap()).toBe(true);
    expect(c.canAdvance()).toBe(false);
  });

  it('holidays step rejects a holiday outside the term span', () => {
    const c = make();
    c.currentStep.set(2);
    c.model.set({
      ...baseModel(),
      schoolHolidays: [
        { schoolHolidayId: null, name: 'Half term', type: 0, startDate: d('2026-10-26'), endDate: d('2026-10-30') },
      ],
    } as unknown as AcademicYearUpsertRequest);
    expect(c.holidaysOutOfSpan()).toBe(false);
    expect(c.canAdvance()).toBe(true);

    c.model.update(m => ({
      ...m,
      schoolHolidays: [
        { schoolHolidayId: null, name: 'Summer', type: 0, startDate: d('2027-08-01'), endDate: d('2027-08-20') },
      ],
    }));
    expect(c.holidaysOutOfSpan()).toBe(true);
    expect(c.canAdvance()).toBe(false);
  });
});
