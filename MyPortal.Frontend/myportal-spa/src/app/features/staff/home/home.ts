import { Component } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';
import { PageHeader } from '../../../shared/components/page-header/page-header';
import { BulletinsFeed } from '../../../shared/components/bulletins/bulletins-feed/bulletins-feed';

@Component({
  selector: 'mp-home',
  imports: [
    PageHeader,
    BulletinsFeed,
    TranslocoDirective,
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
}
