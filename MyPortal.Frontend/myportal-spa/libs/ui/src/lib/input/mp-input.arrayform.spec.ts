import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormField, applyEach, form, required, validate } from '@angular/forms/signals';

import { MpInput } from './mp-input';

interface Item {
  title: string;
}

@Component({
  imports: [FormField, MpInput],
  template: `
    @for (item of f.items; track $index) {
      <input mpInput [formField]="item.title" />
    }
  `,
})
class Host {
  readonly model = signal<{ items: Item[] }>({ items: [{ title: '' }, { title: 'ok' }] });
  readonly f = form(this.model, p => {
    applyEach(p.items, item => {
      required(item.title);
      validate(item.title, ({ value }) =>
        value().trim().length ? undefined : { kind: 'blank', message: 'blank' },
      );
    });
  });

  add(): void {
    this.model.update(m => ({ items: [...m.items, { title: '' }] }));
  }

  removeAt(index: number): void {
    this.model.update(m => ({ items: m.items.filter((_, i) => i !== index) }));
  }
}

describe('Signal Forms array binding (applyEach) probe', () => {
  function setup() {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    return fixture;
  }

  it('renders one control per array item and binds each item field', () => {
    const fixture = setup();
    const inputs = fixture.nativeElement.querySelectorAll('input');
    expect(inputs.length).toBe(2);
  });

  it('validates each item independently via applyEach', () => {
    const fixture = setup();
    const host = fixture.componentInstance;
    expect(host.f().invalid()).toBe(true);
    expect(host.f.items[0]().invalid()).toBe(true);
    expect(host.f.items[1]().invalid()).toBe(false);
  });

  it('becomes valid once the empty item is filled', () => {
    const fixture = setup();
    const host = fixture.componentInstance;
    host.model.update(m => ({ items: m.items.map((it, i) => (i === 0 ? { title: 'now set' } : it)) }));
    fixture.detectChanges();
    expect(host.f().valid()).toBe(true);
  });

  it('tracks items added and removed from the model', () => {
    const fixture = setup();
    const host = fixture.componentInstance;

    host.add();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('input').length).toBe(3);
    expect(host.f.items[2]().invalid()).toBe(true);

    host.removeAt(0);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('input').length).toBe(2);
  });
});
