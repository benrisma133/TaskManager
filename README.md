# TaskFlow — Task & Project Manager

A modern WPF desktop application for managing tasks, projects, and categories with a built-in timer system. Built with a clean 3-tier architecture and full dark/light theme support.

---

## Screenshots

> _Screenshots coming soon_

---

## Architecture

```


    TaskManager/
    ├── Repository/          # Data access layer
    ├── Service/             # Business logic layer
    └── TaskManagerUI/       # WPF Presentation layer

```

**Flow:** UI → Service → Repository → SQL Server (Docker in CI / SQLEXPRESS locally)

---

## Tech Stack

| Layer        | Technology                        |
|--------------|-----------------------------------|
| UI           | WPF (.NET 8, Windows)             |
| Business     | C# Class Library (.NET 8)         |
| Data Access  | C# Class Library + Stored Procs   |
| Database     | SQL Server Express (local) / Docker (CI) |
| CI           | GitHub Actions (Ubuntu + SQL Server container) |

---

## Features

### ✅ Categories
- View all categories in a responsive card grid
- Search and filter by name and type
- Add / Edit category (name, type, icon, color)
- Delete with smart validation:
  - Built-in categories cannot be deleted
  - Categories in use by projects cannot be deleted
- Full dark / light theme support

### ✅ Settings
- Toggle dark / light mode

### 🔜 Projects _(coming soon)_
- View projects per category
- Add / Edit / Delete projects
- Filter by status and priority

### 🔜 Tasks _(coming soon)_
- View tasks per project
- Add / Edit / Delete tasks
- Filter by status and priority

### 🔜 Timer _(coming soon)_
- Start / Pause / Stop timer per task
- Active task shown in dashboard
- Active indicator on task menu item
- Timer session card on dashboard

### 🔜 Dashboard _(coming soon)_
- Active task session card
- Tasks completed today / this week / this month
- Current streak and longest streak
- Daily minutes chart

---

## Navigation Flow _(coming soon)_

```

    Categories Page
      └── Click card → Projects Page (filtered by category)
            └── Click card → Tasks Page (filtered by project)
                  └── Click task → Task Detail + Timer
                        └── Start timer → Dashboard session card

```

Each child page has a **back button** (top left) to return to the parent.

---

## CI / CD

Every push and pull request to `main` runs:
- Restore dependencies
- Build Repository, Service, and UI layers
- Branch protection: CI must pass before merge

```yaml
runs-on: ubuntu-latest
dotnet-version: '8.0.x'
```

---

## Local Setup

### Prerequisites
- Windows 10/11
- .NET 8 SDK
- SQL Server Express (`localhost\SQLEXPRESS01`)
- Visual Studio 2022+

### Database
1. Run `TaskFlowDb.sql` in SSMS to create the database and schema
2. Set environment variables:
```powershell
[System.Environment]::SetEnvironmentVariable("DB_SERVER", "localhost\SQLEXPRESS01", "User")
[System.Environment]::SetEnvironmentVariable("DB_NAME",   "TaskFlowDb", "User")
```

### Run
```bash
git clone https://github.com/benrisma133/TaskManager.git
cd TaskManager
dotnet build
```

Then open `TaskManagerUI` as startup project in Visual Studio and run.

---

## Project Status

| Feature      | Status        |
|--------------|---------------|
| Categories   | ✅ Complete   |
| Settings     | ✅ Complete   |
| Projects     | 🔜 Coming soon |
| Tasks        | 🔜 Coming soon |
| Timer        | 🔜 Coming soon |
| Dashboard    | 🔜 Coming soon |

---

## License

MIT