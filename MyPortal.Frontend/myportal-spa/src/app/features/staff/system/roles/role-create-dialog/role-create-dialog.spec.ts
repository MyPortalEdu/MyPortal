import { TestBed } from '@angular/core/testing';
import { TranslocoService } from '@jsverse/transloco';

import { RoleCreateDialog } from './role-create-dialog';
import { RolesDataService } from '../../../../../shared/services/roles-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { UserType } from '../../../../../core/types/user-type';
import { createSpyObj } from '@testing/spy';

interface Internals {
  model: { set(v: { name: string; userType: UserType }): void };
  f: {
    (): { valid(): boolean; invalid(): boolean };
    name: () => { value(): string; touched(): boolean; errors(): ReadonlyArray<{ kind: string }> };
  };
}

function make(): Internals {
  TestBed.configureTestingModule({
    imports: [RoleCreateDialog],
    providers: [
      { provide: RolesDataService, useValue: createSpyObj<RolesDataService>(['create']) },
      { provide: NotificationService, useValue: createSpyObj<NotificationService>(['success', 'apiError']) },
      { provide: ConfirmationDialog, useValue: createSpyObj<ConfirmationDialog>(['confirm']) },
      { provide: TranslocoService, useValue: { translate: (k: string) => k } },
    ],
  });
  const fixture = TestBed.createComponent(RoleCreateDialog);
  fixture.componentRef.setInput('visible', false);
  return fixture.componentInstance as unknown as Internals;
}

describe('RoleCreateDialog Signal Forms', () => {
  it('required(name) drives form validity off the signal model', () => {
    const c = make();
    expect(c.f().invalid()).toBe(true);
    c.model.set({ name: 'Head of Year', userType: UserType.Staff });
    expect(c.f().valid()).toBe(true);
    expect(c.f.name().value()).toBe('Head of Year');
  });

  it('field starts untouched so the error is not surfaced on load', () => {
    const c = make();
    expect(c.f().invalid()).toBe(true);
    expect(c.f.name().touched()).toBe(false);
  });

  it('custom validate() rejects a whitespace-only name (beyond required)', () => {
    const c = make();
    c.model.set({ name: '   ', userType: UserType.Staff });
    expect(c.f().invalid()).toBe(true);
    expect(c.f.name().errors().some(e => e.kind === 'blank')).toBe(true);
  });

  it('goes invalid again when the required field is cleared', () => {
    const c = make();
    c.model.set({ name: 'x', userType: UserType.Student });
    expect(c.f().valid()).toBe(true);
    c.model.set({ name: '', userType: UserType.Student });
    expect(c.f().invalid()).toBe(true);
  });
});
