# \# TaskFlow

# 

# TaskFlow is a full-stack task management application built as a technical assessment using \*\*ASP.NET Core Web API\*\* and \*\*Angular\*\*.

# 

# The project demonstrates:

# 

# \- JWT authentication

# \- ASP.NET Core Identity

# \- Role-based authorization (`Admin` / `User`)

# \- Ownership-based task access rules

# \- Task CRUD operations

# \- Angular front-end integration

# \- Unit testing

# \- Swagger/OpenAPI support

# 

# \---

# 

# \## Repository

# 

# GitHub Repository: `https://github.com/M-Abudayyah/TaskFlow.git`

# 

# \---

# 

# \## Overview

# 

# TaskFlow allows authenticated users to manage tasks through a clean Angular interface backed by a secure .NET 8 API.

# 

# \### Roles

# 

# \#### Admin

# \- Can view all tasks

# \- Can edit any task

# \- Can delete any task

# 

# \#### User

# \- Can view only their own tasks

# \- Can create tasks

# \- Can edit only their own tasks

# \- Cannot delete tasks

# 

# \---

# 

# \## Tech Stack

# 

# \### Backend

# \- .NET 8

# \- ASP.NET Core Web API

# \- Entity Framework Core

# \- SQLite

# \- ASP.NET Core Identity

# \- JWT Authentication

# \- FluentValidation

# \- Swagger / OpenAPI

# \- xUnit

# 

# \### Frontend

# \- Angular

# \- TypeScript

# \- RxJS

# \- Standalone Components

# \- Custom toast notifications

# \- Custom confirmation dialog

# 

# \---

# 

# \## Solution Structure

# 

# ```text

# TaskFlow/

# ├─ README.md

# ├─ .gitignore

# ├─ global.json

# ├─ docs/

# ├─ backend/

# │  ├─ TaskFlow.sln

# │  ├─ src/

# │  │  ├─ TaskFlow.Api/

# │  │  ├─ TaskFlow.Application/

# │  │  ├─ TaskFlow.Domain/

# │  │  └─ TaskFlow.Infrastructure/

# │  └─ tests/

# │     └─ TaskFlow.UnitTests/

# └─ frontend/

# &#x20;  └─ taskflow-web/

