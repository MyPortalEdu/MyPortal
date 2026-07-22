import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  forwardRef,
  inject,
  input,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { MpBadge } from '@myportal/ui';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { ContactsDataService } from '../../../../../../shared/services/contacts-data.service';
import { Loading } from '../../../../../../shared/components/loading/loading';
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { ContactStudentResponse } from '../../../../../../shared/types/contact-student';
import { ContactAreaPanel } from './contact-area-panel';

@Component({
  selector: 'mp-contact-associated-students-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, MpBadge, Loading, SectionHeader, TranslocoDirective],
  providers: [
    provideTranslocoScope('contacts'),
    { provide: ContactAreaPanel, useExisting: forwardRef(() => ContactAssociatedStudentsPanel) },
  ],
  templateUrl: './contact-associated-students-panel.html',
})
export class ContactAssociatedStudentsPanel extends ContactAreaPanel implements OnInit {
  private readonly data = inject(ContactsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly contactId = input.required<string>();
  readonly permissions = input.required<Set<string>>();

  override readonly editing = signal(false);
  override readonly dirty = signal(false);
  override readonly valid = signal(true);
  override readonly canEdit = signal(false);
  override readonly saving = signal(false);

  protected readonly loading = signal(false);
  protected readonly loaded = signal(false);
  protected readonly students = signal<ContactStudentResponse[]>([]);

  override startEdit(): void {}
  override cancel(): void {}
  override async save(): Promise<void> {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getAssociatedStudents(this.contactId()).subscribe({
      next: rows => {
        this.students.set(rows);
        this.loaded.set(true);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('contacts.associatedStudents.loadError'));
      },
    });
  }
}
