# Subscription Management Test Suite - Execution Guide

## Overview

This comprehensive test suite validates the entire subscription management system, including core operations, Stripe integration, webhooks, billing, privileges, and admin operations. The tests are designed to ensure 100% production readiness and reliability.

## Test Suite Structure

### 1. ComprehensiveSubscriptionManagementTests.cs
**Core functionality testing with 30 test cases**

#### Test Categories:
- **Core Subscription Management** (Tests 1-4)
  - Create, read, update, delete operations
  - User subscription retrieval
  - Data validation

- **Subscription Lifecycle Management** (Tests 5-9)
  - Pause/Resume functionality
  - Cancellation and reactivation
  - Plan upgrades and migrations

- **Subscription Plan Management** (Tests 10-14)
  - Plan CRUD operations
  - Plan activation/deactivation
  - Plan updates and modifications

- **Billing and Payment Tests** (Tests 15-17)
  - Billing history retrieval
  - Payment processing
  - Payment method management

- **Privilege Management Tests** (Tests 18-19)
  - Privilege usage validation
  - Usage statistics

- **Admin Operations Tests** (Tests 20-21)
  - Bulk operations
  - Admin-only functionality

- **Error Handling and Validation** (Tests 22-24)
  - Invalid data handling
  - Authorization validation
  - Business rule enforcement

- **Data Consistency Tests** (Tests 25-26)
  - Status consistency
  - Plan consistency

- **Stripe Integration Tests** (Tests 27-28)
  - Service availability
  - Stripe ID validation

- **Analytics and Reporting** (Tests 29-30)
  - Analytics data retrieval
  - Export functionality

### 2. StripeWebhookIntegrationTests.cs
**Webhook integration testing with 20 test cases**

#### Test Categories:
- **Webhook Event Processing** (Tests 1-5)
  - Subscription lifecycle webhooks
  - Status change handling

- **Payment Webhooks** (Tests 6-8)
  - Payment success/failure
  - Action required scenarios

- **Customer Webhooks** (Tests 9-10)
  - Customer creation/updates

- **Payment Method Webhooks** (Tests 11-12)
  - Payment method attachment/detachment

- **Invoice Webhooks** (Tests 13-14)
  - Invoice finalization and sending

- **Webhook Security** (Tests 15-16)
  - Signature validation
  - Secret configuration

- **Error Handling and Retry** (Tests 17-18)
  - Retry logic
  - Comprehensive logging

- **Integration Tests** (Tests 19-20)
  - End-to-end webhook flow
  - Idempotency handling

### 3. AdvancedSubscriptionScenariosTests.cs
**Edge cases and complex scenarios with 17 test cases**

#### Test Categories:
- **Complex Subscription Workflows** (Tests 1-2)
  - Complete lifecycle management
  - Plan migration scenarios

- **Edge Cases and Business Logic** (Tests 3-5)
  - Duplicate prevention
  - Invalid transitions
  - Expiration handling

- **Billing Edge Cases** (Tests 6-8)
  - Failed payment handling
  - Payment retry logic
  - Prorated upgrades

- **Privilege Management Edge Cases** (Tests 9-10)
  - Usage limit enforcement
  - Expiration handling

- **Data Integrity** (Tests 11-12)
  - Status consistency
  - Plan consistency

- **Admin Operations Edge Cases** (Tests 13-14)
  - Bulk operations
  - Analytics performance

- **Performance and Scalability** (Tests 15-16)
  - Large dataset handling
  - Export performance

- **Integration and End-to-End** (Test 17)
  - Complete workflow validation

## Test Execution Instructions

### Prerequisites
1. **Database Setup**: Ensure in-memory database is configured for testing
2. **Service Dependencies**: All required services must be registered in DI container
3. **Test Data**: Test database will be automatically initialized with required entities

### Running Tests

#### Option 1: Run All Tests
```bash
# From the test project directory
dotnet test

# With verbose output
dotnet test --verbosity normal

# With specific logger
dotnet test --logger "console;verbosity=detailed"
```

#### Option 2: Run Specific Test Categories
```bash
# Run only core subscription management tests
dotnet test --filter "Category=Core Subscription Management"

# Run only webhook tests
dotnet test --filter "Category=Webhook Event Processing"

# Run only critical priority tests
dotnet test --filter "Priority=Critical"
```

#### Option 3: Run Individual Test Classes
```bash
# Run comprehensive tests only
dotnet test --filter "FullyQualifiedName~ComprehensiveSubscriptionManagementTests"

# Run webhook tests only
dotnet test --filter "FullyQualifiedName~StripeWebhookIntegrationTests"

# Run advanced scenarios only
dotnet test --filter "FullyQualifiedName~AdvancedSubscriptionScenariosTests"
```

#### Option 4: Run Specific Test Methods
```bash
# Run a specific test method
dotnet test --filter "FullyQualifiedName~Test_01_CreateSubscription_WithValidData_ShouldSucceed"

# Run tests by name pattern
dotnet test --filter "Name~CreateSubscription"
```

### Test Execution Order

The tests are designed to be run in sequence within each class, but can also be run independently. The test numbering system indicates the logical flow:

1. **Setup Phase**: Database initialization and test data creation
2. **Core Functionality**: Basic CRUD operations and validation
3. **Lifecycle Management**: Status transitions and business logic
4. **Integration Testing**: Stripe integration and webhook handling
5. **Edge Cases**: Error scenarios and boundary conditions
6. **Performance Testing**: Scalability and performance validation
7. **Cleanup Phase**: Test data removal and resource cleanup

## Test Data Management

### Automatic Setup
- Each test class automatically creates its own test database
- Test data includes users, plans, subscriptions, and privileges
- Database is isolated using unique names to prevent conflicts

### Test Data Entities
- **Users**: Test user and admin user with different roles
- **Subscription Plans**: Basic, Premium, and Enterprise plans
- **Billing Cycles**: Monthly and annual billing options
- **Currencies**: USD currency configuration
- **Privileges**: Consultation, medication, and home visit privileges
- **Subscriptions**: Active subscriptions with Stripe integration

### Data Cleanup
- Tests automatically clean up after execution
- In-memory database is deleted after each test class
- Resources are properly disposed

## Expected Test Results

### Success Criteria
- **All Critical Tests**: Must pass (100% success rate)
- **High Priority Tests**: Should pass (95%+ success rate)
- **Medium Priority Tests**: Expected to pass (90%+ success rate)

### Common Failure Scenarios
1. **Service Dependencies**: Missing service registrations
2. **Database Issues**: In-memory database configuration problems
3. **Stripe Integration**: Missing Stripe service mocks
4. **Authorization**: Token validation issues
5. **Business Logic**: Validation rule mismatches

### Troubleshooting

#### Test Failures
1. **Check Service Registration**: Ensure all services are properly registered
2. **Verify Database Setup**: Confirm in-memory database is working
3. **Check Dependencies**: Verify all required packages are installed
4. **Review Logs**: Check test output for detailed error messages

#### Performance Issues
1. **Database Size**: Large test datasets may slow execution
2. **Service Initialization**: Heavy services may impact startup time
3. **Cleanup Operations**: Ensure proper resource disposal

## Continuous Integration

### CI/CD Integration
- Tests can be integrated into CI/CD pipelines
- Use `dotnet test --logger trx` for XML output
- Configure test results reporting in your CI system

### Test Reporting
- Generate HTML reports using tools like ReportGenerator
- Track test coverage using coverlet.collector
- Monitor test execution time and success rates

## Production Readiness Validation

### What These Tests Validate
1. **Functionality**: All subscription management features work correctly
2. **Reliability**: System handles errors and edge cases gracefully
3. **Performance**: System performs well under various load conditions
4. **Security**: Authorization and validation work properly
5. **Integration**: Stripe integration and webhook handling work correctly
6. **Data Integrity**: Business rules and data consistency are maintained

### Success Metrics
- **Test Coverage**: Aim for 90%+ code coverage
- **Test Execution Time**: Complete suite should run in under 5 minutes
- **Success Rate**: 95%+ test pass rate
- **Performance**: No tests should take longer than 30 seconds individually

## Maintenance and Updates

### Adding New Tests
1. Follow the existing naming convention
2. Use appropriate traits for categorization
3. Ensure proper cleanup in Dispose method
4. Add comprehensive assertions

### Updating Existing Tests
1. Maintain backward compatibility
2. Update test data as needed
3. Ensure business logic changes are reflected
4. Update documentation

### Test Data Updates
1. Modify InitializeTestDatabaseAsync method
2. Add new entities as needed
3. Update cleanup logic
4. Ensure data consistency

## Conclusion

This comprehensive test suite provides thorough validation of the subscription management system. By running these tests regularly, you can ensure:

- **Production Readiness**: All functionality is working correctly
- **Reliability**: System handles edge cases and errors gracefully
- **Performance**: System meets performance requirements
- **Security**: Authorization and validation are working properly
- **Integration**: External services are properly integrated

Regular execution of this test suite will help maintain system quality and catch issues before they reach production.
