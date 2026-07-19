import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MpButton } from '../button/mp-button';
import { MpDialog } from '../dialog/mp-dialog';
import { MpConfirmStore } from './mp-confirm-store';

@Component({
  selector: 'mp-confirm-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MpDialog, MpButton],
  templateUrl: './mp-confirm-dialog.html',
})
export class MpConfirmDialog {
  protected readonly store = inject(MpConfirmStore);
  protected readonly open = computed(() => this.store.request() !== null);
}
