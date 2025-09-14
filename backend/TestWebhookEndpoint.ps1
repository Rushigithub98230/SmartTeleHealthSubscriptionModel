# Stripe Webhook Test Script
# This script tests the webhook endpoint with various Stripe events

param(
    [string]$WebhookUrl = "https://pwlkgvc0-61376.inc1.devtunnels.ms/api/StripeWebhook/webhook",
    [string]$TestMode = "basic"
)

Write-Host "Testing Stripe Webhook Endpoint: $WebhookUrl" -ForegroundColor Green

# Test webhook endpoint accessibility
function Test-WebhookEndpoint {
    param([string]$Url)
    
    try {
        $response = Invoke-WebRequest -Uri $Url -Method POST -ContentType "application/json" -Body '{"test": "connectivity"}' -TimeoutSec 10
        Write-Host "✓ Webhook endpoint is accessible (Status: $($response.StatusCode))" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "✗ Webhook endpoint is not accessible: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Test webhook signature validation
function Test-WebhookSignature {
    param([string]$Url)
    
    $testPayload = @{
        id = "evt_test_webhook"
        object = "event"
        type = "test.webhook"
        created = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
        data = @{
            object = @{
                id = "test_object"
                object = "test"
            }
        }
    } | ConvertTo-Json -Depth 3
    
    try {
        # This will fail signature validation, which is expected
        $response = Invoke-WebRequest -Uri $Url -Method POST -ContentType "application/json" -Body $testPayload -TimeoutSec 10
        Write-Host "⚠ Unexpected success - signature validation may not be working" -ForegroundColor Yellow
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 400) {
            Write-Host "✓ Webhook signature validation is working (Expected 400 error)" -ForegroundColor Green
        } else {
            Write-Host "✗ Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

# Test specific webhook events
function Test-WebhookEvents {
    param([string]$Url)
    
    $events = @(
        @{
            Name = "Customer Subscription Created"
            Type = "customer.subscription.created"
            Data = @{
                id = "sub_test_123"
                object = "subscription"
                status = "active"
                customer = "cus_test_123"
                current_period_start = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
                current_period_end = [DateTimeOffset]::UtcNow.AddDays(30).ToUnixTimeSeconds()
            }
        },
        @{
            Name = "Invoice Payment Succeeded"
            Type = "invoice.payment_succeeded"
            Data = @{
                id = "in_test_123"
                object = "invoice"
                amount_paid = 2000
                amount_due = 0
                currency = "usd"
                customer = "cus_test_123"
                subscription = "sub_test_123"
                number = "INV-123"
            }
        },
        @{
            Name = "Invoice Payment Failed"
            Type = "invoice.payment_failed"
            Data = @{
                id = "in_test_456"
                object = "invoice"
                amount_due = 2000
                currency = "usd"
                customer = "cus_test_123"
                subscription = "sub_test_123"
                number = "INV-456"
            }
        }
    )
    
    foreach ($event in $events) {
        Write-Host "`nTesting: $($event.Name)" -ForegroundColor Cyan
        
        $payload = @{
            id = "evt_test_$($event.Type.Replace('.', '_'))"
            object = "event"
            type = $event.Type
            created = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
            data = @{
                object = $event.Data
            }
        } | ConvertTo-Json -Depth 4
        
        try {
            # Note: These will fail signature validation, but we can test endpoint structure
            $response = Invoke-WebRequest -Uri $Url -Method POST -ContentType "application/json" -Body $payload -TimeoutSec 10
            Write-Host "  ✓ Event processed (Status: $($response.StatusCode))" -ForegroundColor Green
        }
        catch {
            if ($_.Exception.Response.StatusCode -eq 400) {
                Write-Host "  ✓ Event rejected due to signature validation (Expected)" -ForegroundColor Green
            } else {
                Write-Host "  ✗ Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }
}

# Main execution
Write-Host "`n=== Stripe Webhook Endpoint Test ===" -ForegroundColor Yellow

# Test 1: Basic connectivity
Write-Host "`n1. Testing endpoint accessibility..." -ForegroundColor Cyan
$isAccessible = Test-WebhookEndpoint -Url $WebhookUrl

if (-not $isAccessible) {
    Write-Host "`n❌ Webhook endpoint is not accessible. Please check:" -ForegroundColor Red
    Write-Host "   - The webhook URL is correct" -ForegroundColor Red
    Write-Host "   - The development tunnel is running" -ForegroundColor Red
    Write-Host "   - The application is started" -ForegroundColor Red
    exit 1
}

# Test 2: Signature validation
Write-Host "`n2. Testing signature validation..." -ForegroundColor Cyan
Test-WebhookSignature -Url $WebhookUrl

# Test 3: Event processing (if basic mode)
if ($TestMode -eq "basic") {
    Write-Host "`n3. Testing event processing..." -ForegroundColor Cyan
    Test-WebhookEvents -Url $WebhookUrl
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Yellow
Write-Host "`nNext Steps:" -ForegroundColor Green
Write-Host "1. Configure webhook in Stripe Dashboard with the correct URL" -ForegroundColor White
Write-Host "2. Set the webhook secret in appsettings.json" -ForegroundColor White
Write-Host "3. Test with real Stripe events using Stripe CLI" -ForegroundColor White
Write-Host "4. Monitor application logs for webhook processing" -ForegroundColor White

Write-Host "`nFor detailed configuration, see: WEBHOOK_CONFIGURATION_GUIDE.md" -ForegroundColor Cyan

