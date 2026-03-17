# Code Architecture

## Overview

The system is structured using a service-based architecture to ensure separation of concerns and maintainability.

---

## Core Services

### AuthService

Handles authentication logic:

- Verifies username and password
- Generates OTP for MFA
- Validates OTP

Example responsibilities:

- `VerifyPassword()`
- `GenerateOtp()`

---

### FraudDetectionService

Handles fraud detection:

- Logs login attempts
- Detects unusual login locations
- Records fraud events

Example responsibilities:

- `LogAttempt()`
- `LocationsAreSimilar()`
- `LogFraud()`

---

## Login Flow in Code

1. User enters credentials
2. `AuthService.VerifyPassword()` is called
3. Login attempt is logged
4. IP and location are retrieved
5. `FraudDetectionService` checks for anomalies
6. OTP is generated
7. User verifies OTP
8. Navigation to dashboard

---

## Key Design Choices

- Services separated for clarity
- Dependency Injection used for flexibility
- Business logic kept out of UI layer

---

## 📌 Summary

The code structure ensures modularity, readability, and scalability, making it easier to extend and maintain.