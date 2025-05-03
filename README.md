# 9dotTask
# Employee Record Management System

A clean, scalable ASP.NET Core Web Application for managing Employee Records using Repository Pattern and PostgreSQL. This application supports full CRUD operations along with image upload, custom validation, and reporting.

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core (.NET 7)
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Frontend**: MVC Views (JS for form validation)
- **Design Pattern**: Repository Pattern
- **Image Handling**: IFormFile + Server Storage
- **Validation**: Server-side & Client-side (DataAnnotations, JS/jQuery)
- **Reporting**: Paginated, searchable report by date range

---

## 📋 Features

### Employee Record Management
- **Auto-generated Employee Code**
- **Photo Upload**: Accepts only `.jpg`/`.png` (Max size configurable)
- **Validations**:
  - Name: No special characters or digits
  - DOB: Must be ≥ 18 years old, not in the future
  - Age: Auto-calculated from DOB (read-only)
  - Gender: Dropdown (Male/Female)
  - Contact: Numeric, exactly 10 digits
  - Email: Proper format required
- **Education**:
  - Degree
  - Year of Passing
  - Percentage
  - Supports multiple entries per employee

### Reporting
- Generate Employee Report for specified date range
- Pagination & Search support

### Miscellaneous
- Prevent duplicate records
- Mandatory fields: Name, DOB, Contact Number

---

## 🏁 Getting Started

### Prerequisites
- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download)
- [PostgreSQL](https://www.postgresql.org/)
- IDE: Visual Studio 2022
