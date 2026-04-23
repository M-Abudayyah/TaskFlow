import { ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';
import { ToastService } from '../../../../core/services/toast.service';
import { TaskItem } from '../../../../core/models/task.models';
import { TaskService } from '../../services/task.service';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog.component';

type SortField = 'title' | 'status' | 'priority' | 'dueDateUtc';
type SortDirection = 'asc' | 'desc';

@Component({
  selector: 'app-task-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ConfirmDialogComponent],
  template: `
    <section class="page">
      <header class="page-header">
        <div>
          <h1>Tasks</h1>
          <p class="muted">{{ currentUserEmail }}</p>
        </div>

        <div class="header-actions">
          <a class="btn btn-primary" routerLink="/tasks/create">Create Task</a>
          <button class="btn btn-secondary" type="button" (click)="logout()">Logout</button>
        </div>
      </header>

      <section class="card table-toolbar">
        <input
          class="input"
          placeholder="Search title, status, priority, owner..."
          [(ngModel)]="searchTerm"
          (ngModelChange)="onSearchChange()"
        />

        <select
          class="input"
          [(ngModel)]="sortField"
          (ngModelChange)="onSortChange()"
        >
          <option value="title">Title</option>
          <option value="status">Status</option>
          <option value="priority">Priority</option>
          <option value="dueDateUtc">Due Date</option>
        </select>

        <button class="btn btn-secondary" type="button" (click)="toggleSortDirection()">
          {{ sortDirection === 'asc' ? 'Ascending' : 'Descending' }}
        </button>
      </section>

      <div class="loading" *ngIf="loading">Loading tasks...</div>
      <div class="state state-error" *ngIf="errorMessage">{{ errorMessage }}</div>

      <div class="state state-empty" *ngIf="!loading && !errorMessage && pagedTasks.length === 0">
        No tasks found.
      </div>

      <section class="card table-wrap" *ngIf="!loading && !errorMessage && pagedTasks.length > 0">
        <table>
          <thead>
            <tr>
              <th>Title</th>
              <th>Status</th>
              <th>Priority</th>
              <th>Due Date</th>
              <th>Owner</th>
              <th>Actions</th>
            </tr>
          </thead>

          <tbody>
            <tr *ngFor="let task of pagedTasks">
              <td>{{ task.title }}</td>
              <td>{{ task.status }}</td>
              <td>{{ task.priority }}</td>
              <td>{{ formatDueDate(task.dueDateUtc) }}</td>
              <td>{{ getOwnerLabel(task) }}</td>
              <td>
                <div class="row-actions">
                  <button
                    class="btn btn-primary"
                    type="button"
                    *ngIf="canEdit(task)"
                    (click)="edit(task.id)"
                  >
                    Edit
                  </button>

                  <button
                    class="btn btn-danger"
                    type="button"
                    *ngIf="isAdmin"
                    (click)="openDeleteDialog(task)"
                  >
                    Delete
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <footer class="pagination" *ngIf="!loading && !errorMessage && totalItems > 0">
        <div class="muted">
          Showing {{ startItem }} - {{ endItem }} of {{ totalItems }}
        </div>

        <div class="pagination-controls">
          <select
            class="input page-size"
            [(ngModel)]="pageSize"
            (ngModelChange)="onPageSizeChange()"
          >
            <option [ngValue]="5">5</option>
            <option [ngValue]="10">10</option>
          </select>

          <button
            class="btn btn-secondary"
            type="button"
            (click)="previousPage()"
            [disabled]="currentPage === 1"
          >
            Previous
          </button>

          <span>Page {{ currentPage }} / {{ totalPages }}</span>

          <button
            class="btn btn-secondary"
            type="button"
            (click)="nextPage()"
            [disabled]="currentPage === totalPages"
          >
            Next
          </button>
        </div>
      </footer>
    </section>

    <app-confirm-dialog
      [visible]="confirmVisible"
      title="Delete Task"
      [message]="confirmMessage"
      (confirm)="confirmDelete()"
      (cancel)="closeConfirmDialog()"
    />
  `
})
export class TaskListComponent implements OnInit {
  private readonly taskService = inject(TaskService);
  private readonly authService = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
private readonly cdr = inject(ChangeDetectorRef);

  tasks: TaskItem[] = [];

  loading = false;
  errorMessage = '';
  searchTerm = '';
  sortField: SortField = 'dueDateUtc';
  sortDirection: SortDirection = 'asc';

  pageSize = 10;
  currentPage = 1;

  currentUserId = '';
  currentUserEmail = '';
  isAdmin = false;

  confirmVisible = false;
  confirmMessage = '';
  pendingDeleteTaskId: string | null = null;

ngOnInit(): void {
  const user = this.authService.getCurrentUser();
  this.currentUserId = user?.userId ?? '';
  this.currentUserEmail = user?.email ?? '';
  this.isAdmin = this.authService.isAdmin();

  queueMicrotask(() => this.loadTasks());
}

  get filteredTasks(): TaskItem[] {
    const search = this.searchTerm.trim().toLowerCase();

    if (!search) {
      return [...this.tasks];
    }

    return this.tasks.filter((task) => {
      const owner = this.getOwnerLabel(task).toLowerCase();

      return (
        (task.title ?? '').toLowerCase().includes(search) ||
        (task.status ?? '').toLowerCase().includes(search) ||
        (task.priority ?? '').toLowerCase().includes(search) ||
        owner.includes(search)
      );
    });
  }

  get sortedTasks(): TaskItem[] {
    const items = [...this.filteredTasks];
    items.sort((a, b) => this.compareTasks(a, b));
    return items;
  }

  get pagedTasks(): TaskItem[] {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    return this.sortedTasks.slice(start, end);
  }

  get totalItems(): number {
    return this.sortedTasks.length;
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalItems / this.pageSize));
  }

  get startItem(): number {
    if (this.totalItems === 0) {
      return 0;
    }

    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get endItem(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }

 loadTasks(): void {
  this.loading = true;
  this.errorMessage = '';

  this.taskService
    .getTasks()
    .pipe(finalize(() => {
      this.loading = false;
      this.cdr.detectChanges();
    }))
    .subscribe({
      next: (items) => {
        this.tasks = Array.isArray(items) ? items : [];
        this.currentPage = 1;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Failed to load tasks:', error);
        this.tasks = [];
        this.errorMessage = 'Failed to load tasks.';
        this.toast.error('Failed to load tasks.');
        this.cdr.detectChanges();
      }
    });
}
  onSearchChange(): void {
    this.currentPage = 1;
  }

  onSortChange(): void {
    this.currentPage = 1;
  }

  onPageSizeChange(): void {
    this.currentPage = 1;
  }

  toggleSortDirection(): void {
    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    this.currentPage = 1;
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  canEdit(task: TaskItem): boolean {
    return this.isAdmin || task.createdByUserId === this.currentUserId;
  }

  edit(id: string): void {
    this.router.navigate(['/tasks', id, 'edit']);
  }

  openDeleteDialog(task: TaskItem): void {
    this.pendingDeleteTaskId = task.id;
    this.confirmMessage = `Are you sure you want to delete "${task.title}"?`;
    this.confirmVisible = true;
  }

  closeConfirmDialog(): void {
    this.pendingDeleteTaskId = null;
    this.confirmVisible = false;
  }

  confirmDelete(): void {
    if (!this.pendingDeleteTaskId) {
      return;
    }

    const id = this.pendingDeleteTaskId;
    this.closeConfirmDialog();

    this.taskService.deleteTask(id).subscribe({
      next: () => {
        this.toast.success('Task deleted successfully.');
        this.loadTasks();
      },
      error: () => {
        this.toast.error('Failed to delete task.');
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }

  formatDueDate(value: string | null): string {
    if (!value) {
      return 'No due date';
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? '-' : date.toLocaleString();
  }

  getOwnerLabel(task: TaskItem): string {
    if (task.createdByUserId === this.currentUserId) {
      return 'You';
    }
    console.log("createdByDisplayName");
    console.log(task.createdByDisplayName);
    if (task.createdByDisplayName?.trim()) {
      return task.createdByDisplayName;
    }
    console.log("createdByEmail");

console.log(task.createdByEmail);
    if (task.createdByEmail?.trim()) {
      return task.createdByEmail;
    }

    if (task.createdByUserId === 'system') {
      return 'System';
    }

    return 'Unknown user';
  }

  private compareTasks(a: TaskItem, b: TaskItem): number {
    const factor = this.sortDirection === 'asc' ? 1 : -1;

    switch (this.sortField) {
      case 'title':
        return factor * (a.title ?? '').localeCompare(b.title ?? '');
      case 'status':
        return factor * (a.status ?? '').localeCompare(b.status ?? '');
      case 'priority':
        return factor * (a.priority ?? '').localeCompare(b.priority ?? '');
      case 'dueDateUtc': {
        const aTime = a.dueDateUtc ? new Date(a.dueDateUtc).getTime() : Number.MAX_SAFE_INTEGER;
        const bTime = b.dueDateUtc ? new Date(b.dueDateUtc).getTime() : Number.MAX_SAFE_INTEGER;
        return factor * (aTime - bTime);
      }
      default:
        return 0;
    }
  }
}
