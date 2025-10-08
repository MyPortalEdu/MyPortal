import {NgModule, Optional, SkipSelf} from '@angular/core';
import { CommonModule } from '@angular/common';
import { PortalRedirectComponent } from './components/portal-redirect/portal-redirect.component';



@NgModule({
  declarations: [
    PortalRedirectComponent
  ],
  imports: [
    CommonModule
  ],
  exports: [
    PortalRedirectComponent
  ]
})
export class CoreModule {
  constructor(@Optional() @SkipSelf() parent?: CoreModule) {
    if (parent) throw new Error('CoreModule should only be imported once.');
  }
}
