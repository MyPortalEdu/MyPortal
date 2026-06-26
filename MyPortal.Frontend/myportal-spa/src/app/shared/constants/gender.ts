// Gender is persisted on Person (shared by staff, students, contacts/agents…)
// as a single-character code aligned to the DfE CBDS Sex code set (CS119):
// M = Male, F = Female, U = Unknown. The UI shows a friendly label via the
// `common.gender.*` i18n keys — see GenderSelect and the genderLabel pipe.
// Note: the school census itself only accepts M/F — U is for incomplete records
// (e.g. pre-admission) and must be resolved before a statutory return.
// Consumers fall back to the raw code for anything not listed, so legacy values
// still render.
export const GENDER_CODES = ['M', 'F', 'U'] as const;

export type GenderCode = (typeof GENDER_CODES)[number];
