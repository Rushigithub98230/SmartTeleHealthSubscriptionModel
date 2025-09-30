# SmartTelehealth Billing System Test Suite

## Overview

This comprehensive test suite validates the SmartTelehealth billing system's reliability, performance, and production readiness. The tests cover all aspects of the billing lifecycle from plan creation to payment processing.

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- PowerShell (for running test scripts)
- Visual Studio 2022 or VS Code (optional)

### Running Tests

#### Option 1: PowerShell Script (Recommended)
```powershell
# Navigate to the test project directory
cd backend/SmartTelehealth.Tests

# Run all billing tests
.\run-comprehensive-billing-tests.ps1

# Run specific test categories
.\run-comprehensive-billing-tests.ps1 -Performance
.\run-comprehensive-billing-tests.ps1 -Integration
.\run-comprehensive-billing-tests.ps1 -Unit

# Run with verbose output
.\run-comprehensive-billing-tests.ps1 -Verbose
```

#### Option 2: dotnet CLI
```bash
# Run all billing tests
dotnet test --filter "FullyQualifiedName~Billing"

# Run performance tests
dotnet test --filter "FullyQualifiedName~PerformanceBillingTests"

# Run integration tests
dotnet test --filter "FullyQualifiedName~Integration"

# Run unit tests
dotnet test --filter "FullyQualifiedName~Unit"
```

## Test Structure

### Test Categories

| Category | Purpose | Execution Time | Dependencies |
|----------|---------|----------------|--------------|
| **Unit Tests** | Test individual components | < 1 second | Mocked dependencies |
| **Integration Tests** | Test component interactions | 1-5 seconds | In-memory database |
| **Performance Tests** | Validate system performance | 5-30 seconds | Performance monitoring |
| **End-to-End Tests** | Test complete workflows | 10-60 seconds | Full test environment |

### Test Files

| File | Purpose | Test Count |
|------|---------|------------|
| `BillingTestBase.cs` | Base class for integration tests | - |
| `TestDataBuilder.cs` | Test data creation utilities | - |
| `ComprehensiveBillingTests.cs` | End-to-end billing tests | 8 |
| `PerformanceBillingTests.cs` | Performance and load tests | 8 |
| `PrivilegeBasedBillingServiceTests.cs` | Unit tests for services | 4 |

## Test Scenarios

### 1. Plan Price Calculation
- ✅ Calculate base price with different commission rates
- ✅ Handle plans with many privileges (performance)
- ✅ Error handling for invalid plans

### 2. Privilege Usage Tracking
- ✅ Process usage within limits
- ✅ Process usage exceeding limits (overage)
- ✅ Handle unlimited privileges
- ✅ Concurrent usage by multiple users

### 3. Subscription Renewal
- ✅ Renew subscription with usage reset
- ✅ Handle pending overage charges
- ✅ Update next billing date

### 4. Billing Record Management
- ✅ Create records with different statuses
- ✅ Process payments and refunds
- ✅ Bulk operations
- ✅ History retrieval

### 5. Performance Validation
- ✅ Response time thresholds
- ✅ Concurrent operation handling
- ✅ Memory usage monitoring
- ✅ Transaction performance

## Test Data

### Standard Test Plans

| Plan | Price | Billing Cycle | Privileges |
|------|-------|---------------|------------|
| **Basic Health Plan** | $29.99 | Monthly | 2 privileges |
| **Premium Health Plan** | $99.99 | Monthly | 3 privileges |
| **Enterprise Health Plan** | $299.99 | Yearly | 3 unlimited privileges |

### Test Users
- **Admin User**: Full access for plan management
- **Regular Users**: 5-1000 users for testing scenarios
- **Test Scenarios**: Basic (5), Medium (50), Large (200), Stress (1000)

## Performance Thresholds

| Operation | Threshold | Measurement |
|-----------|-----------|-------------|
| Plan Price Calculation | < 1000ms | Response time |
| Privilege Usage Processing | < 500ms | Response time |
| Subscription Renewal | < 2000ms | Response time |
| Bulk Operations | < 5000ms | Response time |
| Concurrent Operations | < 10000ms | Response time |
| Memory Usage | < 500MB | Peak memory |

## Configuration

### Test Configuration (`test-config.json`)
```json
{
  "TestConfiguration": {
    "Database": {
      "Provider": "InMemory",
      "EnableSensitiveDataLogging": false
    },
    "Performance": {
      "MaxExecutionTimeMs": {
        "PlanPriceCalculation": 1000,
        "PrivilegeUsageProcessing": 500
      }
    }
  }
}
```

### Test Scenarios
- **Basic**: 5 users, 3 plans, 6 privileges
- **Medium**: 50 users, 5 plans, 20 privileges
- **Large**: 200 users, 10 plans, 50 privileges
- **Stress**: 1000 users, 20 plans, 100 privileges

## Expected Results

### Success Rates
- **Unit Tests**: 100%
- **Integration Tests**: 100%
- **Performance Tests**: 95%
- **End-to-End Tests**: 100%

### Performance Metrics
- **Response Time**: < 1000ms for most operations
- **Throughput**: > 10 operations/second
- **Memory Usage**: < 500MB peak
- **Concurrent Users**: Support 100+ concurrent operations

## Troubleshooting

### Common Issues

#### 1. Test Database Errors
```bash
# Solution: Ensure in-memory database is configured
# Check: Verify UseInMemoryDatabase in test setup
```

#### 2. Mock Service Failures
```bash
# Solution: Verify mock setup in test base class
# Check: Ensure IStripeService is properly mocked
```

#### 3. Performance Test Failures
```bash
# Solution: Check system resources and test data size
# Check: Verify performance thresholds in configuration
```

### Debugging

#### Enable Detailed Logging
```json
{
  "Logging": {
    "Level": "Debug",
    "EnablePerformanceLogging": true
  }
}
```

#### Run Single Test
```bash
dotnet test --filter "FullyQualifiedName~TestName" --logger "console;verbosity=detailed"
```

## CI/CD Integration

### GitHub Actions Example
```yaml
name: Billing System Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '8.0.x'
    - name: Test
      run: dotnet test --verbosity normal
```

## Documentation

- **[Comprehensive Test Documentation](BILLING_SYSTEM_TEST_DOCUMENTATION.md)**: Detailed test documentation
- **[Test Configuration](test-config.json)**: Test configuration settings
- **[Test Runner Script](run-comprehensive-billing-tests.ps1)**: PowerShell test runner

## Support

For questions or issues:
1. Check the troubleshooting section
2. Review the comprehensive documentation
3. Contact the development team

## Test Results

After running tests, results are available in multiple formats:
- **Console**: Real-time output
- **TRX**: XML format for Visual Studio
- **HTML**: Human-readable report
- **JSON**: Machine-readable format

---

**Note**: This test suite is designed to ensure the billing system is production-ready and can handle real-world scenarios with confidence.
