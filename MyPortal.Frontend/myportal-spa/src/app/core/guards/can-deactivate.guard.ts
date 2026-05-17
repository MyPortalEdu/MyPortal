import { CanDeactivateFn } from '@angular/router';

/**
 * Implemented by components that own unsaved state. The guard delegates the
 * decision to the component so each page can word its own confirm prompt.
 */
export interface CanComponentDeactivate {
  canDeactivate(): boolean | Promise<boolean>;
}

export const canDeactivateGuard: CanDeactivateFn<CanComponentDeactivate> =
  (component) => component?.canDeactivate ? component.canDeactivate() : true;
