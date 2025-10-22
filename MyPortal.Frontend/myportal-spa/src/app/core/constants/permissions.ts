export const Permissions = {
  System: {
    ViewUsers:           'System.ViewUsers',
    EditUsers:           'System.EditUsers',
    ViewGroups:          'System.ViewGroups',
    EditGroups:          'System.EditGroups'
  }
} as const;

// Flatten to a union type of all permission strings
type Category = typeof Permissions[keyof typeof Permissions];
export type Permission = Category[keyof Category];

// Convenience: a flat readonly array if you ever need it
export const ALL_PERMISSIONS = Object.freeze(
  Object.values(Permissions).flatMap(cat => Object.values(cat))
) as readonly Permission[];
