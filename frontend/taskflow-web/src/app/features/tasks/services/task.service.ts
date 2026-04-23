import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { apiConfig } from '../../../core/config/api.config';
import {
  CreateTaskRequest,
  TaskItem,
  UpdateTaskRequest
} from '../../../core/models/task.models';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly baseUrl = `${apiConfig.baseUrl}/api/tasks`;

  constructor(private readonly http: HttpClient) {}

  getTasks(): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(this.baseUrl);
  }

  getTaskById(id: string): Observable<TaskItem> {
    return this.http.get<TaskItem>(`${this.baseUrl}/${id}`);
  }

  createTask(payload: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(this.baseUrl, payload);
  }

  updateTask(id: string, payload: UpdateTaskRequest): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${this.baseUrl}/${id}`, payload);
  }

  deleteTask(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
