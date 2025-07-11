# ExcellyGen LMS Backend

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/en-us/sql-server)
[![Firebase](https://img.shields.io/badge/Firebase-FFCA28?style=for-the-badge&logo=firebase&logoColor=black)](https://firebase.google.com/)
[![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)](https://swagger.io/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](https://opensource.org/licenses/MIT)

## 🚀 Overview

The **ExcellyGen Learning and Management System (L&MS) Backend** is a robust, cloud-based platform designed to enhance employee training and development within ExcellyGen. This system streamlines learning processes, tracks progress, manages certifications, and provides comprehensive tools for administrators, course coordinators, and project managers to oversee training programs and employee assignments.

This backend serves as the core API for the L&MS, handling data persistence, business logic, authentication, and integration with external services.

## 📋 Table of Contents

- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Database Setup](#database-setup)
  - [Firebase Configuration](#firebase-configuration)
  - [Running the Application](#running-the-application)
- [API Documentation](#-api-documentation)
- [Contributing](#-contributing)
- [License](#-license)

## ✨ Features

### 🔐 Authentication & Authorization
- Secure login with JWT tokens
- Role-based access control (RBAC)
- Multi-factor authentication
- Firebase Authentication integration
- Password management and reset

### 📚 Course Management
- **Course Coordinators**: Create, update, delete courses with videos, documents, and quizzes
- **Employees**: Search, enroll, complete courses and track progress
- **Administrators**: Assign courses to learning paths, manage content, archive courses

### 💬 Discussion Forums
- **Employees**: Post threads, reply to discussions, receive notifications
- **Moderators**: Edit/delete inappropriate content

### 👥 Employee Management
- Project assignment based on skills and availability
- Bench status management (assigned/available)
- Employee records and role tracking
- Availability notifications

### 📊 Analytics & Reporting
- Training progress reports
- Filterable analytics by course, department, or employee
- Dynamic CV generation support
- Comprehensive dashboard metrics

### 🔔 Additional Features
- System-wide notifications
- Multilingual support (English/Norwegian)
- File upload and management
- Email services integration

## 🛠 Technology Stack

| Category | Technology |
|----------|------------|
| **Framework** | .NET 8.0 |
| **Language** | C# |
| **Database** | Microsoft SQL Server |
| **ORM** | Entity Framework Core |
| **Authentication** | JWT, Firebase Auth |
| **File Storage** | Firebase Storage |
| **Email** | SMTP (Gmail) |
| **Documentation** | Swagger/OpenAPI |
| **Architecture** | Clean Architecture |

### Key Dependencies
```xml
Microsoft.AspNetCore.Authentication.JwtBearer
Google.Cloud.Storage.V1
DotNetEnv
Microsoft.EntityFrameworkCore.SqlServer
```

## 📁 Project Structure

```
udaraexcellygen-excellygen-lms-backend/
├── ExcellyGenLMS.sln                    # Solution file
├── ExcellyGenLMS.API/                   # 🌐 Web API Layer
│   ├── Controllers/                     # API endpoints by domain/role
│   ├── Extensions/                      # Service registrations
│   ├── Middleware/                      # Custom middleware
│   └── appsettings.json                # Application configuration
├── ExcellyGenLMS.Application/          # 💼 Application Layer
│   ├── DTOs/                           # Data Transfer Objects
│   ├── Interfaces/                     # Service interfaces
│   └── Services/                       # Business logic
├── ExcellyGenLMS.Core/                 # 🎯 Core Domain Layer
│   ├── Entities/                       # Domain entities
│   ├── Enums/                          # Enumerations
│   └── Interfaces/                     # Repository interfaces
└── ExcellyGenLMS.Infrastructure/       # 🔧 Infrastructure Layer
    ├── Data/                           # DbContext & repositories
    ├── Migrations/                     # Database migrations
    └── Services/                       # External service implementations
```

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Local or Azure)
- Firebase project with service account key
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/udaraexcellygen/excellygen-lms-backend.git
   cd excellygen-lms-backend
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore ExcellyGenLMS.sln
   ```

### Database Setup

1. **Configure Connection String**
   
   Update `ExcellyGenLMS.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server_name;Initial Catalog=excellygen_lms;User ID=your_username;Password=your_password;TrustServerCertificate=True;Connection Timeout=30;"
     }
   }
   ```

   > ⚠️ **Note**: `TrustServerCertificate=True` is for local development only. Use proper certificates in production.

2. **Apply Database Migrations**
   ```bash
   cd ExcellyGenLMS.API
   dotnet ef database update
   ```

### Firebase Configuration

1. **Obtain Service Account Key**
   - Go to Firebase Console → Project Settings → Service Accounts
   - Generate new private key and download JSON file

2. **Configure Service Account Path**
   
   **Option A**: Place file at default path
   ```
   C:\SecureKeys\excelly-lms-f3500-firebase-adminsdk-fbsvc-2162cc0e87.json
   ```

   **Option B** (Recommended): Use environment variables
   ```bash
   # Set environment variable
   FIREBASE_SERVICE_ACCOUNT_PATH=/path/to/your/firebase-key.json
   ```

3. **Update Firebase Settings**
   
   Configure `appsettings.json` Firebase section with your project details:
   ```json
   {
     "Firebase": {
       "ProjectId": "your-project-id",
       "AuthDomain": "your-project.firebaseapp.com",
       "StorageBucket": "your-project.appspot.com"
     }
   }
   ```

### Running the Application

1. **Navigate to API directory**
   ```bash
   cd ExcellyGenLMS.API
   ```

2. **Start the application**
   ```bash
   dotnet run
   ```

3. **Access the application**
   - API: `http://localhost:5177`
   - Swagger UI: `http://localhost:5177/swagger`

## 📖 API Documentation

### Main Controller Groups

| Endpoint | Description |
|----------|-------------|
| `/api/Auth` | User registration, login, token management |
| `/api/Admin` | Administrative tasks, user management, analytics |
| `/api/Course` | Course operations for learners and coordinators |
| `/api/Learner` | Learner-specific functionalities |
| `/api/ProjectManager` | Employee assignment and project operations |

### Authentication

All protected endpoints require JWT authentication:

```bash
Authorization: Bearer <your-jwt-token>
```

### Example API Calls

**Register User**
```bash
POST /api/Auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Get Courses**
```bash
GET /api/Course
Authorization: Bearer <token>
```

> 📚 **Full API Documentation**: Visit `/swagger` when the application is running for complete interactive documentation.

## 🤝 Contributing

We welcome contributions! Here's how you can help:

1. **Fork the repository**
2. **Create a feature branch**
   ```bash
   git checkout -b feature/amazing-feature
   ```
3. **Commit your changes**
   ```bash
   git commit -m 'Add some amazing feature'
   ```
4. **Push to the branch**
   ```bash
   git push origin feature/amazing-feature
   ```
5. **Open a Pull Request**

### Development Guidelines

- Follow C# coding conventions
- Write unit tests for new features
- Update documentation as needed
- Ensure all tests pass before submitting PR

## 📄 License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT) - see the [LICENSE](LICENSE) file for details.

---

<div align="center">

**Built with ❤️ by the ExcellyGen Team**

[Report Bug](https://github.com/udaraexcellygen/excellygen-lms-backend/issues) • [Request Feature](https://github.com/udaraexcellygen/excellygen-lms-backend/issues) • [Documentation](https://github.com/udaraexcellygen/excellygen-lms-backend/wiki)

</div>
