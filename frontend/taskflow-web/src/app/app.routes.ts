import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { LoginComponent } from './features/auth/pages/login/login.component';
import { TaskCreateComponent } from './features/tasks/pages/task-create/task-create.component';
import { TaskEditComponent } from './features/tasks/pages/task-edit/task-edit.component';
import { TaskListComponent } from './features/tasks/pages/task-list/task-list.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'tasks' },
  { path: 'login', component: LoginComponent },
  {
    path: 'tasks',
    canActivate: [authGuard],
    children: [
      { path: '', component: TaskListComponent },
      { path: 'create', component: TaskCreateComponent },
      { path: ':id/edit', component: TaskEditComponent }
    ]
  },
  { path: '**', redirectTo: 'tasks' }
];
