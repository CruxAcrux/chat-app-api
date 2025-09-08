# ChatApp Backend

A real-time chat application backend built with ASP.NET Core, SignalR, and PostgreSQL. It handles user authentication, friend management, real-time messaging, image uploads, and password reset via email. This repository contains only the backend; the frontend is hosted in a separate repository.

## Features
- User authentication (login, registration, password reset via email)
- Friend search and management
- Real-time messaging with text and images using SignalR
- Image upload and storage
- Email notifications for password reset

## Prerequisites
To run the ChatApp backend, ensure you have the following installed:

### Software and Tools
- **.NET SDK**:
  - Version: 9.0 or later
  - Install: [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0)
  - Verify:
    ```bash
    dotnet --version
    ```

- **PostgreSQL**:
  - Version: 13 or later
  - Install: [postgresql.org](https://www.postgresql.org/download/) or use `brew install postgresql` (macOS), `apt install postgresql` (Ubuntu)
  - Verify:
    ```bash
    psql --version
    ```

- **Code Editor**:
  - Recommended: [Visual Studio Code](https://code.visualstudio.com/) with extensions:
    - C#
    - PostgreSQL (optional)

- **Frontend Client**:
  - The backend requires a running ChatApp frontend (React with TypeScript) at `http://localhost:3000`.
  - See the [ChatApp Frontend repository](https://github.com/CruxAcrux/ChatApp-Frontend) for frontend setup.

- **Gmail Account**:
  - Required for sending password reset emails.
  - Enable 2-Step Verification and generate an App Password via [Google Account > Security > App Passwords](https://myaccount.google.com/security).

### Additional Setup
- **PostgreSQL Database**:
  - Create a database named `chatappdb`:
    ```sql
    CREATE DATABASE chatappdb;
    ```
  - Configure with username `postgres` and a secure password.
  - Verify connection:
    ```bash
    psql -h localhost -U postgres -d chatappdb
    ```

- **Static Files**:
  - Ensure the `wwwroot/Uploads` folder is writable for image storage:
    ```bash
    chmod -R u+w ChatApp/wwwroot/Uploads
    ```

## Installation
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/AcruxAcrux/chat-app-api.git
   cd ChatApp-Backend
   ```

2. **Install Dependencies**:
   ```bash
   dotnet restore
   ```
   - Installs `Microsoft.AspNetCore.Identity`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `MailKit`, and other dependencies listed in `ChatApp.csproj`.

3. **Apply Database Migrations**:
   ```bash
   dotnet ef database update
   ```
   - Sets up the `chatappdb` schema for users, messages, and password reset tokens.

4. **Start the Backend**:
   ```bash
   dotnet run
   ```
   - Runs the app at `http://localhost:5072`.

## Configuration
Update `ChatApp/appsettings.json` with the following:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=chatappdb;Username=postgres;Password=your_postgres_password"
  },
  "Jwt": {
    "Key": "your_secure_jwt_key_32_chars_long!!",
    "Issuer": "ChatApp",
    "Audience": "ChatAppUsers"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderName": "ChatApp",
    "SenderEmail": "your-email@gmail.com",
    "Password": "your_gmail_app_password"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Replace:**
- `your_postgres_password`: Your PostgreSQL password.
- `your_secure_jwt_key_32_chars_long!!`: A 32-character secret key.
- `your-email@gmail.com`: Your Gmail address.
- `your_gmail_app_password`: Your Gmail App Password.

The backend serves API and SignalR at `http://localhost:5072`. Ensure the frontend is configured to connect to this URL.

## Usage
1. **Start the Backend**:
   - Run `dotnet run` to start the server at `http://localhost:5072`.

2. **API Endpoints**:
   - Register: `POST /api/auth/register`
   - Login: `POST /api/auth/login`
   - Password Reset Request: `POST /api/auth/request-password-reset`
   - Reset Password: `POST /api/auth/reset-password`
   - Search Users: `GET /api/chat/search-users?query={query}`
   - Add Friend: `POST /api/chat/add-friend`
   - Get Friends: `GET /api/chat/friends`
   - Get Messages: `GET /api/chat/messages/{friendId}`
   - Upload Image: `POST /api/chat/upload-image`

3. **SignalR Hub**:
   - Connect to `http://localhost:5072/chatHub` for real-time messaging.

4. **Email Notifications**:
   - Password reset emails are sent via Gmail SMTP when users request a reset.

## Known Issues
- **Image Uploads**: Images may fail to upload or save correctly, resulting in empty messages in the database. This is being investigated.
- **Email Notifications**: Password reset emails may not send if Gmail SMTP settings are incorrect. Check `appsettings.json` and backend logs for errors.
