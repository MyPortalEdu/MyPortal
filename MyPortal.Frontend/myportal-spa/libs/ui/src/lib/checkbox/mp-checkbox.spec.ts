import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { MpCheckbox } from './mp-checkbox';

// Host exercises the real forms integration: [ngModel] + (ngModelChange), the pattern the app uses.
@Component({
  standalone: true,
  imports: [MpCheckbox, FormsModule],
  template: `<mp-checkbox [ngModel]="value()" (ngModelChange)="value.set($event)" [disabled]="disabled()" />`,
})
class Host {
  readonly value = signal(false);
  readonly disabled = signal(false);
}

describe('MpCheckbox', () => {
  let fixture: ComponentFixture<Host>;
  let host: Host;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    fixture = TestBed.createComponent(Host);
    host = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  function input(): HTMLInputElement {
    return fixture.nativeElement.querySelector('input[type="checkbox"]');
  }

  it('reflects the model value onto the native input (writeValue)', async () => {
    host.value.set(true);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(input().checked).toBeTrue();
  });

  it('writes back to the model when toggled (onChange)', () => {
    const el = input();
    el.checked = true;
    el.dispatchEvent(new Event('change'));
    fixture.detectChanges();
    expect(host.value()).toBeTrue();
  });

  it('disables the native input via the [disabled] input', async () => {
    host.disabled.set(true);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(input().disabled).toBeTrue();
  });
});
