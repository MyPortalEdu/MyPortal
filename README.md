# <img src="https://i.imgur.com/dAVgTNy.png" alt="MyPortal" width="500"/>

An open-source School Information Management System (MIS) for UK schools. ASP.NET Core Web API + Angular SPA + SQL Server, designed to be self-hostable, modular, and transparent.

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)

---

## Status

Under active development. Core services and foundational modules are in place; additional functionality is being built iteratively. Not yet production-ready.

## Modules

Attendance, behaviour, exams, assessment, profiles and reporting, curriculum and timetabling, student management, staff management, SEND, documents and attachments, admissions, school configuration, and system administration.

Each module is designed to stand on its own while contributing to a coherent whole. Permissions, auditing, and safeguarding controls are first-class concerns.

## Tech stack

- **Backend** — ASP.NET Core 8 Web API
- **Frontend** — Angular 20 SPA (PrimeNG, Tailwind) serving staff, students, and parents via role-based routing
- **Database** — SQL Server 2019+ (additional providers planned)
- **Auth** — OAuth 2.0 / OpenID Connect
- **Architecture** — Modular, API-first; SQL stored in versioned migrations and embedded resources

## Repository layout

```
MyPortal.WebApi/         ASP.NET Core host
MyPortal.Data/           Repositories, Dapper queries, embedded SQL
MyPortal.Migrations/     Console runner — creates/updates the database schema
MyPortal.Core/           Domain entities
MyPortal.Contracts/      Request/response DTOs
MyPortal.Common/         Shared interfaces and utilities
MyPortal.Auth/           Authentication and authorisation
MyPortal.Frontend/myportal-spa/   Angular SPA
```

---

## Getting started

### Prerequisites

- .NET 8 SDK
- Node.js 20+ and npm
- SQL Server 2019+ (Developer, Express, or LocalDB)
- A SQL login with permission to create databases

### 1. Clone

```bash
git clone https://github.com/MyPortalEdu/MyPortal.git
cd MyPortal
```

### 2. Configure the database connection

Both `MyPortal.Migrations` and `MyPortal.WebApi` read their connection string from `Database:ConnectionString`. Use user-secrets so it stays out of source control:

```bash
dotnet user-secrets init --project MyPortal.Migrations
dotnet user-secrets set "Database:ConnectionString" "Server=.;Database=MyPortal;Trusted_Connection=True;TrustServerCertificate=True" --project MyPortal.Migrations

dotnet user-secrets init --project MyPortal.WebApi
dotnet user-secrets set "Database:ConnectionString" "Server=.;Database=MyPortal;Trusted_Connection=True;TrustServerCertificate=True" --project MyPortal.WebApi
```

Alternatives: set `Database__ConnectionString` as an environment variable, or pass `--Database:ConnectionString=...` on the command line.

### 3. Create / update the database

The migrations runner creates the database if it does not exist and applies any pending updates, views, functions, and stored procedures.

```bash
dotnet run --project MyPortal.Migrations
```

Re-run this whenever you pull changes that include new SQL.

### 4. Run the Web API

```bash
dotnet run --project MyPortal.WebApi
```

The API listens on the URLs configured in `MyPortal.WebApi/Properties/launchSettings.json`.

### 5. Run the SPA

```bash
cd MyPortal.Frontend/myportal-spa
npm install
npm start
```

The dev server runs on `http://localhost:4200`. Use `npm run start:https` to serve over HTTPS with the proxy config in `proxy.conf.json`.

---

## Development

- **Build everything** — `dotnet build MyPortal.sln`
- **Run backend tests** — `dotnet test`
- **Run SPA tests** — `npm test` from `MyPortal.Frontend/myportal-spa`

SQL changes live in `MyPortal.Migrations/Sql`:
- `Updates/` — one-shot migrations, applied in filename order
- `StoredProcedures/`, `Functions/`, `Views/`, `Indexes/` — re-applied on every run (idempotent `CREATE OR ALTER`)

New SQL files must be registered as `EmbeddedResource` in `MyPortal.Migrations.csproj`. Query templates used by repositories at runtime live under `MyPortal.Data/Sql` and are registered in `MyPortal.Data.csproj` the same way.

---

## Contributing

Issues, discussions, and pull requests are welcome. Useful areas include backend services, frontend UI/UX, documentation, UK education domain modelling, and deployment tooling. Open an issue or discussion before starting non-trivial work so we can align on direction.

## License

Licensed under the [GNU Affero General Public License v3.0](https://www.gnu.org/licenses/agpl-3.0). Modifications deployed over a network must be made available under the same licence.
