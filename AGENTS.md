# Smart TeleHealth Subscription Model - Agent Guidelines

## Build & Test Commands
- **Build solution**: `dotnet build backend/SmartTelehealth.sln`
- **Run API**: `dotnet run --project backend/SmartTelehealth.API`
- **Run all tests**: `dotnet test backend/SmartTelehealth.API.Tests`
- **Run specific test**: `dotnet test --filter "FullyQualifiedName~{TestClassName}"`
- **Run test category**: `dotnet test --filter "Category=Core Subscription Management"`
- **Run single test method**: `dotnet test --filter "Name~{MethodName}"`

## Architecture & Structure
- **Clean Architecture**: Core (entities) → Application (services/DTOs) → Infrastructure (data/external) → API (controllers)
- **Projects**: Core (domain), Application (business logic), Infrastructure (data/external services), API (web layer), API.Tests (xUnit tests)
- **Key Integrations**: Stripe payments, Twilio/SendGrid communication, OpenTok video, AWS S3/Azure Blob storage
- **Database**: Entity Framework Core with SQL Server, ASP.NET Core Identity
- **Real-time**: SignalR hubs for chat and video calls

## Code Style & Conventions
- **.NET 8** with nullable enabled, implicit usings enabled
- **Entities**: Inherit from `BaseEntity`, PascalCase singular names (User, Subscription)
- **DTOs**: Located in Application/DTOs/, suffixed with `Dto` (SubscriptionDto, CreateSubscriptionDto)
- **Controllers**: Inherit from `BaseController`, use `[ApiController]` and `[Route("api/[controller]")]`
- **Services**: Interface pattern `I{Service}Service` in Application/Interfaces/, implementation in Application/Services/
- **Authentication**: JWT with token model extraction via `GetToken(HttpContext)` in BaseController
- **Return Type**: All controller methods return `JsonModel` wrapper
- **Validation**: FluentValidation and Data Annotations, comprehensive model validation

## Cursor Rules
Follow the comprehensive analysis process from `.cursor/rules/my-custome-rule.mdc`:
1. **Deep Code Comprehension**: Read and understand entire codebase context before changes
2. **Task Context Analysis**: Understand requirements and business logic completely
3. **Real Context Verification**: Never assume variable names, function signatures, or data structures
