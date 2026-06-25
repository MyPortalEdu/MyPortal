// Gender is persisted on Person (shared by staff, students, contacts/agents…)
// as a single-character code. The UI shows a friendly label via the
// `common.gender.*` i18n keys — see GenderSelect and the genderLabel pipe.
// Extend this list if MyPortal needs to record more codes; consumers fall back
// to the raw code for anything not listed, so legacy values still render.
export const GENDER_CODES = ['M', 'F', 'X'] as const;

export type GenderCode = (typeof GENDER_CODES)[number];
