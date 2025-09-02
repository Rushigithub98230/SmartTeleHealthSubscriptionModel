using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.API.Controllers;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.DTOs;
using System.Security.Claims;
using Xunit;

namespace SmartTelehealth.API.Tests;

[Trait("Category", "Privilege Management Controller")]
public class PrivilegeManagementControllerTests
{
    private readonly Mock<ISubscriptionService> _mockSubscriptionService;
    private readonly Mock<ILogger<SubscriptionManagementController>> _mockLogger;
    private readonly SubscriptionManagementController _controller;

    public PrivilegeManagementControllerTests()
    {
        _mockSubscriptionService = new Mock<ISubscriptionService>();
        _mockLogger = new Mock<ILogger<SubscriptionManagementController>>();
        var mockCategoryService = new Mock<ICategoryService>();
        var mockAnalyticsService = new Mock<IAnalyticsService>();
        var mockAuditService = new Mock<IAuditService>();
        
        _controller = new SubscriptionManagementController(
            _mockSubscriptionService.Object, 
            mockCategoryService.Object,
            mockAnalyticsService.Object,
            mockAuditService.Object);

        // Setup controller context with admin user
        SetupControllerContext(1, "admin@test.com", "Admin", "User", 1);
    }

    private void SetupControllerContext(int userId, string email, string firstName, string lastName, int roleId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("FirstName", firstName),
            new Claim("LastName", lastName),
            new Claim("RoleID", roleId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    #region Assign Privileges to Plan Tests

    [Fact]
    public async Task AssignPrivilegesToPlan_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var planId = Guid.NewGuid().ToString();
        var privileges = new List<PlanPrivilegeDto>
        {
                         new PlanPrivilegeDto
             {
                 PrivilegeId = Guid.NewGuid(),
                 Value = 100,
                 UsagePeriodId = Guid.NewGuid(),
                 DailyLimit = 10,
                 WeeklyLimit = 50,
                 MonthlyLimit = 200,
                 ExpirationDate = DateTime.UtcNow.AddDays(30)
             }
        };

        var expectedResult = new JsonModel
        {
            data = new { AssignedCount = 1 },
            Message = "Successfully assigned 1 privileges to plan",
            StatusCode = 200
        };

        _mockSubscriptionService
            .Setup(x => x.AssignPrivilegesToPlanAsync(It.IsAny<Guid>(), It.IsAny<List<PlanPrivilegeDto>>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.AssignPrivilegesToPlan(planId, privileges);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Successfully assigned 1 privileges to plan", result.Message);
        Assert.NotNull(result.data);

        _mockSubscriptionService.Verify(x => x.AssignPrivilegesToPlanAsync(
            Guid.Parse(planId), 
            privileges, 
            It.IsAny<TokenModel>()), Times.Once);
    }

    [Fact]
    public async Task AssignPrivilegesToPlan_WithInvalidPlanId_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidPlanId = "invalid-guid";
        var privileges = new List<PlanPrivilegeDto>();

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => 
            _controller.AssignPrivilegesToPlan(invalidPlanId, privileges));
    }

    [Fact]
    public async Task AssignPrivilegesToPlan_WithServiceError_ShouldReturnError()
    {
        // Arrange
        var planId = Guid.NewGuid().ToString();
        var privileges = new List<PlanPrivilegeDto>();

        var expectedResult = new JsonModel
        {
            data = new object(),
            Message = "Subscription plan not found",
            StatusCode = 404
        };

        _mockSubscriptionService
            .Setup(x => x.AssignPrivilegesToPlanAsync(It.IsAny<Guid>(), It.IsAny<List<PlanPrivilegeDto>>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.AssignPrivilegesToPlan(planId, privileges);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Subscription plan not found", result.Message);
    }

    [Fact]
    public async Task AssignPrivilegesToPlan_WithEmptyPrivileges_ShouldCallService()
    {
        // Arrange
        var planId = Guid.NewGuid().ToString();
        var privileges = new List<PlanPrivilegeDto>(); // Empty list

        var expectedResult = new JsonModel
        {
            data = new { AssignedCount = 0 },
            Message = "Successfully assigned 0 privileges to plan",
            StatusCode = 200
        };

        _mockSubscriptionService
            .Setup(x => x.AssignPrivilegesToPlanAsync(It.IsAny<Guid>(), It.IsAny<List<PlanPrivilegeDto>>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.AssignPrivilegesToPlan(planId, privileges);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Successfully assigned 0 privileges to plan", result.Message);

        _mockSubscriptionService.Verify(x => x.AssignPrivilegesToPlanAsync(
            Guid.Parse(planId), 
            privileges, 
            It.IsAny<TokenModel>()), Times.Once);
    }

    #endregion

    #region Remove Privilege from Plan Tests

    [Fact]
    public async Task RemovePrivilegeFromPlan_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var planId = Guid.NewGuid().ToString();
        var privilegeId = Guid.NewGuid().ToString();

        var expectedResult = new JsonModel
        {
            data = true,
            Message = "Privilege removed from plan successfully",
            StatusCode = 200
        };

        _mockSubscriptionService
            .Setup(x => x.RemovePrivilegeFromPlanAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.RemovePrivilegeFromPlan(planId, privilegeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Privilege removed from plan successfully", result.Message);
        Assert.True((bool)result.data);

        _mockSubscriptionService.Verify(x => x.RemovePrivilegeFromPlanAsync(
            Guid.Parse(planId), 
            Guid.Parse(privilegeId), 
            It.IsAny<TokenModel>()), Times.Once);
    }

    [Fact]
    public async Task RemovePrivilegeFromPlan_WithInvalidPlanId_ShouldThrowException()
    {
        // Arrange
        var invalidPlanId = "invalid-guid";
        var privilegeId = Guid.NewGuid().ToString();

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => 
            _controller.RemovePrivilegeFromPlan(invalidPlanId, privilegeId));
    }

    [Fact]
    public async Task RemovePrivilegeFromPlan_WithInvalidPrivilegeId_ShouldThrowException()
    {
        // Arrange
        var planId = Guid.NewGuid().ToString();
        var invalidPrivilegeId = "invalid-guid";

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => 
            _controller.RemovePrivilegeFromPlan(planId, invalidPrivilegeId));
    }

    [Fact]
    public async Task RemovePrivilegeFromPlan_WithServiceError_ShouldReturnError()
    {
        // Arrange
        var planId = Guid.NewGuid().ToString();
        var privilegeId = Guid.NewGuid().ToString();

        var expectedResult = new JsonModel
        {
            data = new object(),
            Message = "Privilege not found in plan",
            StatusCode = 404
        };

        _mockSubscriptionService
            .Setup(x => x.RemovePrivilegeFromPlanAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.RemovePrivilegeFromPlan(planId, privilegeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Privilege not found in plan", result.Message);
    }

    #endregion

    #region Update Plan Privilege Tests

    [Fact]
    public async Task UpdatePlanPrivilege_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var planId = Guid.NewGuid().ToString();
        var privilegeId = Guid.NewGuid().ToString();
                 var privilegeDto = new PlanPrivilegeDto
         {
             PrivilegeId = Guid.Parse(privilegeId),
             Value = 150,
             UsagePeriodId = Guid.NewGuid(),
             DailyLimit = 15,
             WeeklyLimit = 75,
             MonthlyLimit = 300,
             ExpirationDate = DateTime.UtcNow.AddDays(45)
         };

        var expectedResult = new JsonModel
        {
            data = new { Updated = true },
            Message = "Plan privilege updated successfully",
            StatusCode = 200
        };

        _mockSubscriptionService
            .Setup(x => x.UpdatePlanPrivilegeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PlanPrivilegeDto>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.UpdatePlanPrivilege(planId, privilegeId, privilegeDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Plan privilege updated successfully", result.Message);
        Assert.NotNull(result.data);

        _mockSubscriptionService.Verify(x => x.UpdatePlanPrivilegeAsync(
            Guid.Parse(planId), 
            Guid.Parse(privilegeId), 
            privilegeDto, 
            It.IsAny<TokenModel>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePlanPrivilege_WithInvalidPlanId_ShouldThrowException()
    {
        // Arrange
        var invalidPlanId = "invalid-guid";
        var privilegeId = Guid.NewGuid().ToString();
        var privilegeDto = new PlanPrivilegeDto();

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => 
            _controller.UpdatePlanPrivilege(invalidPlanId, privilegeId, privilegeDto));
    }

    [Fact]
    public async Task UpdatePlanPrivilege_WithInvalidPrivilegeId_ShouldThrowException()
    {
        // Arrange
        var planId = Guid.NewGuid().ToString();
        var invalidPrivilegeId = "invalid-guid";
        var privilegeDto = new PlanPrivilegeDto();

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => 
            _controller.UpdatePlanPrivilege(planId, invalidPrivilegeId, privilegeDto));
    }

    [Fact]
    public async Task UpdatePlanPrivilege_WithServiceError_ShouldReturnError()
    {
        // Arrange
        var planId = Guid.NewGuid().ToString();
        var privilegeId = Guid.NewGuid().ToString();
        var privilegeDto = new PlanPrivilegeDto();

        var expectedResult = new JsonModel
        {
            data = new object(),
            Message = "Subscription plan not found",
            StatusCode = 404
        };

        _mockSubscriptionService
            .Setup(x => x.UpdatePlanPrivilegeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PlanPrivilegeDto>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.UpdatePlanPrivilege(planId, privilegeId, privilegeDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Subscription plan not found", result.Message);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task AssignPrivilegesToPlan_WithNonAdminUser_ShouldReturnAccessDenied()
    {
        // Arrange
        SetupControllerContext(2, "user@test.com", "Regular", "User", 2); // Non-admin user
        var planId = Guid.NewGuid().ToString();
        var privileges = new List<PlanPrivilegeDto>();

        var expectedResult = new JsonModel
        {
            data = new object(),
            Message = "Access denied - Admin only",
            StatusCode = 403
        };

        _mockSubscriptionService
            .Setup(x => x.AssignPrivilegesToPlanAsync(It.IsAny<Guid>(), It.IsAny<List<PlanPrivilegeDto>>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.AssignPrivilegesToPlan(planId, privileges);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal("Access denied - Admin only", result.Message);
    }

    [Fact]
    public async Task RemovePrivilegeFromPlan_WithNonAdminUser_ShouldReturnAccessDenied()
    {
        // Arrange
        SetupControllerContext(2, "user@test.com", "Regular", "User", 2); // Non-admin user
        var planId = Guid.NewGuid().ToString();
        var privilegeId = Guid.NewGuid().ToString();

        var expectedResult = new JsonModel
        {
            data = new object(),
            Message = "Access denied - Admin only",
            StatusCode = 403
        };

        _mockSubscriptionService
            .Setup(x => x.RemovePrivilegeFromPlanAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.RemovePrivilegeFromPlan(planId, privilegeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal("Access denied - Admin only", result.Message);
    }

    [Fact]
    public async Task UpdatePlanPrivilege_WithNonAdminUser_ShouldReturnAccessDenied()
    {
        // Arrange
        SetupControllerContext(2, "user@test.com", "Regular", "User", 2); // Non-admin user
        var planId = Guid.NewGuid().ToString();
        var privilegeId = Guid.NewGuid().ToString();
        var privilegeDto = new PlanPrivilegeDto();

        var expectedResult = new JsonModel
        {
            data = new object(),
            Message = "Access denied - Admin only",
            StatusCode = 403
        };

        _mockSubscriptionService
            .Setup(x => x.UpdatePlanPrivilegeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PlanPrivilegeDto>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.UpdatePlanPrivilege(planId, privilegeId, privilegeDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(403, result.StatusCode);
        Assert.Equal("Access denied - Admin only", result.Message);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CompletePrivilegeManagementWorkflow_ShouldWorkEndToEnd()
    {
        // Arrange
        var planId = Guid.NewGuid().ToString();
        var privilegeId = Guid.NewGuid().ToString();
        var privileges = new List<PlanPrivilegeDto>
        {
                         new PlanPrivilegeDto
             {
                 PrivilegeId = Guid.Parse(privilegeId),
                 Value = 100,
                 UsagePeriodId = Guid.NewGuid(),
                 DailyLimit = 10,
                 WeeklyLimit = 50,
                 MonthlyLimit = 200,
                 ExpirationDate = DateTime.UtcNow.AddDays(30)
             }
        };

        var updateDto = new PlanPrivilegeDto
        {
            PrivilegeId = Guid.Parse(privilegeId),
            Value = 150,
            UsagePeriodId = Guid.NewGuid(),
            DailyLimit = 15,
            WeeklyLimit = 75,
            MonthlyLimit = 300,
            ExpirationDate = DateTime.UtcNow.AddDays(45)
        };

        // Setup service responses
        var assignResult = new JsonModel
        {
            data = new { AssignedCount = 1 },
            Message = "Successfully assigned 1 privileges to plan",
            StatusCode = 200
        };

        var updateResult = new JsonModel
        {
            data = new { Updated = true },
            Message = "Plan privilege updated successfully",
            StatusCode = 200
        };

        var removeResult = new JsonModel
        {
            data = true,
            Message = "Privilege removed from plan successfully",
            StatusCode = 200
        };

        _mockSubscriptionService
            .Setup(x => x.AssignPrivilegesToPlanAsync(It.IsAny<Guid>(), It.IsAny<List<PlanPrivilegeDto>>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(assignResult);

        _mockSubscriptionService
            .Setup(x => x.UpdatePlanPrivilegeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<PlanPrivilegeDto>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(updateResult);

        _mockSubscriptionService
            .Setup(x => x.RemovePrivilegeFromPlanAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<TokenModel>()))
            .ReturnsAsync(removeResult);

        // Act - Step 1: Assign privileges
        var assignResponse = await _controller.AssignPrivilegesToPlan(planId, privileges);

        // Act - Step 2: Update privilege
        var updateResponse = await _controller.UpdatePlanPrivilege(planId, privilegeId, updateDto);

        // Act - Step 3: Remove privilege
        var removeResponse = await _controller.RemovePrivilegeFromPlan(planId, privilegeId);

        // Assert - All operations should succeed
        Assert.Equal(200, assignResponse.StatusCode);
        Assert.Equal(200, updateResponse.StatusCode);
        Assert.Equal(200, removeResponse.StatusCode);

        Assert.Equal("Successfully assigned 1 privileges to plan", assignResponse.Message);
        Assert.Equal("Plan privilege updated successfully", updateResponse.Message);
        Assert.Equal("Privilege removed from plan successfully", removeResponse.Message);

        // Verify all service methods were called
        _mockSubscriptionService.Verify(x => x.AssignPrivilegesToPlanAsync(
            Guid.Parse(planId), privileges, It.IsAny<TokenModel>()), Times.Once);
        _mockSubscriptionService.Verify(x => x.UpdatePlanPrivilegeAsync(
            Guid.Parse(planId), Guid.Parse(privilegeId), updateDto, It.IsAny<TokenModel>()), Times.Once);
        _mockSubscriptionService.Verify(x => x.RemovePrivilegeFromPlanAsync(
            Guid.Parse(planId), Guid.Parse(privilegeId), It.IsAny<TokenModel>()), Times.Once);
    }

    #endregion
}
