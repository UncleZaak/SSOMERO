# Ssomero Architecture

## Overview

Ssomero is a .NET MAUI (.NET 10) cross-platform application following the **MVVM pattern** with **Dependency Injection** and **Shell Navigation**.

## Layers

```
????????????????????????????????????????
?          Views (XAML + Code-behind)  ?  UI layer — data binding only
????????????????????????????????????????
?          ViewModels                  ?  Presentation logic, state, commands
????????????????????????????????????????
?          Services                    ?  Business logic, API calls
????????????????????????????????????????
?          Interfaces                  ?  Contracts (testability)
????????????????????????????????????????
?          Models / DTOs               ?  Data transfer objects
????????????????????????????????????????
```

## Navigation

Shell-based navigation defined in `AppShell.xaml`:

- **LoginPage** — displayed outside TabBar (no tabs visible)
- **MainTabs** — TabBar with Dashboard, Courses, Announcements
- **course-detail** — registered as a programmatic route

### Auth Guard Flow

```
App.CreateWindow()
  ?? Shell.Loaded
       ?? Token exists + not expired ? GoToAsync("//MainTabs")
       ?? No token / expired        ? stays on LoginPage
```

## Authentication

1. User logs in ? `POST auth/login` ? stores JWT + refresh token in `SecureStorage`
2. Every API call attaches `Authorization: Bearer <token>` via per-request `HttpRequestMessage`
3. On 401 response ? `ApiService` acquires `SemaphoreSlim` lock ? calls `POST auth/refresh`
4. On refresh success ? retries original request
5. On refresh failure ? clears tokens ? navigates to `//LoginPage`

## Dependency Injection

All services, ViewModels, and Pages are registered in `MauiProgram.cs`:

- **Singletons**: HttpClient, ApiService, AuthService, TokenStorageService, domain services
- **Transient**: ViewModels, Pages (fresh instance per navigation)

## Configuration

Embedded `appsettings.json` files loaded via `Microsoft.Extensions.Configuration.Json`:

- `appsettings.json` — production defaults
- `appsettings.Development.json` — debug overrides (included only in Debug builds)

Strongly typed via `ApiSettings` class.

## Logging

`ILogger<T>` injected into all services. Structured log messages use `LogWarning` / `LogError` with message templates. No sensitive data (passwords, tokens) is logged.

## Error Handling

- **Services**: Return empty/null on API failures; log warnings
- **ViewModels**: Wrap `LoadAsync` in try/catch; surface `ErrorMessage` property
- **ApiService**: Catches `HttpRequestException` and `TaskCanceledException`; logs and re-throws
- **App.xaml.cs**: Global handlers for `AppDomain.UnhandledException` and `TaskScheduler.UnobservedTaskException`

## Cancellation

`BaseViewModel` exposes `CreateLinkedToken()` and `CancelPendingRequests()`. Pages call cancel from `OnDisappearing()` to abort in-flight HTTP requests when navigating away.
