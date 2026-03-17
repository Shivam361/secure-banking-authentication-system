# Engineering Decisions

## Overview

This document outlines the key technical decisions made during the development of the Secure Banking Authentication System.

---

## Why Multi-Factor Authentication (MFA)?

- Passwords alone are vulnerable to attacks
- MFA significantly reduces unauthorized access
- Simulates real-world banking security

---

## Why Rule-Based Fraud Detection?

- Simpler to implement in a prototype
- Easy to explain and debug
- Provides deterministic behaviour

---

## Why Service-Based Architecture?

- Separates authentication and fraud logic
- Improves maintainability
- Makes the system easier to scale

---

## Why .NET MAUI?

- Cross-platform capability
- Strong integration with C#
- Suitable for rapid prototyping

---

## Why Dependency Injection?

- Reduces tight coupling
- Makes testing easier
- Aligns with modern .NET practices

---

## Trade-Offs Considered

| Decision | Trade-Off |
|--------|----------|
| Rule-based fraud detection | Less accurate than ML |
| Simple architecture | Not fully scalable |
| OTP via display | Not production-ready |

---

## Future Improvements

- Replace rule-based system with ML
- Add real OTP delivery (SMS/Email)
- Implement JWT authentication
- Move to microservices architecture

---

## 📌 Summary

The system was designed with simplicity, clarity, and security in mind while making realistic trade-offs appropriate for a prototype.