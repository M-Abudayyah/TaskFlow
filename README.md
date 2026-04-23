# TaskFlow

TaskFlow is a full-stack task management application built as a technical assessment project using **ASP.NET Core Web API** and **Angular**.

It demonstrates:

- JWT authentication
- ASP.NET Core Identity
- Role-based access control (`Admin` / `User`)
- Task CRUD operations
- Ownership-based authorization
- Angular front-end integration
- Unit testing
- Swagger/OpenAPI documentation

---

## Overview

The system allows authenticated users to manage tasks through a clean Angular interface backed by a .NET 8 Web API.

### Roles

- **Admin**
  - Can view all tasks
  - Can edit any task
  - Can delete any task

- **User**
  - Can view only their own tasks
  - Can create tasks
  - Can edit only their own tasks
  - Cannot delete tasks

---

## Tech Stack

### Backend
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- ASP.NET Core Identity
- JWT Authentication
- FluentValidation
- Swagger / OpenAPI
- xUnit

### Frontend
- Angular
- TypeScript
- RxJS
- Standalone Components
- Custom toast notifications
- Custom confirmation dialog

---

## Solution Structure

```text
TaskFlow/
├─ README.md
├─ .gitignore
├─ global.json
├─ docs/
├─ backend/
│  ├─ TaskFlow.sln
│  ├─ src/
│  │  ├─ TaskFlow.Api/
│  │  ├─ TaskFlow.Application/
│  │  ├─ TaskFlow.Domain/
│  │  └─ TaskFlow.Infrastructure/
│  └─ tests/
│     └─ TaskFlow.UnitTests/
└─ frontend/
   └─ taskflow-web/
