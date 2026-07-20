export const GENDER_CODES = ['M', 'F', 'U'] as const;

export type GenderCode = (typeof GENDER_CODES)[number];
