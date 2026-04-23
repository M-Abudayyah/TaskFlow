import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { TaskItem, TaskPriority, TaskStatus } from '../../../../core/models/task.models';
import { TaskService } from '../../services/task.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-task-edit-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <section class="page">
      <h1>Edit Task</h1>


      <form
        class="card form-grid"
        [formGroup]="form"
        (ngSubmit)="submit()"
        *ngIf="!loadingTask && loaded"
      >
        <label>Title</label>
        <input class="input" type="text" formControlName="title" />

        <label>Description</label>
        <textarea class="input" rows="4" formControlName="description"></textarea>

        <div class="two-columns">
          <div>
            <label>Status</label>
            <select class="input" formControlName="status">
              <option *ngFor="let s of statuses" [value]="s">{{ s }}</option>
            </select>
          </div>

          <div>
            <label>Priority</label>
            <select class="input" formControlName="priority">
              <option *ngFor="let p of priorities" [value]="p">{{ p }}</option>
            </select>
          </div>
        </div>

        <label>Due Date</label>
        <input class="input" type="datetime-local" formControlName="dueDateUtc" />

        <button class="btn btn-primary" type="submit" [disabled]="saving || form.invalid">
          {{ saving ? 'Saving...' : 'Save Changes' }}
        </button>

        <a class="btn btn-secondary" routerLink="/tasks">Cancel</a>

        <p class="state state-error" *ngIf="errorMessage">{{ errorMessage }}</p>
      </form>

      <p class="state state-error" *ngIf="!loadingTask && !loaded && errorMessage">
        {{ errorMessage }}
      </p>
    </section>
  `
})
export class TaskEditComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly taskService = inject(TaskService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly priorities: TaskPriority[] = ['Low', 'Medium', 'High'];
  readonly statuses: TaskStatus[] = ['Todo', 'InProgress', 'Done'];

  readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    description: [''],
    status: this.fb.nonNullable.control<TaskStatus>('Todo'),
    priority: this.fb.nonNullable.control<TaskPriority>('Medium'),
    dueDateUtc: ['']
  });

  taskId = '';
  loaded = false;
  loadingTask = false;
  saving = false;
  errorMessage = '';

  ngOnInit(): void {
    this.taskId = this.route.snapshot.paramMap.get('id') ?? '';

    if (!this.taskId) {
      this.router.navigateByUrl('/tasks');
      return;
    }

    queueMicrotask(() => this.loadTask());
  }

  private loadTask(): void {
    this.loadingTask = true;
    this.loaded = false;
    this.errorMessage = '';

    this.taskService
      .getTaskById(this.taskId)
      .pipe(
        finalize(() => {
          this.loadingTask = false;
        })
      )
      .subscribe({
        next: (task) => {
          try {
            this.patchForm(task);
            this.loaded = true;
          } catch (error) {
            console.error('Failed while preparing edit form:', error);
            this.loaded = false;
            this.errorMessage = 'Failed to render task details.';
            this.toast.error('Failed to render task details.');
          }
        },
        error: (error) => {
          console.error('Failed to load task:', error);
          this.loaded = false;
          this.errorMessage = 'Failed to load task.';
          this.toast.error('Failed to load task.');
        }
      });
  }

  submit(): void {
    if (this.form.invalid || this.saving || !this.taskId) {
      return;
    }

    this.saving = true;
    this.errorMessage = '';

    const value = this.form.getRawValue();

    this.taskService
      .updateTask(this.taskId, {
        title: value.title,
        description: value.description || null,
        status: value.status,
        priority: value.priority,
        dueDateUtc: value.dueDateUtc ? new Date(value.dueDateUtc).toISOString() : null
      })
      .pipe(
        finalize(() => {
          this.saving = false;
        })
      )
      .subscribe({
        next: () => {
          this.toast.success('Task updated successfully.');
          this.router.navigateByUrl('/tasks');
        },
        error: (error) => {
          console.error('Failed to update task:', error);
          this.errorMessage = 'Failed to update task.';
          this.toast.error('Failed to update task.');
        }
      });
  }

  private patchForm(task: TaskItem): void {
    this.form.patchValue({
      title: task.title ?? '',
      description: task.description ?? '',
      status: task.status,
      priority: task.priority,
      dueDateUtc: task.dueDateUtc ? this.toDateTimeLocal(task.dueDateUtc) : ''
    });
  }

  private toDateTimeLocal(value: string): string {
    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return '';
    }

    const offset = date.getTimezoneOffset() * 60000;
    return new Date(date.getTime() - offset).toISOString().slice(0, 16);
  }
}
