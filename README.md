# Tsuki - Project Readme

The Tsuki project is a novel-reading website built on top of ASP.NET Core MVC and PostgreSQL. The system supports managing novels, chapters, categories, and authors along with key user features such as bookmarking, reading history, favorites, and text-to-speech reading.

## Tech Stack

- Framework: ASP.NET Core MVC (Target framework: .NET 10.0)
- Database: PostgreSQL
- ORM: Entity Framework Core 10
- Frontend Enhancements: Htmx for dynamic page updates and optimized performance
- Security: Argon2id (via Konscious.Security.Cryptography.Argon2) for secure password hashing
- Text-To-Speech: Web Speech API (integrated in browser, supports volume adjustment and custom boundary tracking to resume reading without repeating from the start)
- Environment Configuration: DotNetEnv for loading application environment variables from a .env file

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [PostgreSQL server (version 14 or higher)](https://www.postgresql.org/download/)

## Installation and Execution Guide

### 1. Environment Setup

Create a file named .env inside the project directory Tsuki/Tsuki/ (or copy the provided .env.example) and configure your database connection parameters:

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=TsukiDb
DB_USER=postgres
DB_PASSWORD=your_password
```

Replace your_password with your actual PostgreSQL database password.

### 2. Restore Dependencies

Open your terminal at the root of the project directory and run the following command to restore NuGet packages:

```bash
dotnet restore
```

### 3. Build the Project

Run the build command to ensure the project compiles without errors:

```bash
dotnet build
```

### 4. Run the Application

Run the application using:

```bash
dotnet run --project Tsuki
```

Upon starting, the application will automatically:
- Check for and apply any pending migrations to your PostgreSQL database.
- Initialize seed data (if the database is empty), including a default Admin user, categories, authors, mock novels, and mock chapters.

Once the application has successfully started, navigate to the local address outputted in the console (for example, http://localhost:5079) to explore the website.

Default administrator credentials for the Admin panel (/Admin):
- Email: admin@tsuki.local
- Password: Password123!
