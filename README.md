# Ampere Chat — Real-Time Cross-Platform Messaging Platform

![C](https://img.shields.io/badge/C%23-.NET%208-blue)
![ASPNET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API%20%26%20SignalR-purple)
![NET MAUI](https://img.shields.io/badge/.NET%20MAUI-Cross--Platform-512BD4)
![MySQL](https://img.shields.io/badge/MySQL-Relational%20DB-4479A1)
![JWT](https://img.shields.io/badge/JWT-Authentication-black)

**Ampere Chat** is a high-performance, real-time messaging ecosystem built with a decoupled architecture. It features a lightweight **ASP.NET Core** backend powering WebSocket communication via **SignalR**, paired with a modern **.NET MAUI** cross-platform client (Android, iOS, Windows, macOS).

---

## 🌟 Architecture & Key Features

### Backend (ASP.NET Core / Web API / SignalR)

- **Real-Time Communication:** Full-duplex messaging pipeline powered by **ASP.NET Core SignalR**.
- **Stateless Authentication:** Secure **JWT Bearer** authentication with custom token extraction algorithms during WebSocket handshake initialization (`OnMessageReceived`).
- **High-Performance Data Access:** Lightweight data mapping using **Dapper** and raw optimized SQL queries for rapid message history retrieval and low latency.
- **RESTful Endpoints:** Fully documented Swagger/OpenAPI endpoints for user authorization, chat generation, and message delivery fallback.
- **Relational Data Storage:** Structured relational database schemas in **MySQL** managing user profiles, chat rooms, and temporal message indexes.

### Client (.NET MAUI)

- **Cross-Platform Interface:** Single C# codebase targeting desktop and mobile operating systems.
- **Resilient Reconnection Network Layer:** Custom implementation of `IRetryPolicy` with exponential backoff algorithms (1s, 5s, 15s, 30s) ensuring automatic recovery during transient connection drops.
- **Event-Driven UI:** Decoupled Architecture separating UI presentation logic from the underlying `SignalRService` network handling.
- **Secure Token Storage:** Encrypted client-side persistent storage using platform-native secure storage for JWT lifecycle management.

---

## 🛠 Tech Stack

- **Language:** C# 12 / .NET 8
- **Backend Framework:** ASP.NET Core Web API, SignalR Hubs
- **Client Framework:** .NET MAUI
- **Data Access & Database:** Dapper, Entity Framework Core (Migrations), MySQL
- **Security & Auth:** System.IdentityModel.Tokens.Jwt, BCrypt / Claims-based Authorization
- **Tools & API Docs:** Swagger UI, Postman, Visual Studio 2022

---

## 📐 System Architecture Overview

```
[ .NET MAUI Client ] ──( HTTP / REST API )──> [ ASP.NET Core API ] ──> [ MySQL Database ]
         │                                            │
         └─────────────( WebSockets / SignalR )───────┘
```

1. **Authentication Flow:** Client requests JWT token via standard HTTP POST `/api/auth/login`.
2. **Handshake:** Token is passed via query parameters (`/chathub?access_token=...`) to establish an authorized WebSocket hub connection.
3. **Messaging Pipeline:** Real-time messages are handled via SignalR groups (`chat_{chatId}`) with background database logging.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (with `.NET MAUI` and `ASP.NET & Web Development` workloads)
- MySQL Server 8.0+

### 1. Database Setup

Execute the initial SQL script or run EF Core migrations to configure tables:

- `User` (user_id, username, password_hash)
- `Chat` (chat_id, display_name, is_group)
- `Message` (message_id, chat_id, user_id, text, sent_at)

### 2. Backend Configuration

Update `appsettings.json` in `AmpereChat.API`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ampere_chat;Uid=root;Pwd=yourpassword;"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SUPER_SECRET_STRONG_KEY_HERE"
  }
}
```

Run the backend server:

```bash
cd AmpereChat.API
dotnet run
```

### 3. Client Setup

Configure the API endpoint in `ApiService.cs`:

```csharp
private const string BaseUrl = "http://localhost:5000"; // Or deployed host URL
```

Build and deploy the application via Visual Studio targeting Android, Windows, or iOS.

---

## 💡 Engineering Highlights

- **Handshake Security Hack:** Standard WebSocket API limits header injection during initial connection. Implemented middleware parsing to dynamically extract Bearer tokens from query string parameters safely.
- **Fault-Tolerant Sockets:** Mitigated network instability in mobile network environments through dynamic state handling (`Closed`, `Reconnecting`, `Reconnected` event hooks).
