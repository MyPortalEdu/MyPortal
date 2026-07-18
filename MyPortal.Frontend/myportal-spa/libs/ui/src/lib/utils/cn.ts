import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

/**
 * Merge conditional class lists and de-conflict Tailwind utilities so the last
 * one wins (e.g. a caller's `px-6` overrides a variant's `px-3`). The standard
 * shadcn/Spartan `cn` helper — the backbone of every `libs/ui` component.
 */
export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs));
}
