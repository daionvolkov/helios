# ☀️ Helios Platform

**Helios** is a self-hosted infrastructure management platform designed to centrally manage servers, agents, monitoring, and remote operations.

It is built as a modular backend system for:

- infrastructure management
- remote command execution
- server monitoring
- operational automation
- building SaaS services on top of managed resources

Helios is designed as a **production-ready multi-tenant orchestration platform**.

---

## 🚀 Current Status

### ✅ Implemented

#### Core Platform
- Identity & JWT authentication
- Multi-tenant isolation
- Tenant context extraction from token
- Swagger API documentation

#### Platform Module — Servers (MVP)
- Server CRUD
- Tenant-scoped uniqueness
- Tagging system
- Filtering and search
- Pagination
- Sorting
- Status management (Active / Inactive)

This is the first infrastructure resource managed by Helios.

---

## 🧱 Architecture

Helios follows **Clean Architecture + Vertical Slice design**.
