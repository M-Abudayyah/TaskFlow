import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="confirm-backdrop" *ngIf="visible">
      <div class="confirm-dialog card">
        <h3>{{ title }}</h3>
        <p>{{ message }}</p>
        <div class="confirm-actions">
          <button type="button" class="btn btn-secondary" (click)="cancel.emit()">Cancel</button>
          <button type="button" class="btn btn-danger" (click)="confirm.emit()">Delete</button>
        </div>
      </div>
    </div>
  `
})
export class ConfirmDialogComponent {
  @Input() visible = false;
  @Input() title = 'Confirm';
  @Input() message = 'Are you sure?';

  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();
}
