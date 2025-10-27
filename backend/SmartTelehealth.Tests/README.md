# SmartTelehealth Integration Test Suite

## Overview

This test suite provides comprehensive integration testing for the SmartTelehealth subscription management system. It uses **real implementations** of all internal services with **mocked third-party services** and a **real SQL Server database** to ensure accurate testing of business logic.

## Architecture

### Test Infrastructure

- **Real Services**: All internal services (SubscriptionService, BillingService, etc.) use real implementations
- **Mocked External Services**: Stripe, Email, SMS, and Push Notification services are mocked
- **Real Database**: Uses SQL Server LocalDB with migrations and proper seed data
- **Comprehensive Coverage**: Tests all business logic, pricing calculations, and data persistence

### Key Components

1. **TestBase**: Base class providing real service implementations and mocked external services
2. **TestDatabaseSetup**: Handles database setup, migrations, and seed data
3. **TestDataBuilder**: Fluent API for creating test entities with realistic data
4. **MasterData**: Container for seeded master data (billing cycles, currencies, privileges, etc.)

## Updated Pricing Architecture

The test suite has been updated to reflect the new pricing architecture:

### Key Changes

- **AdminCommissionPercent Only**: All admin commissions are now percentage-based
- **BasePrice**: Primary pricing field for subscription plans
- **Auto-Calculated Pricing**: Plans can auto-calculate prices from privilege costs + commission
- **DiscountPercentage**: Promotional discounts applied as percentages
- **BillingDiscountPercentage**: Additional billing cycle discounts

### Pricing Formula

```
Final Price = BasePrice - (BasePrice × DiscountPercentage) - (BasePrice × BillingDiscountPercentage)
```

For auto-calculated plans:
```
BasePrice = Σ(Privilege Costs) + (Σ(Privilege Costs) × AdminCommissionPercent)
```

## Test Categories

### 1. Subscription Plan Service Tests (`SubscriptionPlanServiceTests`)

- **Plan Creation**: Manual and auto-calculated pricing
- **Pricing Validation**: Commission percentages, discount validation
- **Plan Management**: CRUD operations, soft deletes
- **Integration**: Complete plan lifecycle testing

### 2. Billing Service Tests (`BillingServiceTests`)

- **Billing Records**: Creation, updates, status management
- **Billing Adjustments**: Discounts, credits, refunds
- **Renewal Processing**: Subscription renewals with proper pricing
- **Edge Cases**: Negative amounts, excessive discounts

### 3. Subscription Service Tests (`SubscriptionServiceTests`)

- **Subscription Lifecycle**: Creation, activation, cancellation
- **Privilege Management**: Usage tracking, limits, resets
- **Status Transitions**: Active, paused, cancelled states
- **Integration**: End-to-end subscription workflows

## Running Tests

### Prerequisites

1. **SQL Server LocalDB**: Must be installed and running
2. **.NET 8.0 SDK**: Required for building and running tests
3. **PowerShell**: For running test scripts (Windows) or PowerShell Core (cross-platform)

### Quick Start

```powershell
# Run all tests
.\run-integration-tests.ps1

# Run specific test type
.\run-integration-tests.ps1 -TestType "SubscriptionPlan"

# Run with verbose output
.\run-integration-tests.ps1 -Verbose

# Clean and rebuild before testing
.\run-integration-tests.ps1 -Clean -Build
```

### Manual Test Execution

```bash
# Build the solution
dotnet build backend/SmartTelehealth.sln --configuration Release

# Run all tests
dotnet test backend/SmartTelehealth.Tests --configuration Release --verbosity normal

# Run specific test class
dotnet test backend/SmartTelehealth.Tests --filter "ClassName=SubscriptionPlanServiceTests" --verbosity normal

# Run with code coverage
dotnet test backend/SmartTelehealth.Tests --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

## Test Data

### Master Data Seeding

The test suite automatically seeds the following master data:

- **Billing Cycles**: Monthly, Quarterly, Annual
- **Currencies**: USD, EUR, GBP
- **Privilege Types**: Video Call, Message, Prescription, Consultation
- **Categories**: Mental Health, Physical Health, General Health
- **Privileges**: Pre-configured with realistic costs and overage rates
- **System Settings**: Default commission percentages and currency settings

### Test Data Builder

The `TestDataBuilder` provides fluent APIs for creating test entities:

```csharp
// Create a user
var user = await TestData.User()
    .WithEmail("test@example.com")
    .WithFirstName("John")
    .WithLastName("Doe")
    .BuildAsync();

// Create a subscription plan
var plan = await TestData.SubscriptionPlan()
    .WithName("Test Plan")
    .WithBasePrice(99.99m)
    .WithAutoCalculatedPrice(false)
    .WithAdminCommissionPercent(10.0m)
    .WithBillingCycle("monthly")
    .WithCurrency("USD")
    .BuildAsync();

// Create a complete subscription setup
var (user, plan, subscription) = await TestData.CreateCompleteSubscriptionAsync(
    planName: "Premium Plan",
    basePrice: 149.99m,
    billingCycle: "monthly",
    isAutoCalculated: false
);
```

## Test Scenarios

### Pricing Scenarios

1. **Manual Pricing**: Plans with fixed base prices
2. **Auto-Calculated Pricing**: Plans that calculate prices from privilege costs
3. **Commission Calculations**: Various commission percentages
4. **Discount Applications**: Promotional and billing discounts
5. **Edge Cases**: Zero prices, excessive discounts, negative amounts

### Business Logic Scenarios

1. **Subscription Lifecycle**: Complete user journey from signup to cancellation
2. **Billing Cycles**: Monthly, quarterly, and annual billing
3. **Privilege Management**: Usage tracking, limits, and resets
4. **Payment Processing**: Successful and failed payments
5. **Renewal Processing**: Automatic and manual renewals

### Integration Scenarios

1. **End-to-End Workflows**: Complete business processes
2. **Data Consistency**: Cross-service data integrity
3. **Transaction Management**: Rollback scenarios
4. **Error Handling**: Graceful failure handling
5. **Performance**: Large dataset handling

## Mocked Services

### Stripe Service

- **Customer Creation**: Mocked customer creation with test IDs
- **Product Management**: Mocked product and price creation
- **Subscription Management**: Mocked subscription lifecycle
- **Payment Processing**: Mocked payment success/failure scenarios

### Notification Services

- **Email Service**: Mocked email sending
- **SMS Service**: Mocked SMS sending
- **Push Notifications**: Mocked push notification delivery

## Database Management

### Test Database

- **Database Name**: `SmartTelehealth_Test`
- **Connection**: SQL Server LocalDB
- **Migrations**: Automatically applied on test setup
- **Data Isolation**: Each test run starts with clean data

### Data Cleanup

- **Automatic Cleanup**: Test data is cleared between test runs
- **Master Data**: Re-seeded for each test run
- **Transaction Rollback**: Failed tests don't affect database state

## Best Practices

### Writing Tests

1. **Use TestDataBuilder**: Always use the fluent API for creating test data
2. **Test Real Scenarios**: Focus on realistic business scenarios
3. **Verify Side Effects**: Check database state, service calls, and notifications
4. **Clean Assertions**: Use FluentAssertions for readable test assertions
5. **Proper Setup**: Use the TestBase class for consistent test environment

### Test Organization

1. **Group Related Tests**: Use test classes for related functionality
2. **Descriptive Names**: Use clear, descriptive test method names
3. **Arrange-Act-Assert**: Follow the AAA pattern for test structure
4. **Single Responsibility**: Each test should verify one specific behavior
5. **Independent Tests**: Tests should not depend on each other

## Troubleshooting

### Common Issues

1. **Database Connection**: Ensure SQL Server LocalDB is running
2. **Migration Errors**: Check that all migrations are applied
3. **Test Data Issues**: Verify master data seeding is working
4. **Service Registration**: Ensure all services are properly registered
5. **Mock Configuration**: Check that external services are properly mocked

### Debugging

1. **Verbose Output**: Use `-Verbose` flag for detailed test output
2. **Test Results**: Check TestResults directory for detailed reports
3. **Logging**: Enable logging in test configuration for debugging
4. **Database State**: Inspect test database for data issues
5. **Service Calls**: Verify mocked service calls are as expected

## Contributing

### Adding New Tests

1. **Follow Naming Conventions**: Use descriptive test method names
2. **Use TestBase**: Inherit from TestBase for consistent setup
3. **Mock External Services**: Always mock third-party services
4. **Test Edge Cases**: Include boundary conditions and error scenarios
5. **Document Complex Tests**: Add comments for complex test logic

### Updating Test Data

1. **Update MasterData**: Add new master data to TestDatabaseSetup
2. **Update TestDataBuilder**: Add new builder methods as needed
3. **Maintain Realism**: Keep test data realistic and representative
4. **Version Compatibility**: Ensure test data works with current schema
5. **Performance**: Consider test data size for performance

## Performance Considerations

### Test Execution

- **Parallel Execution**: Tests can run in parallel for faster execution
- **Database Optimization**: Use efficient queries and proper indexing
- **Memory Management**: Dispose of resources properly
- **Test Isolation**: Ensure tests don't interfere with each other
- **Cleanup**: Proper cleanup prevents memory leaks

### Database Performance

- **Connection Pooling**: Efficient database connection management
- **Transaction Scope**: Minimize transaction scope for better performance
- **Index Usage**: Ensure proper database indexing
- **Query Optimization**: Use efficient queries for test data
- **Bulk Operations**: Use bulk operations for large datasets

## Conclusion

This integration test suite provides comprehensive coverage of the SmartTelehealth subscription management system. It ensures that all business logic, pricing calculations, and data persistence work correctly in a realistic environment while maintaining fast execution and reliable results.

The updated architecture reflects the new pricing model with percentage-based commissions and provides a solid foundation for testing all aspects of the subscription management system.