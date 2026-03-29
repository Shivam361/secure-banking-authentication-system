# Future Updates & Improvement Roadmap

This document outlines planned improvements for the Secure Banking Authentication System, organized by priority and impact.

---

## Phase 1 — Critical Security Fixes

### 1.1 Replace SHA-256 with a Proper Password Hashing Algorithm
- **Current:** Passwords are hashed using raw `SHA256`, which is a fast general-purpose hash — easily brute-forced at billions of attempts per second on modern GPUs.
- **Planned:** Migrate to `BCrypt`, `Argon2`, or `PBKDF2` (`Rfc2898DeriveBytes` in .NET) with sufficient work factors and per-user salts.
- **Impact:** Eliminates the most critical security vulnerability in the system.

### 1.2 Implement Real OTP Delivery
- **Current:** The OTP code is displayed directly in a `DisplayAlert` dialog after login, defeating the purpose of multi-factor authentication.
- **Planned:** Send OTP via email (or SMS) using a mock or real delivery service. Display only a confirmation message: *"An OTP has been sent to your registered email."*
- **Impact:** Ensures MFA functions as a true second factor.

### 1.3 Enforce HTTPS for External API Calls
- **Current:** The geolocation lookup (`ip-api.com`) is called over plain `HTTP`, exposing the user's IP address in transit.
- **Planned:** Switch to an HTTPS-enabled geolocation provider or upgrade the API tier.

### 1.4 Secure Admin Credential Seeding
- **Current:** Admin credentials (`"admin"` / `"adminpass"`) are hardcoded in two separate locations (`MauiProgram.cs` and `MainPage.xaml.cs`).
- **Planned:** Consolidate seeding to a single location, load default credentials from a configuration/secrets file, and remove the duplicate in `MainPage`.

### 1.5 Role-Based Access Control
- **Current:** Admin access is determined by checking `if (user.Username == "admin")` — a fragile magic-string approach.
- **Planned:** Add an `IsAdmin` boolean or `Role` enum property to the `User` model and enforce role checks through it.

---

## Phase 2 — Architecture & Dependency Injection

### 2.1 Fix Service Lifetime Mismatch
- **Current:** `AuthService` and `FraudDetectionService` are registered as **Singleton**, but `AppDbContext` is **Scoped**. This causes the singletons to hold a stale/disposed DbContext.
- **Planned:** Re-register services as `Scoped` or `Transient` to match the DbContext lifetime.

### 2.2 Eliminate Service Locator Anti-Pattern
- **Current:** Most pages resolve dependencies via `MauiProgram.ServiceProvider.GetRequiredService<T>()` instead of using constructor injection.
- **Planned:** Refactor all pages to accept services through constructor parameters (pages are already registered in DI).

### 2.3 Extract Service Interfaces
- **Current:** `AuthService` and `FraudDetectionService` are concrete classes with no interface abstractions.
- **Planned:** Extract `IAuthService` and `IFraudDetectionService` interfaces to enable unit testing with mocks and true dependency inversion (SOLID principles).

### 2.4 Remove Duplicated Network Helper Logic
- **Current:** `LoginPage.xaml.cs` contains private copies of `GetPublicIpAsync()` and `GetGeoLocationAsync()` — identical to the code already in `NetworkHelper.cs`.
- **Planned:** Delete the duplicate methods and use `NetworkHelper` (or an injected service) everywhere.

---

## Phase 3 — Data & Logic Bug Fixes

### 3.1 Fix HomeLocation Registration Bug
- **Current:** During registration, the geo-location result is fetched but immediately overwritten with the raw IP address (`homeLocation = ip;`). This means fraud detection compares an IP string against a city string — they never match, causing every login to be falsely flagged.
- **Planned:** Remove the overwriting line so the actual geo-location (e.g., `"London, UK"`) is stored.

### 3.2 Standardize DateTime Usage
- **Current:** `LoginAttempt.Timestamp` uses `DateTime.Now` (local time) while every other model uses `DateTime.UtcNow`. This mismatch breaks time-window queries in fraud detection.
- **Planned:** Standardize all timestamps to `DateTime.UtcNow`.

### 3.3 Fix OTP Validation Order
- **Current:** `ValidateOtp()` queries for a matching OTP, then runs a cleanup of all expired OTPs (which may delete the matched one), then checks if the query returned null.
- **Planned:** Reorder logic — validate and consume the OTP first, then clean up expired records.

### 3.4 Add Password Strength Validation
- **Current:** Registration accepts any password, including empty or single-character strings.
- **Planned:** Enforce minimum length (e.g., 8 characters), require mixed case, digits, and special characters.

### 3.5 Add Email Validation
- **Current:** Registration accepts any string as an email address.
- **Planned:** Add regex or `MailAddress` validation to ensure proper email format.

---

## Phase 4 — Code Quality & Cleanup

### 4.1 Fix Code Formatting Consistency
- Normalize indentation across all files (particularly `AuthService.cs` which has mixed indentation levels from copy-paste).

### 4.2 Remove Duplicate `using` Statements
- `AppDbContext.cs`: `Microsoft.Maui.Storage` and `System.IO` imported twice
- `MainPage.xaml.cs`: `Microsoft.Maui.Controls` imported twice
- `Transaction.cs`: `System` imported twice
- `NetworkHelper.cs`: `System.Threading.Tasks` imported twice

### 4.3 Remove Platform-Specific Imports from Shared Code
- `FraudDetectionService.cs` and `AppDbContext.cs` contain `using Windows.System;` which locks the code to Windows and will fail on Android/iOS/Mac targets.

### 4.4 Remove Dead & Debug Code
- Commented-out debug `DisplayAlert` in `LoginPage.xaml.cs`
- Unused `dashboard` variable in `OtpPage.xaml.cs`
- Double-resolved `TransactionPage` in `MainPage.xaml.cs`

### 4.5 Use Shared HttpClient Instance
- **Current:** `new HttpClient()` is created inside every method call, exhausting socket handles under load.
- **Planned:** Use a single static `HttpClient` or `IHttpClientFactory`.

### 4.6 Remove Unused NuGet Package
- `EntityFramework 6.5.1` (EF6 legacy) is referenced alongside `Microsoft.EntityFrameworkCore 8.0.1`. EF6 is completely unused and only bloats the build.

---

## Phase 5 — Feature Enhancements

### 5.1 Session Management & Logout
- Implement a proper logout flow that clears the authentication state.
- Add session timeout/expiry so users aren't permanently authenticated.

### 5.2 Transaction Re-Authentication
- Require re-authentication (PIN or OTP) for transactions above a configurable threshold.

### 5.3 OTP Rate Limiting
- Limit the number of OTP verification attempts (e.g., 5 tries) to prevent brute-force attacks on the 6-digit code.

### 5.4 Audit Trail & Access Logging
- Log when admin pages are accessed, who viewed what data, and when.

### 5.5 Encrypted Data at Rest
- Encrypt the SQLite database file to protect sensitive data stored on disk.

### 5.6 Machine Learning–Based Fraud Detection
- Move beyond rule-based detection to ML models that can identify complex behavioural patterns (transaction velocity, anomalous amounts, unusual timing).

### 5.7 Biometric Authentication
- Integrate fingerprint or face ID as an additional or alternative authentication factor using device-native APIs.

### 5.8 JWT-Based Session Management
- Replace the current in-memory session tracking with JSON Web Tokens for stateless, secure session handling.

---

## Phase 6 — Project & DevOps Polish

### 6.1 Add Unit Tests
- Create a test project with tests covering `AuthService`, `FraudDetectionService`, model validation, and fraud detection rules.

### 6.2 CI/CD Pipeline
- Add a GitHub Actions workflow for automated build, test, and lint on every push/PR.

### 6.3 Update `.gitignore`
- Expand the current `.gitignore` (31 bytes) to properly exclude `bin/`, `obj/`, `.vs/`, `*.user`, `*.db`, and other build artifacts.

### 6.4 Add Screenshots to README
- Replace the `_(Add screenshots here later)_` placeholder with actual application screenshots demonstrating key flows.

### 6.5 Cloud Deployment
- Explore cloud hosting for the backend services to support multi-device access and real-time monitoring.

### 6.6 Microservices Architecture
- Decompose the monolithic service layer into independent microservices for authentication, fraud detection, and transaction processing.

---

## Priority Summary

| Phase | Focus | Effort | Impact |
|-------|-------|--------|--------|
| **1** | Critical security fixes | Medium | 🔴 Critical |
| **2** | Architecture & DI cleanup | Medium | 🟠 High |
| **3** | Data & logic bug fixes | Low | 🟡 High |
| **4** | Code quality & cleanup | Low | 🔵 Medium |
| **5** | Feature enhancements | High | 🟣 Medium |
| **6** | DevOps & polish | Medium | ⚪ Low–Medium |

---

> **Note:** Phases 1–3 should be completed before the project is shared publicly or included in a portfolio. The password hashing issue and the HomeLocation bug are the highest-priority items.
