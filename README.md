# Secure Banking Authentication System

A secure online banking authentication system designed to simulate real-world financial security practices. This project focuses on multi-factor authentication (MFA), fraud detection, and clean software architecture.

---

## Overview

With the rise of digital banking, protecting user accounts from unauthorized access and fraud is critical. This system demonstrates how modern banking applications secure authentication flows and detect suspicious activity using rule-based logic.

---

## Features/Key Highlights

-  User Registration & Login
-  Multi-Factor Authentication (OTP-based)
-  Rule-Based Fraud Detection
-  Location-Based Login Monitoring
-  Transaction Handling System
-  Transaction History Tracking
-  Fraud Alerts Logging
-  Admin Monitoring Capabilities
-  Clean Architecture using SOLID principles

---

## How It Works

1. User logs in using username and password
2. System verifies credentials via AuthService
3. OTP is generated and required for verification (MFA)
4. System retrieves user location via IP
5. FraudDetectionService compares login location with home location
6. Suspicious activity is logged as a fraud event
7. User gains access only after successful OTP verification

---

## Threat Model

| Threat                    | Mitigation                  |
|---------------------------|-----------------------------|
| Brute force attack        | Login attempt tracking      |
| Credential theft          | Multi-Factor Authentication |
| Session hijacking         | Controlled session flow     |
| Fraudulent login location | Location-based detection    |
| Suspicious transactions   | Rule-based fraud detection  |

---

## System Architecture

The system follows a modular and maintainable architecture:

- **Frontend**: .NET MAUI (UI Layer)
- **Backend Logic**: Service-based architecture
- **Design Pattern**: MVC-inspired separation
- **Core Services**:
  - `AuthService` → Handles authentication & OTP
  - `FraudDetectionService` → Detects suspicious behavior
  - Transaction handling logic

---

## Security Features

### Multi-Factor Authentication (MFA)
- Username + Password
- One-Time Password (OTP) verification

### Fraud Detection Rules
- Login from unusual location
- Multiple failed login attempts
- Suspicious transaction patterns

### Secure Practices
- Password hashing (no plain text storage)
- Input validation
- Error handling
- Separation of concerns

---

## Fraud Detection Logic

The system flags suspicious activity using:

| Scenario | Action |
|--------|--------|
| Unknown login location | Logged as fraud |
| Rapid login attempts | Tracked |
| Large transactions | Flagged |

---

## Testing & Validation

- Simulated login attacks
- Invalid credential testing
- Fraud scenario simulation
- Input validation testing

---

## Demo

### Key Screens:
- Login Page
- OTP Verification
- Dashboard
- Transactions
- Fraud Alerts

_(Add screenshots here later)_

---

## Technologies Used

- C#
- .NET MAUI
- REST principles
- JSON APIs
- UML (System Design)
- Secure coding practices

---

## Documentation

- UML Diagrams (Use Case, Sequence, ERD)
- [System Design](documentation/system-design.md)
- [Security Model](documentation/security-model.md)
- [Code Architecture](documentation/code-architecture.md)
- [Engineering Decisions](documentation/engineering-decisions.md)


---

## Future Improvements

- Machine Learning-based fraud detection
- Biometric authentication
- JWT-based session management
- Cloud deployment
- Microservices architecture

---

## Disclaimer

This is a prototype system built for educational purposes and does not represent a production-ready banking system.

---

## Author

Shivam Parab

---

## If you found this useful, consider starring the repo!