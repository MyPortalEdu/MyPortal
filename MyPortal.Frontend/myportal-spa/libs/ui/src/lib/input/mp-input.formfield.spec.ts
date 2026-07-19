import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormField, form, maxLength, required } from '@angular/forms/signals';
import { MpInput } from './mp-input';

@Component({
  standalone: true,
  imports: [FormField, MpInput],
  template: `<input mpInput [formField]="f.name" />`,
})
class Host {
  readonly model = signal({ name: '' });
  readonly f = form(this.model, p => {
    required(p.name);
    maxLength(p.name, 5);
  });
}

describe('MpInput + Signal Forms FormField wiring', () => {
  let fixture: ComponentFixture<Host>;
  function input(): HTMLInputElement {
    return fixture.nativeElement.querySelector('input');
  }

  beforeEach(async () => {
    fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('FormField pushes touched-gated invalid into MpInput (aria-invalid)', async () => {
    expect(input().getAttribute('aria-invalid')).toBeNull();
    input().dispatchEvent(new Event('blur'));
    fixture.detectChanges();
    await fixture.whenStable();
    expect(input().getAttribute('aria-invalid')).toBe('true');
  });

  it('FormField forwards the schema maxLength constraint onto the control', () => {
    expect(input().getAttribute('maxlength')).toBe('5');
  });
});
