export const Permissions = {
  // Object key matches the C# `Permissions.SystemAdmin` class name (which avoids the `System`
  // namespace collision); the wire-string values keep the "System." prefix to match the
  // server's permission rows.
  SystemAdmin: {
    ViewUsers:           'System.ViewUsers',
    EditUsers:           'System.EditUsers',
    ViewGroups:          'System.ViewGroups',
    EditGroups:          'System.EditGroups',
    BulletinSettings:    'System.BulletinSettings'
  }
} as const;

// Flatten to a union of all permission string literals across every category.
// Distributes over keys so adding a second category yields a union, not `never`.
export type Permission = {
  [C in keyof typeof Permissions]: (typeof Permissions)[C][keyof (typeof Permissions)[C]]
}[keyof typeof Permissions];

// Convenience: a flat readonly array if you ever need it
export const ALL_PERMISSIONS = Object.freeze(
  Object.values(Permissions).flatMap(cat => Object.values(cat))
) as readonly Permission[];
