import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { PermissionResponse } from '../../../../../shared/types/role';

export interface PermissionTreeNode {
  key: string;
  label: string;
  permissionId?: string;
  // Font Awesome glyph, set on top-level area groups only.
  icon?: string;
  // Every descendant permission id (a leaf's is just its own). Drives parent tri-state + subtree toggle.
  leafIds: string[];
  children: PermissionTreeNode[];
}

// Glyph per top-level permission area (keyed by the first Area segment).
const AREA_ICONS: Record<string, string> = {
  Agencies: 'fa-building',
  Attendance: 'fa-clipboard-check',
  Curriculum: 'fa-graduation-cap',
  School: 'fa-school',
  Staff: 'fa-users',
  System: 'fa-gear',
  Timetable: 'fa-calendar-days',
};

// Builds a tree from the permission catalogue by splitting each permission's Area on '.' — e.g.
// "Staff.Absences" nests Staff ▸ Absences, with the permission (FriendlyName) as a leaf underneath.
export function buildPermissionTree(permissions: PermissionResponse[]): PermissionTreeNode[] {
  const root: PermissionTreeNode = { key: '', label: '', leafIds: [], children: [] };

  for (const p of permissions) {
    const segments = (p.area || 'Other').split('.').filter(s => s.length > 0);
    let node = root;
    let path = '';
    for (const seg of segments) {
      const isTopLevel = node === root;
      path = path ? `${path}.${seg}` : seg;
      const groupKey = `group:${path}`;
      let child = node.children.find(c => c.key === groupKey);
      if (!child) {
        child = {
          key: groupKey,
          label: humanise(seg),
          icon: isTopLevel ? AREA_ICONS[seg] : undefined,
          leafIds: [],
          children: [],
        };
        node.children.push(child);
      }
      node = child;
    }
    node.children.push({
      key: `perm:${p.id}`,
      label: p.friendlyName,
      permissionId: p.id,
      leafIds: [p.id],
      children: [],
    });
  }

  const finalise = (n: PermissionTreeNode): void => {
    if (n.permissionId) return;
    n.children.forEach(finalise);
    n.leafIds = n.children.flatMap(c => c.leafIds);
    // Groups before individual permissions, each alphabetical.
    n.children.sort((a, b) => {
      const ag = a.permissionId ? 1 : 0;
      const bg = b.permissionId ? 1 : 0;
      return ag !== bg ? ag - bg : a.label.localeCompare(b.label);
    });
  };
  root.children.forEach(finalise);
  root.children.sort((a, b) => a.label.localeCompare(b.label));
  return root.children;
}

// "BasicDetails" → "Basic Details", "PreEmploymentChecks" → "Pre Employment Checks".
function humanise(segment: string): string {
  return segment.replace(/([a-z0-9])([A-Z])/g, '$1 $2');
}

@Component({
  selector: 'mp-permission-tree',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgTemplateOutlet],
  templateUrl: './permission-tree.html',
})
export class PermissionTree {
  readonly nodes = input.required<PermissionTreeNode[]>();
  readonly selectedIds = input.required<ReadonlySet<string>>();
  readonly disabled = input(false);

  // A subtree (or single leaf) was toggled; the parent owns the selection set and applies it.
  readonly toggle = output<{ ids: string[]; checked: boolean }>();

  private readonly collapsedKeys = signal<ReadonlySet<string>>(new Set());

  isLeaf(node: PermissionTreeNode): boolean {
    return node.permissionId !== undefined;
  }

  isChecked(node: PermissionTreeNode): boolean {
    const sel = this.selectedIds();
    return node.leafIds.length > 0 && node.leafIds.every(id => sel.has(id));
  }

  isIndeterminate(node: PermissionTreeNode): boolean {
    if (this.isLeaf(node)) return false;
    const sel = this.selectedIds();
    return node.leafIds.some(id => sel.has(id)) && !this.isChecked(node);
  }

  isCollapsed(node: PermissionTreeNode): boolean {
    return this.collapsedKeys().has(node.key);
  }

  toggleCollapse(node: PermissionTreeNode): void {
    this.collapsedKeys.update(s => {
      const next = new Set(s);
      if (next.has(node.key)) next.delete(node.key);
      else next.add(node.key);
      return next;
    });
  }

  onToggle(node: PermissionTreeNode, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.toggle.emit({ ids: node.leafIds, checked });
  }
}
