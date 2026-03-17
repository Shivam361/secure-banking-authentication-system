# Security Model

## Overview

This system was designed with a security-first approach to simulate how real-world banking applications protect user data and prevent unauthorized access. The model focuses on authentication security, fraud detection, and risk mitigation using industry-inspired practices.

---

## Security Objectives

The key objectives of the system are:

- Prevent unauthorized account access
- Detect and log suspicious behaviour
- Protect user credentials
- Ensure secure authentication flow
- Maintain system integrity under attack scenarios

---

## Authentication Security

### Password-Based Authentication
- Users authenticate using a username and password
- Passwords are securely handled and never stored in plain text
- Input validation is applied to prevent malformed data

### Multi-Factor Authentication (MFA)
- After successful password verification, an OTP is generated
- The user must enter the OTP to complete login
- This prevents unauthorized access even if credentials are compromised

---

## Fraud Detection Strategy

The system implements a **rule-based fraud detection model**.

### Key Detection Rules:

#### 1. Location-Based Detection
- The system retrieves the user's login location using IP-based geolocation
- This is compared with the stored "home location"
- If the locations differ significantly, the activity is flagged as suspicious

#### 2. Login Attempt Monitoring
- All login attempts are recorded
- Failed attempts can indicate brute force attacks
- These are logged for monitoring and analysis

#### 3. Suspicious Behaviour Logging
- Any detected anomaly is stored in fraud logs
- Admins and users can view fraud alerts

---

## Threat Model

| Threat | Description | Mitigation |
|------|------------|-----------|
| Brute Force Attack | Multiple password attempts to gain access | Login attempt tracking |
| Credential Theft | Stolen username/password | MFA (OTP verification) |
| Unauthorized Login | Login from unknown location | Location-based fraud detection |
| Session Abuse | Attempt to bypass authentication flow | Controlled login + OTP flow |
| Suspicious Transactions | Unusual financial behaviour | Rule-based detection |

---

## Secure Design Principles

The system applies several secure software engineering practices:

### Separation of Concerns
- Authentication logic handled by `AuthService`
- Fraud detection handled by `FraudDetectionService`

### Least Privilege (Conceptual)
- Users only access their own data
- Admin functionality is separated

### Input Validation
- User inputs are sanitized and validated

### Error Handling
- Errors do not expose sensitive system details

---

## Security Testing

The system was tested using simulated attack scenarios:

- Invalid login attempts
- Repeated failed authentication
- Login from different locations
- Fraud scenario triggering

These tests validate that the system responds correctly to suspicious activity.

---

## Design Decisions

### Why Rule-Based Fraud Detection?
- Simpler to implement for a prototype
- Easy to understand and explain
- Provides clear and predictable behavior

### Limitations
- Cannot detect complex fraud patterns
- No machine learning or behavioural analysis
- Limited to predefined rules

---

## Future Improvements

- Machine learning-based fraud detection
- Risk scoring system for login attempts
- Biometric authentication (fingerprint/face ID)
- JWT-based secure session management
- Cloud-based monitoring and alerting systems

---

## Summary

This security model demonstrates how core banking security principles can be applied in a prototype system. While simplified, it reflects real-world practices such as multi-factor authentication, anomaly detection, and secure system design.