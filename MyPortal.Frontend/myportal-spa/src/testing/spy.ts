import { vi, type Mock } from 'vitest';

export type SpyObj<T> = {
  [K in keyof T]: T[K] extends (...args: infer A) => infer R ? Mock<(...args: A) => R> : T[K];
};

export function createSpyObj<T>(methods: Array<keyof T & string>): SpyObj<T> {
  const obj = {} as Record<string, unknown>;
  for (const m of methods) obj[m] = vi.fn();
  return obj as SpyObj<T>;
}
