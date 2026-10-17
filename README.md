# Willovate Store API

Backend API for **Willovate-One**, a multi-business e-commerce platform.

The API provides the backend foundation for businesses, stores, templates, products, customers, carts, orders, payments, inventory, and future AI-powered features.

---

# 1. Project Overview

Willovate-One is being developed as a platform where multiple businesses can create and operate their own online stores.

The Store API provides the backend services required by the frontend, admin applications, AI services, and other platform components.

High-level flow:

```text
Willovate-One
      │
      ├── Store UI
      ├── Admin UI
      ├── AI Services
      │
      ↓
Willovate Store API
      │
      ├── Business / Store
      ├── Templates
      ├── Products
      ├── Categories
      ├── Inventory
      ├── Customers
      ├── Cart
      ├── Orders
      ├── Payments
      └── Analytics
      │
      ↓
 PostgreSQL
```

---

# 2. Technology Stack

* ASP.NET Core Web API
* .NET 9
* Entity Framework Core
* PostgreSQL
* OpenAPI / Swagger
* Docker
* GitHub Actions
* Integration Tests

---

# 3. Requirements

Install:

* .NET SDK 9.0.304 or compatible
* Docker
* PostgreSQL through Docker

---

# 4. Local Development

Start PostgreSQL:

```bash
docker compose up -d postgres
```

Restore tools:

```bash
dotnet tool restore
```

Restore dependencies:

```bash
dotnet restore
```

Run the API:

```bash
dotnet run --project src/Willovate.Store.Api
```

API:

```text
http://localhost:5191
```

Swagger:

```text
http://localhost:5191/swagger
```

---

# 5. Environment Configuration

The database connection should be configured using:

```text
ConnectionStrings__Store
```

Example:

```text
ConnectionStrings__Store=Host=localhost;Port=5432;Database=willovate_store;Username=postgres;Password=...
```

Do not commit passwords or other secrets.

Use `.env` or appropriate local configuration.

---

# 6. Architecture

The backend follows a layered architecture.

```text
HTTP Request
     ↓
API / Controllers
     ↓
Application
     ↓
Domain
     ↓
Infrastructure
     ↓
PostgreSQL / External Services
```

Responsibilities should remain separated.

### API Layer

Handles:

* HTTP requests
* Validation
* Authentication/authorization
* HTTP responses

### Application Layer

Handles:

* Use cases
* Application services
* Commands
* Queries
* Business workflows

### Domain Layer

Contains:

* Entities
* Value objects
* Domain rules
* Core business concepts

### Infrastructure Layer

Handles:

* EF Core
* PostgreSQL
* External APIs
* Payment providers
* Other infrastructure services

---

# 7. Core Platform Model

The main platform relationship is:

```text
Business
   │
   ├── Store Configuration
   ├── Template
   ├── Products
   ├── Categories
   ├── Inventory
   ├── Customers
   └── Orders
```

A business/store should own its ecommerce data.

For example:

```text
Business A
 ├── Products
 ├── Customers
 └── Orders

Business B
 ├── Products
 ├── Customers
 └── Orders
```

Data belonging to one business must not accidentally appear in another business.

---

# 8. Business / Store Foundation

The Business/Store module is the foundation for multi-business support.

A business may contain information such as:

```text
Id
Name
Slug
Description
Status
CreatedAt
UpdatedAt
```

Additional configuration can be added as the platform grows.

Example:

```text
Business
   ↓
Store
   ↓
Template
   ↓
Products
```

The exact domain relationship should follow the architecture already established in the project.

---

# 9. Multi-Tenant Data

Business-owned data should be scoped using the appropriate business/store context.

Example:

```text
BusinessId
```

Conceptually:

```text
Products
---------
Id
BusinessId
Name
Price
CategoryId
...
```

Orders:

```text
Orders
------
Id
BusinessId
CustomerId
Status
Total
...
```

The purpose is to ensure:

```text
Business A → Business A data

Business B → Business B data
```

Cross-business data access should not happen accidentally.

---

# 10. Templates

Templates are reusable storefront designs.

A template may contain:

```text
Id
Name
Slug
Category
Description
Preview
Status
```

Example:

```text
Sports
├── Velocity
├── Arena
├── ProGear
└── Sprint
```

Templates should be reusable by multiple businesses.

Conceptually:

```text
Template
    │
    ├── Business A
    ├── Business B
    └── Business C
```

Template data should not contain business-specific product data.

---

# 11. Business + Template

A business can select a template:

```text
Business
   ↓
Selected Template
   ↓
Store Configuration
```

Example:

```text
ABC Sports
     ↓
Velocity Template
     ↓
ABC Sports branding
     ↓
ABC Sports products
```

The template controls presentation.

The business controls the actual store data.

---

# 12. Store Customization

Future store configuration may include:

```text
Logo
Brand Name
Colors
Fonts
Navigation
Homepage Sections
Footer
Contact Information
Social Links
Store Settings
```

Conceptually:

```text
Business
   │
   ├── Template
   ├── Branding
   ├── Navigation
   ├── Homepage
   └── Settings
```

---

# 13. Products

The current API provides product catalog functionality.

### List products

```http
GET /api/products
```

Supported parameters:

```text
search
category
featured
page
pageSize
```

Example:

```text
GET /api/products?search=shoe&page=1&pageSize=20
```

### Categories

```http
GET /api/products/categories
```

### Product details

```http
GET /api/products/{slug}
```

---

# 14. Future Product Management

The product module will eventually support:

```text
Product
 ├── Name
 ├── Slug
 ├── Description
 ├── Price
 ├── Category
 ├── Images
 ├── Variants
 ├── SKU
 ├── Inventory
 ├── Status
 └── BusinessId
```

Variants may include:

```text
Size
Color
SKU
Price
Stock
```

---

# 15. Categories

Categories organize products.

Example:

```text
Sports
 ├── Shoes
 ├── Clothing
 ├── Accessories
 └── Equipment
```

Categories should be associated with the correct business/store context where applicable.

---

# 16. Inventory

Future inventory functionality will manage:

```text
Product
Variant
SKU
Stock
Reserved Stock
Inventory Status
```

Example:

```text
Product
   ↓
Variant
   ↓
SKU
   ↓
Inventory
```

Inventory logic belongs in the backend rather than the frontend.

---

# 17. Customers

The customer module will eventually support:

```text
Customer
 ├── Account
 ├── Profile
 ├── Addresses
 ├── Orders
 ├── Wishlist
 └── Authentication
```

Customer data must be scoped correctly to the business/store where required by the platform model.

---

# 18. Cart

The future backend cart module may support:

```text
Cart
 ├── Customer / Session
 ├── BusinessId
 ├── Items
 ├── Quantity
 └── Pricing
```

Example:

```text
Cart
 ↓
Cart Items
 ↓
Products / Variants
```

The backend should validate product availability and pricing during checkout.

---

# 19. Orders

The order module will manage:

```text
Order
 ├── Customer
 ├── Business
 ├── Items
 ├── Amount
 ├── Payment Status
 ├── Order Status
 ├── Shipping
 └── CreatedAt
```

Typical order flow:

```text
Cart
 ↓
Checkout
 ↓
Order Creation
 ↓
Payment
 ↓
Payment Verification
 ↓
Order Confirmation
 ↓
Fulfillment
```

---

# 20. Payments

Payment functionality should be implemented behind a clear service abstraction.

Conceptually:

```text
Checkout
   ↓
Payment Service
   ↓
Payment Provider
   ↓
Verification
   ↓
Order Update
```

The backend must verify payment status rather than trusting the frontend.

---

# 21. API Health

Health endpoint:

```http
GET /api/health
```

This can be used by:

* Local development
* Docker
* CI/CD
* Monitoring
* Deployment systems

---

# 22. Database

PostgreSQL is the primary database.

Entity Framework Core is used for database access and migrations.

Database changes should be handled through migrations.

Create a migration:

```bash
dotnet tool run dotnet-ef migrations add DescribeTheChange \
  --project src/Willovate.Store.Api \
  --startup-project src/Willovate.Store.Api \
  --output-dir Data/Migrations
```

Never edit an already deployed migration.

If a change is required, create a new migration.

---

# 23. Seed Data

Seed data should be repeatable and safe for development.

Seed data may include:

```text
Businesses
Templates
Categories
Products
```

Development seed data should not contain production secrets or real customer information.

---

# 24. API Contract

The API is consumed by multiple clients.

Potential consumers:

```text
Store UI
Admin UI
AI Services
Mobile Applications
Third-party integrations
```

Therefore API responses should remain predictable and documented.

Avoid unnecessary breaking changes.

When an API contract changes:

```text
Backend Developer
      ↓
Update API Contract
      ↓
Notify Frontend Developer
      ↓
Update Frontend
      ↓
Test Integration
```

---

# 25. AI Integration

AI functionality will be added to Willovate-One in future phases.

Examples:

```text
AI Store Generator
AI Product Description
AI Content Generation
AI Customer Assistant
AI Analytics
AI Automation
AI Agents
```

AI services should use application-level business logic.

Preferred architecture:

```text
User
 ↓
AI Agent
 ↓
Intent
 ↓
Application Service / Tool
 ↓
Business Logic
 ↓
Database
 ↓
Result
 ↓
AI Response
```

AI should not directly modify the database.

---

# 26. Security Principles

Backend development must consider:

* Authentication
* Authorization
* Business/store isolation
* Input validation
* Secure secrets
* SQL injection protection
* Rate limiting where required
* Payment verification
* Logging
* Error handling

Never trust values supplied by the frontend for sensitive operations.

---

# 27. Testing

Build:

```bash
dotnet build
```

Run tests:

```bash
dotnet test
```

Check formatting:

```bash
dotnet format --verify-no-changes
```

Before opening a PR, all relevant checks should pass.

---

# 28. Integration Tests

The project includes self-contained integration tests.

Tests should cover important API behavior such as:

```text
Health
Products
Categories
Product details
Business isolation
Validation
Future checkout/order flows
```

New business-critical features should include appropriate tests.

---

# 29. Docker

Start PostgreSQL:

```bash
docker compose up -d postgres
```

Build and run the complete backend environment:

```bash
docker compose up --build
```

---

# 30. Git Workflow

Use feature branches.

Example:

```text
main
 │
 ├── feature/business-foundation
 ├── feature/template-api
 ├── feature/product-management
 ├── feature/customer-api
 └── feature/order-api
```

Example:

```bash
git checkout main
git pull

git checkout -b feature/business-foundation
```

After implementation:

```bash
git add .
git commit -m "Add business store foundation"
git push -u origin feature/business-foundation
```

Create a Pull Request after validation.

---

# 31. Backend Development Rules

### Do

* Follow the existing architecture
* Reuse existing domain models where appropriate
* Keep business logic in the correct layer
* Use migrations for database changes
* Add tests for important functionality
* Keep APIs documented
* Maintain business/store isolation
* Communicate API contract changes
* Keep changes focused

### Don't

* Rebuild the project unnecessarily
* Remove existing Product, Template, or Onboarding functionality
* Directly modify deployed migrations
* Put database logic inside controllers
* Trust frontend values for sensitive operations
* Hard-code secrets
* Create duplicate business logic
* Break existing APIs without coordination

---

# 32. Development Principle

The backend should be developed incrementally.

Do not attempt to build the complete ecommerce platform in one change.

Recommended order:

```text
Phase 1
Business / Store Foundation
        ↓
Phase 2
Templates
        ↓
Phase 3
Store Customization
        ↓
Phase 4
Products
        ↓
Phase 5
Customers
        ↓
Phase 6
Cart & Checkout
        ↓
Phase 7
Orders
        ↓
Phase 8
Payments
        ↓
Phase 9
Admin & Analytics
        ↓
Phase 10
AI
        ↓
Phase 11
Production
```

---

# 33. Current API Status

The current backend already provides the foundation for the product catalog:

```text
Health
Products
Categories
Product Details
PostgreSQL
EF Core
Swagger
Docker
Integration Tests
CI
```

The next major architectural step is supporting:

```text
Business / Store
        ↓
Template
        ↓
Business-specific Products
        ↓
Store Configuration
```

Existing functionality should continue working while these modules are added.

---

# 34. Frontend Integration

The Store UI normally runs at:

```text
http://localhost:5173
```

The API runs at:

```text
http://localhost:5191
```

Integration:

```text
React Store UI
      │
      │ HTTP/REST
      ↓
ASP.NET Core API
      │
      ↓
PostgreSQL
```

Frontend API configuration:

```env
VITE_API_URL=http://localhost:5191
```

---

# 35. Team Responsibilities

### Backend Team

Owns:

```text
API
Database
Domain Models
Business Logic
Authentication
Products
Templates API
Orders
Payments
Inventory
Backend AI Integration
Testing
```

### Frontend Team

Owns:

```text
Pages
Components
Templates
Responsive UI
Customer Experience
Client State
API Integration
```

### Shared Responsibility

Both teams must coordinate on:

```text
API Contracts
Data Models
Business / Store Architecture
Template Architecture
Authentication
Checkout
Error Handling
Integration Testing
```

---

# 36. Definition of Done

A backend feature is considered complete when:

```text
[ ] Requirement is implemented
[ ] Existing functionality still works
[ ] Domain/application architecture is respected
[ ] Database changes use migrations
[ ] API contract is documented
[ ] Tests are added/updated
[ ] dotnet build passes
[ ] dotnet test passes
[ ] dotnet format passes
[ ] No secrets committed
[ ] Frontend integration requirements communicated
[ ] PR is ready for review
```

---

# 37. Related Documentation

Root project documentation:

```text
Willovate-One/
├── README.md
├── docs/
│   ├── ARCHITECTURE.md
│   ├── API-CONTRACT.md
│   ├── PROJECT-ROADMAP.md
│   └── TEAM-WORKFLOW.md
```

Frontend documentation:

```text
willovate-store-ui/README.md
```

---

# 38. Final Platform Vision

Willovate-One is intended to become a complete multi-business ecommerce platform.

The long-term backend architecture is:

```text
                    Willovate-One
                          │
                    Store API
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
     Business          Template          Products
        │                 │                 │
        └─────────────────┼─────────────────┘
                          │
                    Store Config
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
    Customers           Cart             Orders
        │                 │                 │
        └─────────────────┼─────────────────┘
                          │
                     Payments
                          │
                     Analytics
                          │
                         AI
                          │
                    PostgreSQL
```

The goal is to build a **reusable platform**, not separate ecommerce systems for every business.

> **Build the platform first, then build features on top of the platform.**
