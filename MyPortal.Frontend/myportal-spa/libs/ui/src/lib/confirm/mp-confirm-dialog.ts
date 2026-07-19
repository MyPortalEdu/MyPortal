import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MpButton } from '../button/mp-button';
import { MpDialog } from '../dialog/mp-dialog';
import { MpConfirmStore } from './mp-confirm-store';

/**
 * Confirm-dialog host — the design-system equivalent of `<p-confirmDialog>`. Mount ONCE at the app
 * root; it renders whatever the ConfirmationDialog service pushes into MpConfirmStore, as an
 * MpDialog. Backdrop/escape dismiss resolves the prompt as rejected.
 */
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
