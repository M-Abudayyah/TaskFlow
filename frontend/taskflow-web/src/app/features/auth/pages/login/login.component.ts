import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="auth-page">
      <form class="card auth-card" [formGroup]="form" (ngSubmit)="submit()">
        <h1>TaskFlow</h1>
        <p class="muted">Sign in to continue</p>

        <label>Email</label>
        <input class="input" type="email" formControlName="email" />

        <label>Password</label>
        <input class="input" type="password" formControlName="password" />

        <button class="btn btn-primary" type="submit" [disabled]="loading || form.invalid">
          {{ loading ? 'Signing in...' : 'Login' }}
        </button>

        <p class="state state-error" *ngIf="errorMessage">{{ errorMessage }}</p>
      </form>
    </section>
  `
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  loading = false;
  errorMessage = '';

  submit(): void {
    if (this.form.invalid || this.loading) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.toast.success('Login success.');
        this.router.navigateByUrl('/tasks');
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Invalid email or password.';
        this.toast.error('Login failed.');
      }
    });
  }
}
