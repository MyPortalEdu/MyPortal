import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Dialog } from 'primeng/dialog';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { Select } from 'primeng/select';
import { Checkbox } from 'primeng/checkbox';

import { BulletinsDataService } from '../../../services/bulletins-data.service';
import { NotificationService } from '../../../services/notification.service';
import { MeService } from '../../../../core/services/me-service';
import {
  BulletinAllowedGroupResponse,
  BulletinAudienceKind,
  BulletinAudienceRequest,
  BulletinAudienceResponse,
  BulletinCategoryResponse,
  BulletinDetailsResponse,
  BulletinUpsertRequest,
} from '../../../types/bulletin';

interface AudienceChoice {
  key: string;            // dedup key
  label: string;
  kind: BulletinAudienceKind;
  studentGroupId?: string;
}

@Component({
  selector: 'mp-bulletin-form-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FormsModule, Dialog, Button, InputText, Textarea, Select, Checkbox],
  templateUrl: './bulletin-form-dialog.html',
})
export class BulletinFormDialog implements OnChanges {
  private readonly data = inject(BulletinsDataService);
  private readonly meService = inject(MeService);
  private readonly notify = inject(NotificationService);

  @Input({ required: true }) visible = false;

  /**
   * When set, the dialog opens in edit mode: fields pre-fill from this bulletin,
   * the submit calls update() instead of create(), and ExpectedVersion is sent
   * so optimistic concurrency catches racing edits. Leave null for create mode.
   */
  @Input() existing: BulletinDetailsResponse | null = null;

  @Output() readonly closed = new EventEmitter<void>();
  @Output() readonly saved = new EventEmitter<void>();

  readonly title = signal('');
  readonly detail = signal('');
  readonly categoryId = signal<string | null>(null);
  readonly isPinned = signal(false);
  readonly requiresAck = signal(false);
  readonly selectedAudienceKeys = signal<Set<string>>(new Set());
  readonly submitting = signal(false);
  readonly canPin = signal(false);
  // Reflected from @Input so computed()s can react. ngOnChanges keeps it in sync.
  private readonly _existing = signal<BulletinDetailsResponse | null>(null);
  // Categories live inside the dialog now rather than as an @Input. With OnPush
  // and the parent's signal-fed binding, the Select could latch onto an empty
  // array on first render and the "No results found" message stayed visible.
  // Owning the fetch here means the options array is always live when the panel
  // opens — and the shared shareReplay() in the data service avoids a second
  // round-trip when the feed has already loaded them.
  readonly categories = signal<BulletinCategoryResponse[]>([]);
  readonly allowedGroups = signal<BulletinAllowedGroupResponse[]>([]);

  readonly isEdit = computed(() => this._existing() !== null);

  readonly audienceChoices = computed<AudienceChoice[]>(() => {
    // Fixed audiences come first so they read like a sentence: "all staff / all
    // pupils / all parents", then any allowlisted student groups.
    const choices: AudienceChoice[] = [
      { key: 'all-staff',   label: 'All staff',   kind: BulletinAudienceKind.AllStaff },
      { key: 'all-pupils',  label: 'All pupils',  kind: BulletinAudienceKind.AllPupils },
      { key: 'all-parents', label: 'All parents', kind: BulletinAudienceKind.AllParents },
    ];

    // De-dup student-group entries by id: the allowlist is the canonical source,
    // but when editing we also surface any group the bulletin currently targets
    // even if the admin has since removed it from the allowlist — otherwise the
    // editor couldn't see (or untick) it.
    const groups = new Map<string, AudienceChoice>();
    for (const g of this.allowedGroups()) {
      groups.set(g.studentGroupId, {
        key: `sg-${g.studentGroupId}`,
        label: g.name,
        kind: BulletinAudienceKind.StudentGroup,
        studentGroupId: g.studentGroupId,
      });
    }
    const existing = this._existing();
    if (existing) {
      for (const a of existing.audiences) {
        if (a.audienceKind === BulletinAudienceKind.StudentGroup
            && a.studentGroupId
            && !groups.has(a.studentGroupId)) {
          groups.set(a.studentGroupId, {
            key: `sg-${a.studentGroupId}`,
            label: a.studentGroupName ?? 'Group',
            kind: BulletinAudienceKind.StudentGroup,
            studentGroupId: a.studentGroupId,
          });
        }
      }
    }

    return [...choices, ...groups.values()];
  });

  readonly isValid = computed(() =>
    this.title().trim().length > 0 &&
    this.detail().trim().length > 0 &&
    this.categoryId() !== null &&
    this.selectedAudienceKeys().size > 0,
  );

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['existing']) {
      this._existing.set(this.existing);
    }
    if (changes['visible'] && this.visible) {
      this.reset();
      this.loadDependencies();
    }
  }

  isSelected(key: string): boolean {
    return this.selectedAudienceKeys().has(key);
  }

  toggleAudience(key: string): void {
    this.selectedAudienceKeys.update(set => {
      const next = new Set(set);
      if (next.has(key)) next.delete(key);
      else next.add(key);
      return next;
    });
  }

  onCancel(): void {
    this.closed.emit();
  }

  onHide(): void {
    // p-dialog emits onHide on backdrop / esc / X. Bubble up so the parent's
    // `visible` input goes false.
    this.closed.emit();
  }

  publish(): void {
    if (!this.isValid() || this.submitting()) return;
    this.submitting.set(true);

    const audiences: BulletinAudienceRequest[] = this.audienceChoices()
      .filter(c => this.selectedAudienceKeys().has(c.key))
      .map(c => ({
        audienceKind: c.kind,
        studentGroupId: c.studentGroupId ?? null,
      }));

    const existing = this._existing();
    const payload: BulletinUpsertRequest = {
      title: this.title().trim(),
      detail: this.detail().trim(),
      categoryId: this.categoryId()!,
      isPinned: this.isPinned(),
      requiresAcknowledgement: this.requiresAck(),
      audiences,
      // We don't expose expiry in the form yet, so on edit we preserve the
      // current value rather than clobbering it to null.
      expiresAt: existing?.expiresAt ?? null,
      // ExpectedVersion is irrelevant on create (server ignores it on POST)
      // but required on update for the optimistic-concurrency guard.
      expectedVersion: existing?.version ?? 0,
    };

    const isEdit = existing !== null;
    const onSuccess = () => {
      this.submitting.set(false);
      this.notify.success(isEdit ? 'Bulletin updated' : 'Bulletin published');
      this.saved.emit();
    };
    const onError = () => {
      this.submitting.set(false);
      this.notify.error(
        isEdit ? "Couldn't save changes" : "Couldn't publish bulletin",
        'Something went wrong. Check the details and try again.',
      );
    };

    if (existing) {
      this.data.update(existing.id, payload).subscribe({ next: onSuccess, error: onError });
    } else {
      this.data.create(payload).subscribe({ next: onSuccess, error: onError });
    }
  }

  private reset(): void {
    const existing = this._existing();
    if (existing) {
      // Edit mode: hydrate every field from the bulletin we're editing.
      this.title.set(existing.title);
      this.detail.set(existing.detail);
      this.categoryId.set(existing.categoryId);
      this.isPinned.set(existing.pinnedAt !== null);
      this.requiresAck.set(existing.requiresAcknowledgement);
      this.selectedAudienceKeys.set(audienceKeysFor(existing.audiences));
      return;
    }
    // Create mode.
    this.title.set('');
    this.detail.set('');
    this.isPinned.set(false);
    this.requiresAck.set(false);
    // Default audience: All staff. Matches the mockup's pre-selected chip.
    this.selectedAudienceKeys.set(new Set(['all-staff']));
    // categoryId default is set after the categories call resolves (see loadDependencies);
    // setting it here from a possibly-empty cache would leave the Select blank.
    this.categoryId.set(null);
  }

  private loadDependencies(): void {
    // Check pin permission once per open. Cheap — MeService caches.
    this.meService.me().subscribe(me => {
      this.canPin.set(!!me.permissions?.includes('School.PinSchoolBulletins'));
    });

    // Categories: the data service shareReplays this, so the feed's earlier call
    // serves the dialog without an extra round-trip. We still resubscribe here
    // because we own the dialog's category default (create mode only — edit
    // already has a categoryId set from the existing bulletin).
    this.data.listCategories(false).subscribe({
      next: cats => {
        this.categories.set(cats ?? []);
        if (cats?.length && this.categoryId() === null) {
          this.categoryId.set(cats[0].id);
        }
      },
      error: () => this.categories.set([]),
    });

    // Fetch the allowlist on every open in case the admin changed it since last
    // render. One extra request per dialog open — acceptable.
    this.data.getSettings().subscribe({
      next: s => this.allowedGroups.set(s.allowedAudienceGroups ?? []),
      error: () => this.allowedGroups.set([]),
    });
  }
}

function audienceKeysFor(audiences: BulletinAudienceResponse[]): Set<string> {
  const keys = new Set<string>();
  for (const a of audiences) {
    switch (a.audienceKind) {
      case BulletinAudienceKind.AllStaff:   keys.add('all-staff');   break;
      case BulletinAudienceKind.AllPupils:  keys.add('all-pupils');  break;
      case BulletinAudienceKind.AllParents: keys.add('all-parents'); break;
      case BulletinAudienceKind.StudentGroup:
        if (a.studentGroupId) keys.add(`sg-${a.studentGroupId}`);
        break;
    }
  }
  return keys;
}
