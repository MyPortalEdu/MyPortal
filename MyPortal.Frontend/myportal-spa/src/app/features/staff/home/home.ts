import { Component } from '@angular/core';
import { Card } from 'primeng/card';
import { PageHeader } from '../../../shared/components/page-header/page-header';
import { BulletinsFeed } from '../../../shared/components/bulletins/bulletins-feed/bulletins-feed';

@Component({
  selector: 'mp-home',
  imports: [
    Card,
    PageHeader,
    BulletinsFeed,
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
}
