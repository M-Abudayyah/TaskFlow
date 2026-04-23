import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TaskPriority } from '../../../../core/models/task.models';
import { TaskService } from '../../services/task.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-task-create-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <section class="page">
      <h1>Create Task</h1>

      <form class="card form-grid" [formGroup]="form" (ngSubmit)="submit()">
        <label>Title</label>
        <input class="input" type="text" formControlName="title" />

        <label>Description</label>
        <textarea class="input" rows="4" formControlName="description"></textarea>

        <div class="two-columns">
          <div>
            <label>Priority</label>
            <select class="input" formControlName="priority">
              <option *ngFor="let p of priorities" [value]="p">{{ p }}</option>
            </select>
          </div>
          <div>
            <label>Due Date</label>
            <input class="input" type="datetime-local" formControlName="dueDateUtc" />
          </div>
        </div>

        <button class="btn btn-primary" type="submit" [disabled]="loading || form.invalid">
          {{ loading ? 'Saving...' : 'Create Task' }}
        </button>
        <a class="btn btn-secondary" routerLink="/tasks">Cancel</a>

        <p class="state state-error" *ngIf="errorMessage">{{ errorMessage }}</p>
      </form>
    </section>
  `
})
export class TaskCreateComponent {
  private readonly fb = inject(FormBuilder);
  private readonly taskService = inject(TaskService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly priorities: TaskPriority[] = ['Low', 'Medium', 'High'];
  readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    description: [''],
    priority: this.fb.nonNullable.control<TaskPriority>('Medium'),
    dueDateUtc: ['']
  });

  loading = false;
  errorMessage = '';

  submit(): void {
    if (this.form.invalid || this.loading) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    const value = this.form.getRawValue();

    this.taskService
      .createTask({
        title: value.title,
        description: value.description || null,
        priority: value.priority,
        dueDateUtc: value.dueDateUtc ? new Date(value.dueDateUtc).toISOString() : null
      })
      .subscribe({
        next: () => {
          this.toast.success('Task created successfully.');
          this.router.navigateByUrl('/tasks');
        },
        error: () => {
          this.loading = false;
          this.errorMessage = 'Failed to create task.';
          this.toast.error('Failed to create task.');
        }
      });
  }
}
