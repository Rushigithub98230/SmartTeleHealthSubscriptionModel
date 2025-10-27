-- Migration: Update Pricing Architecture
-- Date: 2024-01-XX
-- Description: Update SubscriptionPlans table to use new sequential pricing architecture

-- Step 1: Add new pricing fields
ALTER TABLE SubscriptionPlans ADD BasePrice decimal(18,2) NOT NULL DEFAULT 0;
ALTER TABLE SubscriptionPlans ADD DiscountPercentage decimal(5,2) NULL;
ALTER TABLE SubscriptionPlans ADD BillingDiscountPercentage decimal(5,2) NULL;

-- Step 2: Migrate existing data
-- For existing plans, set BasePrice = Price (assuming Price was already the final price)
UPDATE SubscriptionPlans SET BasePrice = Price;

-- Step 3: Remove old pricing fields
ALTER TABLE SubscriptionPlans DROP COLUMN Price;
ALTER TABLE SubscriptionPlans DROP COLUMN DiscountedPrice;
ALTER TABLE SubscriptionPlans DROP COLUMN BillingDiscount;

-- Step 4: Add constraints
ALTER TABLE SubscriptionPlans ADD CONSTRAINT CK_SubscriptionPlans_BasePrice CHECK (BasePrice >= 0);
ALTER TABLE SubscriptionPlans ADD CONSTRAINT CK_SubscriptionPlans_DiscountPercentage CHECK (DiscountPercentage IS NULL OR (DiscountPercentage >= 0 AND DiscountPercentage <= 100));
ALTER TABLE SubscriptionPlans ADD CONSTRAINT CK_SubscriptionPlans_BillingDiscountPercentage CHECK (BillingDiscountPercentage IS NULL OR (BillingDiscountPercentage >= 0 AND BillingDiscountPercentage <= 100));

-- Note: This migration assumes existing Price field contained the final price
-- If existing Price field contained base price, adjust the migration accordingly
