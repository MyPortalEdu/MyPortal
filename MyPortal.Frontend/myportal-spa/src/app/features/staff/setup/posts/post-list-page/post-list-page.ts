import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { MpBadge, MpButton, MpCard, MpSkeleton, MpTable, MpTableBody, MpTableHeader } from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../../shared/components/empty-state/empty-state';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { PostsDataService } from '../../../../../shared/services/posts-data.service';
import { PostResponse, PostsResponse } from '../../../../../shared/types/staff-setup';
import { PostEditorDialog } from '../post-editor-dialog/post-editor-dialog';

@Component({
  selector: 'mp-post-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MpBadge,
    MpButton,
    MpCard,
    MpSkeleton,
    MpTable,
    MpTableHeader,
    MpTableBody,
    PageHeader,
    EmptyState,
    PostEditorDialog,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-setup')],
  templateUrl: './post-list-page.html',
})
export class PostListPage implements OnInit {
  private readonly data = inject(PostsDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  protected readonly loading = signal(false);
  protected readonly area = signal<PostsResponse | null>(null);
  protected readonly editorOpen = signal(false);
  protected readonly editing = signal<PostResponse | null>(null);

  protected readonly posts = computed(() => this.area()?.posts ?? []);
  protected readonly canEdit = computed(() => this.area()?.canEdit ?? false);

  protected readonly headerActions = computed<HeaderAction[]>(() =>
    this.canEdit()
      ? [
          {
            label: this.transloco.translate('staff-setup.posts.add'),
            icon: 'fa-solid fa-plus',
            severity: 'primary',
            command: () => this.openNew(),
          },
        ]
      : [],
  );

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.data.getPosts().subscribe({
      next: row => {
        this.area.set(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-setup.posts.loadError'));
      },
    });
  }

  protected openNew(): void {
    this.editing.set(null);
    this.editorOpen.set(true);
  }

  protected openEdit(post: PostResponse): void {
    if (!this.canEdit()) return;
    this.editing.set(post);
    this.editorOpen.set(true);
  }

  protected onSaved(): void {
    this.editorOpen.set(false);
    this.load();
  }

  protected label(list: { id: string; description: string }[], id: string | null | undefined): string {
    if (!id) return '—';
    return list.find(x => x.id === id)?.description ?? '—';
  }

  protected categories = computed(() => this.area()?.postCategories ?? []);

  protected async confirmDelete(post: PostResponse): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('staff-setup.posts.deleteHeader'),
      message: this.transloco.translate('staff-setup.posts.deleteConfirm'),
      acceptLabel: this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });

    if (!ok) return;

    try {
      await firstValueFrom(this.data.delete(post.id));
      this.notify.success(this.transloco.translate('staff-setup.posts.deletedToast'));
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-setup.posts.deleteError'));
    }
  }
}
