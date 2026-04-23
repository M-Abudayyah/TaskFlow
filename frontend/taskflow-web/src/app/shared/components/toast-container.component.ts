import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-stack">
      <div
        class="toast"
        *ngFor="let toast of toastService.toasts()"
        [class.toast-success]="toast.type === 'success'"
        [class.toast-error]="toast.type === 'error'"
      >
        <span>{{ toast.message }}</span>
        <button type="button" (click)="toastService.remove(toast.id)">×</button>
      </div>
    </div>
  `
})
export class ToastContainerComponent {
  readonly toastService = inject(ToastService);
}
