import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ContractAnalysisReportItem,
  ContractInformationReportItem,
  IndividualAbsenceReportItem,
  LongTermAbsenceReportItem,
  ReportOption,
  SalaryInformationReportItem,
  StaffAbsenceAnalysisReportItem,
  StaffTrainingReportItem,
  StaffTypeFilter,
  TerminatingContractReportItem,
  TrainingCourseAttendeeReportItem,
} from '../types/staff-reports';

@Injectable({ providedIn: 'root' })
export class StaffReportsDataService {
  private readonly http = inject(HttpClient);

  /** Salary Information report. effectiveDate is a YYYY-MM-DD string (no time, no UTC shift). */
  getSalaryInformation(
    staffType: StaffTypeFilter,
    effectiveDate: string,
  ): Observable<SalaryInformationReportItem[]> {
    const params = new HttpParams().set('staffType', staffType).set('effectiveDate', effectiveDate);
    return this.http.get<SalaryInformationReportItem[]>(
      '/api/v1/staffreports/salary-information',
      { params },
    );
  }

  /** Contract Information report. */
  getContractInformation(
    staffType: StaffTypeFilter,
    effectiveDate: string,
  ): Observable<ContractInformationReportItem[]> {
    const params = new HttpParams().set('staffType', staffType).set('effectiveDate', effectiveDate);
    return this.http.get<ContractInformationReportItem[]>(
      '/api/v1/staffreports/contract-information',
      { params },
    );
  }

  /** Contract Analysis report (summary by service term). */
  getContractAnalysis(
    staffType: StaffTypeFilter,
    effectiveDate: string,
  ): Observable<ContractAnalysisReportItem[]> {
    const params = new HttpParams().set('staffType', staffType).set('effectiveDate', effectiveDate);
    return this.http.get<ContractAnalysisReportItem[]>(
      '/api/v1/staffreports/contract-analysis',
      { params },
    );
  }

  /** Terminating Contracts report. Dates are YYYY-MM-DD strings. */
  getTerminatingContracts(
    startDate: string,
    endDate: string,
  ): Observable<TerminatingContractReportItem[]> {
    const params = new HttpParams().set('startDate', startDate).set('endDate', endDate);
    return this.http.get<TerminatingContractReportItem[]>(
      '/api/v1/staffreports/terminating-contracts',
      { params },
    );
  }

  /** Individual Absence report for one staff member. */
  getIndividualAbsence(
    staffMemberId: string,
    absenceTypeId: string | null,
    startDate: string,
    endDate: string,
  ): Observable<IndividualAbsenceReportItem[]> {
    let params = new HttpParams()
      .set('staffMemberId', staffMemberId)
      .set('startDate', startDate)
      .set('endDate', endDate);
    if (absenceTypeId) params = params.set('absenceTypeId', absenceTypeId);
    return this.http.get<IndividualAbsenceReportItem[]>(
      '/api/v1/staffreports/individual-absence',
      { params },
    );
  }

  /** Staff Absence Analysis report (summary by service term). */
  getStaffAbsenceAnalysis(
    absenceTypeId: string | null,
    startDate: string,
    endDate: string,
  ): Observable<StaffAbsenceAnalysisReportItem[]> {
    let params = new HttpParams().set('startDate', startDate).set('endDate', endDate);
    if (absenceTypeId) params = params.set('absenceTypeId', absenceTypeId);
    return this.http.get<StaffAbsenceAnalysisReportItem[]>(
      '/api/v1/staffreports/staff-absence-analysis',
      { params },
    );
  }

  /** Long-Term Absence Analysis report. */
  getLongTermAbsence(
    startDate: string,
    endDate: string,
    minDays: number,
  ): Observable<LongTermAbsenceReportItem[]> {
    const params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate)
      .set('minDays', minDays);
    return this.http.get<LongTermAbsenceReportItem[]>('/api/v1/staffreports/long-term-absence', {
      params,
    });
  }

  /** Staff Training report, optionally for one staff member. */
  getStaffTraining(
    staffMemberId: string | null,
    startDate: string,
    endDate: string,
  ): Observable<StaffTrainingReportItem[]> {
    let params = new HttpParams().set('startDate', startDate).set('endDate', endDate);
    if (staffMemberId) params = params.set('staffMemberId', staffMemberId);
    return this.http.get<StaffTrainingReportItem[]>('/api/v1/staffreports/staff-training', {
      params,
    });
  }

  /** Training Course report: attendees for one course. */
  getTrainingCourseAttendees(
    trainingCourseId: string,
  ): Observable<TrainingCourseAttendeeReportItem[]> {
    const params = new HttpParams().set('trainingCourseId', trainingCourseId);
    return this.http.get<TrainingCourseAttendeeReportItem[]>(
      '/api/v1/staffreports/training-course',
      { params },
    );
  }

  /** Active training courses for the Training Course picker. */
  getTrainingCourses(): Observable<ReportOption[]> {
    return this.http.get<ReportOption[]>('/api/v1/staffreports/training-courses');
  }

  /** Absence types for the absence reports' type filter. */
  getAbsenceTypes(): Observable<ReportOption[]> {
    return this.http.get<ReportOption[]>('/api/v1/staffreports/absence-types');
  }
}
