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
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- IDE: Visual Studio 2022

---

### 🐳 Run with Docker (Local PostgreSQL)

To run the application with Docker using your **local PostgreSQL**:

#### Step 1: **Clone the repository** (if not already done)
```bash
git clone https://github.com/Sambit-Kumar-2001/9dotTask.git
cd 9dotTask
```

#### Step 2: **Create a Dockerfile**  
In your project directory, create a `Dockerfile` with the following content:

#### Step 3: **Create `.dockerignore`**
```bash
touch .dockerignore
```

Add the following content:

```
bin/
obj/
.vscode/
*.user
*.suo
*.md
Dockerfile
docker-compose.yml
```

#### Step 4: **Create `docker-compose.yml`**

```bash
touch docker-compose.yml
```

Add this content to the `docker-compose.yml`:

```yaml
services:
  webapp:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__EmployeeManagementConnection=Host=host.docker.internal;Port=5432;Database=yourdatabase;Username=username;Password=<your password>
```

> **Note**: The application will connect to your **local PostgreSQL** using `host.docker.internal`.

#### Step 5: **Configure `appsettings.json`**

In the `appsettings.json`, update the connection string:

```json
"ConnectionStrings": {
  "EmployeeManagementConnection": "Host=host.docker.internal;Port=5432;Database=yourdatabase;Username=username;Password=<your password>"
}
```

### 🖥️ Run the Application

#### Step 6: **Build and Run with Docker**

Run the following command to start the Docker containers in detached mode:

```bash
docker-compose up --build -d
```

This will build the image, start the web application container, and run it in the background.

#### Step 7: **Access the Application**

After the Docker containers are running, open your browser and visit:

```
http://localhost:8080
```

---

### 🚨 Troubleshooting

- **Docker Not Running**: Ensure **Docker Desktop** is running and properly configured on your system.
- **PostgreSQL Not Connecting**: Make sure your local PostgreSQL allows external connections and the credentials in your `appsettings.json` match.
- **Container Logs**: If you need to check logs:

```bash
docker-compose logs -f
```

---

### 🛑 Stop the Docker Containers

To stop and remove the containers:

```bash
docker-compose down
```

---

## ✨ Other Setup Options

If you wish to run the app **without Docker**, follow the below steps to configure the app with **local PostgreSQL**:

1. **Install PostgreSQL** locally on your machine.
2. **Update the connection string** in `appsettings.json` to match your local PostgreSQL configuration:

```json
"ConnectionStrings": {
  "EmployeeManagementConnection": "Host=localhost;Port=5432;Database=yourdatabase;Username=username;Password=<your password>"
}
```

3. **Run the application locally**:

```bash
dotnet run
```

Visit [http://localhost:5000](http://localhost:5000) to access the app.

---
