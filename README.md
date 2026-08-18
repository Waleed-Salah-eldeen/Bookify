# 📚 Bookify: Library Management System

A modern library management system built with **ASP.NET Core MVC** for managing books, users, and borrow/return operations.

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-Language-239120?logo=csharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC292B?logo=microsoftsqlserver&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?logo=bootstrap&logoColor=white)
![Hangfire](https://img.shields.io/badge/Hangfire-Background%20Tasks-FF4500)

---

## 📸 Screenshots

### 🔐 Authentication
<table>
  <tr>
    <td width="50%" align="center"><b>Login</b></td>
    <td width="50%" align="center"><b>Email Confirmation</b></td>
  </tr>
  <tr>
    <td><img src="screenshots/p1.png" width="100%" alt="Login"></td>
    <td><img src="screenshots/p2.png" width="100%" alt="Email Confirmation"></td>
  </tr>
</table>

<br>

### 👤 Profile
<table>
  <tr>
    <td width="50%" align="center"><b>Profile</b></td>
    <td width="50%" align="center"><b>Profile</b></td>
  </tr>
  <tr>
    <td><img src="screenshots/p3.png" width="100%" alt="Profile"></td>
    <td><img src="screenshots/p4.png" width="100%" alt="Profile"></td>
  </tr>
</table>

---

## ✨ Features

### ✅ Implemented
- 📖 **Book Management**
- 🔐 **User Authentication**
- 👥 **Role-Based Access Control** (Admin, Receptionist, Archivist)
- 🔄 **Borrow/Return System with subscriber management**
- ☁️ **Cloud-hosted book covers**
- ⚙️ **Automated background tasks using Hangfire**
- 🔍 **Advanced search & filtering**
- 📊 **Export reports (PDF/Excel)**

### 🔜 Planned
- Admin dashboard with analytics
- Apply the Repository pattern

---

## 🧰 Technology Stack

### Backend
| Technology |
| --- |
| ASP.NET Core MVC |
| Entity Framework Core |
| LINQ |
| AutoMapper \| Hangfire \| Data Protection API |
| SQL Server |
| ASP.NET Core Identity |

### Frontend
| Layer | Technology |
| --- | --- |
| **UI** | Razor Pages with Bootstrap 5, HTML and CSS |
| **Client-side** | jQuery |