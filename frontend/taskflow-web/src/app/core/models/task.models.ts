export type TaskStatus = 'Todo' | 'InProgress' | 'Done';
export type TaskPriority = 'Low' | 'Medium' | 'High';

export interface TaskItem {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  createdAtUtc: string;
  createdByUserId: string;
  createdByEmail?: string | null;
  createdByDisplayName?: string | null;
  dueDateUtc: string | null;
  updatedAtUtc: string | null;
}

export interface CreateTaskRequest {
  title: string;
  description: string | null;
  priority: TaskPriority;
  dueDateUtc: string | null;
}

export interface UpdateTaskRequest {
  title?: string;
  description?: string | null;
  status?: TaskStatus;
  priority?: TaskPriority;
  dueDateUtc?: string | null;
}
