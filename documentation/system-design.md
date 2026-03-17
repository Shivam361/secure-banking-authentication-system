# System Design

## Overview

The Secure Banking Authentication System is designed using a modular, service-based architecture to ensure maintainability, scalability, and separation of concerns. The system simulates a real-world banking authentication workflow with integrated fraud detection.

---

## Architecture Style

The system follows a **layered architecture** inspired by MVC principles:

### Layers:

1. **Presentation Layer (UI)**
   - Built using .NET MAUI
   - Handles user interaction (Login, OTP, Dashboard)

2. **Service Layer (Business Logic)**
   - Contains core application logic
   - Includes:
     - `AuthService`
     - `FraudDetectionService`

3. **Data Layer**
   - Handles database operations
   - Stores users, transactions, and fraud logs

---

## Core Components

### 1. AuthService
Responsible for:
- User authentication
- Password verification
- OTP generation and validation

### 2. FraudDetectionService
Responsible for:
- Logging login attempts
- Detecting suspicious login behaviour
- Comparing login location with home location
- Recording fraud events

### 3. User Model
Stores:
- Username
- Password (hashed)
- Home location
- Account balance

### 4. Transaction Logic
Handles:
- Sending money
- Receiving money
- Recording transaction history

---

## Authentication Flow

1. User enters username and password
2. `AuthService` verifies credentials
3. System logs login attempt
4. User location is retrieved via IP
5. `FraudDetectionService` checks for anomalies
6. OTP is generated and sent to user
7. User enters OTP
8. Access is granted upon successful verification

---

## Data Flow

- UI sends input → Service Layer
- Service Layer processes logic → Data Layer
- Data Layer returns results → Service Layer
- Service Layer updates UI

---

## Fraud Detection Flow

1. Login attempt occurs
2. IP address is retrieved
3. Location is resolved via geolocation API
4. System compares:
   - Current login location
   - Stored home location
5. If mismatch:
   - Fraud event is logged
   - User/admin can view alert

---

## Design Principles Used

### SOLID Principles

- **Single Responsibility**
  Each service has a clear role (Auth vs Fraud)

- **Open/Closed Principle**
  System can be extended (e.g., adding ML fraud detection)

- **Dependency Injection**
  Services are injected using .NET DI container

---

## Scalability Considerations

Although this is a prototype, the design allows:

- Separation into microservices (Auth, Fraud, Transactions)
- Integration with cloud services
- Expansion of fraud detection logic

---

## Limitations

- Single database (no distributed system)
- Rule-based fraud detection only
- No real-time notification system
- No external authentication provider (e.g., OAuth)

---

## Future Enhancements

- Convert to microservices architecture
- Add API gateway
- Implement JWT authentication
- Introduce event-driven fraud detection
- Deploy to cloud platform (Azure/AWS)

---

## Summary

The system demonstrates a clean and modular architecture that separates concerns between UI, business logic, and data handling. This makes the system easier to maintain, test, and extend, reflecting real-world software engineering practices.