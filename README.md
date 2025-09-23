# MiniKanban

A small **Kanban-style** demo app built with:

- **Frontend:** Vue 3 + Vite + TypeScript + Pinia + Axios + Vue Router  
- **Backend:** .NET 8 Minimal API + ASP.NET Identity + EF Core  
- **Auth:** JWT Bearer authentication  
- **Database:** SQLite in development, SQL Server in production

It lets you **register, log in, and view a protected Kanban page** that shows the current user’s email.

---

## 🛠️ Tech Stack

| Area        | Technology                          |
|-------------|-------------------------------------|
| Frontend    | Vue 3, TypeScript, Vite, Pinia, Axios |
| Backend     | .NET 8 Minimal API, ASP.NET Core Identity |
| Database    | EF Core (SQLite for dev / SQL Server for prod) |
| Auth        | JWT (JSON Web Tokens) |
| Tooling     | Swagger / OpenAPI for API testing |

---

## ✨ Features

- **User Registration & Login** using ASP.NET Identity
- **JWT-based Authentication** with access tokens stored in Pinia
- **Protected Routes** in Vue Router (`/kanban` requires login)
- **RESTful API** with Swagger UI (`/swagger`)
- **Proxy Setup** in Vite for seamless API calls during development

---

## 📥 Installation

Clone the repository and install dependencies for **both** the backend and frontend.

```bash
# Clone the project
git clone https://github.com/your-username/minikanban.git
cd minikanban
```

### Back-end

```bash
cd back-end
dotnet restore        # restore NuGet packages
dotnet ef database update   # create the SQLite dev database
cd back-end
dotnet run
```

### Front-end

```bash
cd ../front-end
npm install           # install npm dependencies
cd front-end
npm run dev
``


