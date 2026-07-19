import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MpDialog, MpDialogFooter } from './mp-dialog';

@Component({
  standalone: true,
  imports: [MpDialog, MpDialogFooter],
  template: `
    <mp-dialog
      [visible]="open()"
      (visibleChange)="open.set($event)"
      [title]="title()"
      [titleIcon]="titleIcon()"
      [showClose]="showClose()"
      [closeDisabled]="closeDisabled()"
      (closed)="closedCount = closedCount + 1">
      <p class="projected">Body content</p>
      @if (withFooter()) {
        <div mpDialogFooter class="footer-content">Footer</div>
      }
    </mp-dialog>
  `,
})
class HostComponent {
  readonly open = signal(true);
  readonly title = signal<string | undefined>('My Title');
  readonly titleIcon = signal<string | undefined>(undefined);
  readonly showClose = signal(true);
  readonly closeDisabled = signal(false);
  readonly withFooter = signal(false);
  closedCount = 0;
}

describe('MpDialog', () => {
  let fixture: ComponentFixture<HostComponent>;
  let host: HostComponent;

  beforeEach(() => {
    fixture = TestBed.createComponent(HostComponent);
    host = fixture.componentInstance;
  });

  function query(sel: string): HTMLElement | null {
    return fixture.nativeElement.querySelector(sel);
  }

  it('framed mode renders a header with the title and a close button, plus projected body', () => {
    fixture.detectChanges();
    const header = query('h3');
    expect(header?.textContent).toContain('My Title');
    expect(query('button[aria-label="Close"]')).toBeTruthy();
    expect(query('.projected')).toBeTruthy();
  });

  it('the close button routes through dismiss(): closes AND emits (closed)', () => {
    fixture.detectChanges();
    query('button[aria-label="Close"]')!.click();
    fixture.detectChanges();
    expect(host.open()).toBe(false);
    expect(host.closedCount).toBe(1);
  });

  it('showClose=false hides the close button but keeps the header', () => {
    host.showClose.set(false);
    fixture.detectChanges();
    expect(query('button[aria-label="Close"]')).toBeNull();
    expect(query('h3')?.textContent).toContain('My Title');
  });

  it('headless mode (no title) renders no header, only projected content', () => {
    host.title.set(undefined);
    fixture.detectChanges();
    expect(query('h3')).toBeNull();
    expect(query('.projected')).toBeTruthy();
  });

  it('renders titleIcon before the title when provided', () => {
    host.titleIcon.set('fa-solid fa-bell');
    fixture.detectChanges();
    expect(query('i.fa-bell')).toBeTruthy();
  });

  it('closeDisabled disables the ✕ so it cannot dismiss', () => {
    host.closeDisabled.set(true);
    fixture.detectChanges();
    const btn = query('button[aria-label="Close"]') as HTMLButtonElement;
    expect(btn.disabled).toBeTrue();
    btn.click();
    fixture.detectChanges();
    expect(host.open()).toBeTrue();
    expect(host.closedCount).toBe(0);
  });

  it('projects [mpDialogFooter] content into a footer strip only when present', () => {
    fixture.detectChanges();
    expect(query('.footer-content')).toBeNull();
    host.withFooter.set(true);
    fixture.detectChanges();
    const footer = query('.footer-content');
    expect(footer).toBeTruthy();
    expect(footer!.closest('.border-t')).toBeTruthy();
  });
});
