import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  id: number;
  message: string;
  type: 'success' | 'error';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly toasts = signal<ToastMessage[]>([]);
  private idCounter = 0;

  success(message: string): void {
    this.show(message, 'success');
  }

  error(message: string): void {
    this.show(message, 'error');
  }

  remove(id: number): void {
    this.toasts.update((items) => items.filter((item) => item.id !== id));
  }

  private show(message: string, type: 'success' | 'error'): void {
    const id = ++this.idCounter;
    this.toasts.update((items) => [...items, { id, message, type }]);
    setTimeout(() => this.remove(id), 3000);
  }
}
