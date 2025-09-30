-- =====================================================
-- COMPREHENSIVE SUBSCRIPTION MANAGEMENT TABLES - PART 3
-- =====================================================
-- This script adds all foreign keys, constraints, and indexes
-- =====================================================

-- =====================================================
-- 10. FOREIGN KEY CONSTRAINTS FOR ALL TABLES
-- =====================================================

-- Subscriptions foreign keys
ALTER TABLE [dbo].[Subscriptions] WITH CHECK ADD 
CONSTRAINT [FK_Subscriptions_User_UserId] 
FOREIGN KEY([UserId]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [FK_Subscriptions_User_UserId]
GO

ALTER TABLE [dbo].[Subscriptions] WITH CHECK ADD 
CONSTRAINT [FK_Subscriptions_SubscriptionPlans_SubscriptionPlanId] 
FOREIGN KEY([SubscriptionPlanId]) REFERENCES [dbo].[SubscriptionPlans] ([Id])
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [FK_Subscriptions_SubscriptionPlans_SubscriptionPlanId]
GO

ALTER TABLE [dbo].[Subscriptions] WITH CHECK ADD 
CONSTRAINT [FK_Subscriptions_MasterBillingCycles_BillingCycleId] 
FOREIGN KEY([BillingCycleId]) REFERENCES [dbo].[MasterBillingCycles] ([Id])
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [FK_Subscriptions_MasterBillingCycles_BillingCycleId]
GO

ALTER TABLE [dbo].[Subscriptions] WITH CHECK ADD 
CONSTRAINT [FK_Subscriptions_Provider_ProviderId] 
FOREIGN KEY([ProviderId]) REFERENCES [dbo].[Provider] ([Id])
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [FK_Subscriptions_Provider_ProviderId]
GO

ALTER TABLE [dbo].[Subscriptions] WITH CHECK ADD 
CONSTRAINT [FK_Subscriptions_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [FK_Subscriptions_User_CreatedBy]
GO

ALTER TABLE [dbo].[Subscriptions] WITH CHECK ADD 
CONSTRAINT [FK_Subscriptions_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [FK_Subscriptions_User_UpdatedBy]
GO

ALTER TABLE [dbo].[Subscriptions] WITH CHECK ADD 
CONSTRAINT [FK_Subscriptions_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [FK_Subscriptions_User_DeletedBy]
GO

-- SubscriptionPayments foreign keys
ALTER TABLE [dbo].[SubscriptionPayments] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPayments_Subscriptions_SubscriptionId] 
FOREIGN KEY([SubscriptionId]) REFERENCES [dbo].[Subscriptions] ([Id])
GO
ALTER TABLE [dbo].[SubscriptionPayments] CHECK CONSTRAINT [FK_SubscriptionPayments_Subscriptions_SubscriptionId]
GO

ALTER TABLE [dbo].[SubscriptionPayments] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPayments_MasterCurrencies_CurrencyId] 
FOREIGN KEY([CurrencyId]) REFERENCES [dbo].[MasterCurrencies] ([Id])
GO
ALTER TABLE [dbo].[SubscriptionPayments] CHECK CONSTRAINT [FK_SubscriptionPayments_MasterCurrencies_CurrencyId]
GO

ALTER TABLE [dbo].[SubscriptionPayments] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPayments_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[SubscriptionPayments] CHECK CONSTRAINT [FK_SubscriptionPayments_User_CreatedBy]
GO

ALTER TABLE [dbo].[SubscriptionPayments] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPayments_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[SubscriptionPayments] CHECK CONSTRAINT [FK_SubscriptionPayments_User_UpdatedBy]
GO

ALTER TABLE [dbo].[SubscriptionPayments] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPayments_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[SubscriptionPayments] CHECK CONSTRAINT [FK_SubscriptionPayments_User_DeletedBy]
GO

-- BillingRecords foreign keys
ALTER TABLE [dbo].[BillingRecords] WITH CHECK ADD 
CONSTRAINT [FK_BillingRecords_User_UserId] 
FOREIGN KEY([UserId]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[BillingRecords] CHECK CONSTRAINT [FK_BillingRecords_User_UserId]
GO

ALTER TABLE [dbo].[BillingRecords] WITH CHECK ADD 
CONSTRAINT [FK_BillingRecords_Subscriptions_SubscriptionId] 
FOREIGN KEY([SubscriptionId]) REFERENCES [dbo].[Subscriptions] ([Id])
GO
ALTER TABLE [dbo].[BillingRecords] CHECK CONSTRAINT [FK_BillingRecords_Subscriptions_SubscriptionId]
GO

ALTER TABLE [dbo].[BillingRecords] WITH CHECK ADD 
CONSTRAINT [FK_BillingRecords_Consultations_ConsultationId] 
FOREIGN KEY([ConsultationId]) REFERENCES [dbo].[Consultations] ([Id])
GO
ALTER TABLE [dbo].[BillingRecords] CHECK CONSTRAINT [FK_BillingRecords_Consultations_ConsultationId]
GO

ALTER TABLE [dbo].[BillingRecords] WITH CHECK ADD 
CONSTRAINT [FK_BillingRecords_MedicationDeliveries_MedicationDeliveryId] 
FOREIGN KEY([MedicationDeliveryId]) REFERENCES [dbo].[MedicationDeliveries] ([Id])
GO
ALTER TABLE [dbo].[BillingRecords] CHECK CONSTRAINT [FK_BillingRecords_MedicationDeliveries_MedicationDeliveryId]
GO

ALTER TABLE [dbo].[BillingRecords] WITH CHECK ADD 
CONSTRAINT [FK_BillingRecords_MasterBillingCycles_BillingCycleId] 
FOREIGN KEY([BillingCycleId]) REFERENCES [dbo].[MasterBillingCycles] ([Id])
GO
ALTER TABLE [dbo].[BillingRecords] CHECK CONSTRAINT [FK_BillingRecords_MasterBillingCycles_BillingCycleId]
GO

ALTER TABLE [dbo].[BillingRecords] WITH CHECK ADD 
CONSTRAINT [FK_BillingRecords_MasterCurrencies_CurrencyId] 
FOREIGN KEY([CurrencyId]) REFERENCES [dbo].[MasterCurrencies] ([Id])
GO
ALTER TABLE [dbo].[BillingRecords] CHECK CONSTRAINT [FK_BillingRecords_MasterCurrencies_CurrencyId]
GO

ALTER TABLE [dbo].[BillingRecords] WITH CHECK ADD 
CONSTRAINT [FK_BillingRecords_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[BillingRecords] CHECK CONSTRAINT [FK_BillingRecords_User_CreatedBy]
GO

ALTER TABLE [dbo].[BillingRecords] WITH CHECK ADD 
CONSTRAINT [FK_BillingRecords_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[BillingRecords] CHECK CONSTRAINT [FK_BillingRecords_User_UpdatedBy]
GO

ALTER TABLE [dbo].[BillingRecords] WITH CHECK ADD 
CONSTRAINT [FK_BillingRecords_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[BillingRecords] CHECK CONSTRAINT [FK_BillingRecords_User_DeletedBy]
GO

-- BillingAdjustments foreign keys
ALTER TABLE [dbo].[BillingAdjustments] WITH CHECK ADD 
CONSTRAINT [FK_BillingAdjustments_BillingRecords_BillingRecordId] 
FOREIGN KEY([BillingRecordId]) REFERENCES [dbo].[BillingRecords] ([Id])
GO
ALTER TABLE [dbo].[BillingAdjustments] CHECK CONSTRAINT [FK_BillingAdjustments_BillingRecords_BillingRecordId]
GO

ALTER TABLE [dbo].[BillingAdjustments] WITH CHECK ADD 
CONSTRAINT [FK_BillingAdjustments_User_AppliedBy] 
FOREIGN KEY([AppliedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[BillingAdjustments] CHECK CONSTRAINT [FK_BillingAdjustments_User_AppliedBy]
GO

ALTER TABLE [dbo].[BillingAdjustments] WITH CHECK ADD 
CONSTRAINT [FK_BillingAdjustments_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[BillingAdjustments] CHECK CONSTRAINT [FK_BillingAdjustments_User_CreatedBy]
GO

ALTER TABLE [dbo].[BillingAdjustments] WITH CHECK ADD 
CONSTRAINT [FK_BillingAdjustments_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[BillingAdjustments] CHECK CONSTRAINT [FK_BillingAdjustments_User_UpdatedBy]
GO

ALTER TABLE [dbo].[BillingAdjustments] WITH CHECK ADD 
CONSTRAINT [FK_BillingAdjustments_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[BillingAdjustments] CHECK CONSTRAINT [FK_BillingAdjustments_User_DeletedBy]
GO

-- PaymentRefunds foreign keys
ALTER TABLE [dbo].[PaymentRefunds] WITH CHECK ADD 
CONSTRAINT [FK_PaymentRefunds_SubscriptionPayments_SubscriptionPaymentId] 
FOREIGN KEY([SubscriptionPaymentId]) REFERENCES [dbo].[SubscriptionPayments] ([Id])
GO
ALTER TABLE [dbo].[PaymentRefunds] CHECK CONSTRAINT [FK_PaymentRefunds_SubscriptionPayments_SubscriptionPaymentId]
GO

ALTER TABLE [dbo].[PaymentRefunds] WITH CHECK ADD 
CONSTRAINT [FK_PaymentRefunds_User_ProcessedByUserId] 
FOREIGN KEY([ProcessedByUserId]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[PaymentRefunds] CHECK CONSTRAINT [FK_PaymentRefunds_User_ProcessedByUserId]
GO

ALTER TABLE [dbo].[PaymentRefunds] WITH CHECK ADD 
CONSTRAINT [FK_PaymentRefunds_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[PaymentRefunds] CHECK CONSTRAINT [FK_PaymentRefunds_User_CreatedBy]
GO

ALTER TABLE [dbo].[PaymentRefunds] WITH CHECK ADD 
CONSTRAINT [FK_PaymentRefunds_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[PaymentRefunds] CHECK CONSTRAINT [FK_PaymentRefunds_User_UpdatedBy]
GO

ALTER TABLE [dbo].[PaymentRefunds] WITH CHECK ADD 
CONSTRAINT [FK_PaymentRefunds_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[PaymentRefunds] CHECK CONSTRAINT [FK_PaymentRefunds_User_DeletedBy]
GO

-- SubscriptionPlanPrivileges foreign keys
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlanPrivileges_SubscriptionPlans_SubscriptionPlanId] 
FOREIGN KEY([SubscriptionPlanId]) REFERENCES [dbo].[SubscriptionPlans] ([Id])
GO
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] CHECK CONSTRAINT [FK_SubscriptionPlanPrivileges_SubscriptionPlans_SubscriptionPlanId]
GO

ALTER TABLE [dbo].[SubscriptionPlanPrivileges] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlanPrivileges_Privileges_PrivilegeId] 
FOREIGN KEY([PrivilegeId]) REFERENCES [dbo].[Privileges] ([Id])
GO
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] CHECK CONSTRAINT [FK_SubscriptionPlanPrivileges_Privileges_PrivilegeId]
GO

ALTER TABLE [dbo].[SubscriptionPlanPrivileges] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId] 
FOREIGN KEY([UsagePeriodId]) REFERENCES [dbo].[MasterBillingCycles] ([Id])
GO
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] CHECK CONSTRAINT [FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId]
GO

ALTER TABLE [dbo].[SubscriptionPlanPrivileges] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlanPrivileges_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] CHECK CONSTRAINT [FK_SubscriptionPlanPrivileges_User_CreatedBy]
GO

ALTER TABLE [dbo].[SubscriptionPlanPrivileges] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlanPrivileges_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] CHECK CONSTRAINT [FK_SubscriptionPlanPrivileges_User_UpdatedBy]
GO

ALTER TABLE [dbo].[SubscriptionPlanPrivileges] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionPlanPrivileges_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] CHECK CONSTRAINT [FK_SubscriptionPlanPrivileges_User_DeletedBy]
GO

-- UserSubscriptionPrivilegeUsages foreign keys
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] WITH CHECK ADD 
CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_Subscriptions_SubscriptionId] 
FOREIGN KEY([SubscriptionId]) REFERENCES [dbo].[Subscriptions] ([Id])
GO
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] CHECK CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_Subscriptions_SubscriptionId]
GO

ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] WITH CHECK ADD 
CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivileges_SubscriptionPlanPrivilegeId] 
FOREIGN KEY([SubscriptionPlanPrivilegeId]) REFERENCES [dbo].[SubscriptionPlanPrivileges] ([Id])
GO
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] CHECK CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivileges_SubscriptionPlanPrivilegeId]
GO

ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] WITH CHECK ADD 
CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId] 
FOREIGN KEY([PrivilegeId]) REFERENCES [dbo].[Privileges] ([Id])
GO
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] CHECK CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId]
GO

ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] WITH CHECK ADD 
CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] CHECK CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_User_CreatedBy]
GO

ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] WITH CHECK ADD 
CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] CHECK CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_User_UpdatedBy]
GO

ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] WITH CHECK ADD 
CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] CHECK CONSTRAINT [FK_UserSubscriptionPrivilegeUsages_User_DeletedBy]
GO

-- PrivilegeUsageHistories foreign keys
ALTER TABLE [dbo].[PrivilegeUsageHistories] WITH CHECK ADD 
CONSTRAINT [FK_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsages_UserSubscriptionPrivilegeUsageId] 
FOREIGN KEY([UserSubscriptionPrivilegeUsageId]) REFERENCES [dbo].[UserSubscriptionPrivilegeUsages] ([Id])
GO
ALTER TABLE [dbo].[PrivilegeUsageHistories] CHECK CONSTRAINT [FK_PrivilegeUsageHistories_UserSubscriptionPrivilegeUsages_UserSubscriptionPrivilegeUsageId]
GO

ALTER TABLE [dbo].[PrivilegeUsageHistories] WITH CHECK ADD 
CONSTRAINT [FK_PrivilegeUsageHistories_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[PrivilegeUsageHistories] CHECK CONSTRAINT [FK_PrivilegeUsageHistories_User_CreatedBy]
GO

ALTER TABLE [dbo].[PrivilegeUsageHistories] WITH CHECK ADD 
CONSTRAINT [FK_PrivilegeUsageHistories_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[PrivilegeUsageHistories] CHECK CONSTRAINT [FK_PrivilegeUsageHistories_User_UpdatedBy]
GO

ALTER TABLE [dbo].[PrivilegeUsageHistories] WITH CHECK ADD 
CONSTRAINT [FK_PrivilegeUsageHistories_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[PrivilegeUsageHistories] CHECK CONSTRAINT [FK_PrivilegeUsageHistories_User_DeletedBy]
GO

-- SubscriptionStatusHistories foreign keys
ALTER TABLE [dbo].[SubscriptionStatusHistories] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionStatusHistories_Subscriptions_SubscriptionId] 
FOREIGN KEY([SubscriptionId]) REFERENCES [dbo].[Subscriptions] ([Id])
GO
ALTER TABLE [dbo].[SubscriptionStatusHistories] CHECK CONSTRAINT [FK_SubscriptionStatusHistories_Subscriptions_SubscriptionId]
GO

ALTER TABLE [dbo].[SubscriptionStatusHistories] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionStatusHistories_User_ChangedByUserId] 
FOREIGN KEY([ChangedByUserId]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[SubscriptionStatusHistories] CHECK CONSTRAINT [FK_SubscriptionStatusHistories_User_ChangedByUserId]
GO

ALTER TABLE [dbo].[SubscriptionStatusHistories] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionStatusHistories_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[SubscriptionStatusHistories] CHECK CONSTRAINT [FK_SubscriptionStatusHistories_User_CreatedBy]
GO

ALTER TABLE [dbo].[SubscriptionStatusHistories] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionStatusHistories_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[SubscriptionStatusHistories] CHECK CONSTRAINT [FK_SubscriptionStatusHistories_User_UpdatedBy]
GO

ALTER TABLE [dbo].[SubscriptionStatusHistories] WITH CHECK ADD 
CONSTRAINT [FK_SubscriptionStatusHistories_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[SubscriptionStatusHistories] CHECK CONSTRAINT [FK_SubscriptionStatusHistories_User_DeletedBy]
GO

-- ServiceConstraints foreign keys
ALTER TABLE [dbo].[ServiceConstraints] WITH CHECK ADD 
CONSTRAINT [FK_ServiceConstraints_SubscriptionPlans_SubscriptionPlanId] 
FOREIGN KEY([SubscriptionPlanId]) REFERENCES [dbo].[SubscriptionPlans] ([Id])
GO
ALTER TABLE [dbo].[ServiceConstraints] CHECK CONSTRAINT [FK_ServiceConstraints_SubscriptionPlans_SubscriptionPlanId]
GO

ALTER TABLE [dbo].[ServiceConstraints] WITH CHECK ADD 
CONSTRAINT [FK_ServiceConstraints_User_CreatedBy] 
FOREIGN KEY([CreatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[ServiceConstraints] CHECK CONSTRAINT [FK_ServiceConstraints_User_CreatedBy]
GO

ALTER TABLE [dbo].[ServiceConstraints] WITH CHECK ADD 
CONSTRAINT [FK_ServiceConstraints_User_UpdatedBy] 
FOREIGN KEY([UpdatedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[ServiceConstraints] CHECK CONSTRAINT [FK_ServiceConstraints_User_UpdatedBy]
GO

ALTER TABLE [dbo].[ServiceConstraints] WITH CHECK ADD 
CONSTRAINT [FK_ServiceConstraints_User_DeletedBy] 
FOREIGN KEY([DeletedBy]) REFERENCES [dbo].[User] ([UserID])
GO
ALTER TABLE [dbo].[ServiceConstraints] CHECK CONSTRAINT [FK_ServiceConstraints_User_DeletedBy]
GO

-- =====================================================
-- 11. CHECK CONSTRAINTS FOR ALL TABLES
-- =====================================================

-- Subscriptions constraints
ALTER TABLE [dbo].[Subscriptions] WITH CHECK ADD 
CONSTRAINT [CK_Subscriptions_Status_NotEmpty] 
CHECK (LEN(TRIM([Status])) > 0)
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [CK_Subscriptions_Status_NotEmpty]
GO

ALTER TABLE [dbo].[Subscriptions] WITH CHECK ADD 
CONSTRAINT [CK_Subscriptions_CurrentPrice_Positive] 
CHECK ([CurrentPrice] >= 0)
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [CK_Subscriptions_CurrentPrice_Positive]
GO

ALTER TABLE [dbo].[Subscriptions] WITH CHECK ADD 
CONSTRAINT [CK_Subscriptions_FailedPaymentAttempts_NonNegative] 
CHECK ([FailedPaymentAttempts] >= 0)
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [CK_Subscriptions_FailedPaymentAttempts_NonNegative]
GO

ALTER TABLE [dbo].[Subscriptions] WITH CHECK ADD 
CONSTRAINT [CK_Subscriptions_TrialDuration_NonNegative] 
CHECK ([TrialDurationInDays] >= 0)
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [CK_Subscriptions_TrialDuration_NonNegative]
GO

ALTER TABLE [dbo].[Subscriptions] WITH CHECK ADD 
CONSTRAINT [CK_Subscriptions_TotalUsageCount_NonNegative] 
CHECK ([TotalUsageCount] >= 0)
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [CK_Subscriptions_TotalUsageCount_NonNegative]
GO

-- SubscriptionPayments constraints
ALTER TABLE [dbo].[SubscriptionPayments] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPayments_Amount_Positive] 
CHECK ([Amount] > 0)
GO
ALTER TABLE [dbo].[SubscriptionPayments] CHECK CONSTRAINT [CK_SubscriptionPayments_Amount_Positive]
GO

ALTER TABLE [dbo].[SubscriptionPayments] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPayments_TaxAmount_NonNegative] 
CHECK ([TaxAmount] >= 0)
GO
ALTER TABLE [dbo].[SubscriptionPayments] CHECK CONSTRAINT [CK_SubscriptionPayments_TaxAmount_NonNegative]
GO

ALTER TABLE [dbo].[SubscriptionPayments] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPayments_NetAmount_Positive] 
CHECK ([NetAmount] > 0)
GO
ALTER TABLE [dbo].[SubscriptionPayments] CHECK CONSTRAINT [CK_SubscriptionPayments_NetAmount_Positive]
GO

ALTER TABLE [dbo].[SubscriptionPayments] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPayments_Description_NotEmpty] 
CHECK (LEN(TRIM([Description])) > 0)
GO
ALTER TABLE [dbo].[SubscriptionPayments] CHECK CONSTRAINT [CK_SubscriptionPayments_Description_NotEmpty]
GO

ALTER TABLE [dbo].[SubscriptionPayments] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPayments_AttemptCount_NonNegative] 
CHECK ([AttemptCount] >= 0)
GO
ALTER TABLE [dbo].[SubscriptionPayments] CHECK CONSTRAINT [CK_SubscriptionPayments_AttemptCount_NonNegative]
GO

ALTER TABLE [dbo].[SubscriptionPayments] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPayments_RefundedAmount_NonNegative] 
CHECK ([RefundedAmount] >= 0)
GO
ALTER TABLE [dbo].[SubscriptionPayments] CHECK CONSTRAINT [CK_SubscriptionPayments_RefundedAmount_NonNegative]
GO

-- BillingRecords constraints
ALTER TABLE [dbo].[BillingRecords] WITH CHECK ADD 
CONSTRAINT [CK_BillingRecords_Amount_NonNegative] 
CHECK ([Amount] >= 0)
GO
ALTER TABLE [dbo].[BillingRecords] CHECK CONSTRAINT [CK_BillingRecords_Amount_NonNegative]
GO

ALTER TABLE [dbo].[BillingRecords] WITH CHECK ADD 
CONSTRAINT [CK_BillingRecords_TaxAmount_NonNegative] 
CHECK ([TaxAmount] >= 0)
GO
ALTER TABLE [dbo].[BillingRecords] CHECK CONSTRAINT [CK_BillingRecords_TaxAmount_NonNegative]
GO

ALTER TABLE [dbo].[BillingRecords] WITH CHECK ADD 
CONSTRAINT [CK_BillingRecords_ShippingAmount_NonNegative] 
CHECK ([ShippingAmount] >= 0)
GO
ALTER TABLE [dbo].[BillingRecords] CHECK CONSTRAINT [CK_BillingRecords_ShippingAmount_NonNegative]
GO

ALTER TABLE [dbo].[BillingRecords] WITH CHECK ADD 
CONSTRAINT [CK_BillingRecords_TotalAmount_NonNegative] 
CHECK ([TotalAmount] >= 0)
GO
ALTER TABLE [dbo].[BillingRecords] CHECK CONSTRAINT [CK_BillingRecords_TotalAmount_NonNegative]
GO

-- BillingAdjustments constraints
ALTER TABLE [dbo].[BillingAdjustments] WITH CHECK ADD 
CONSTRAINT [CK_BillingAdjustments_Amount_NonZero] 
CHECK ([Amount] != 0)
GO
ALTER TABLE [dbo].[BillingAdjustments] CHECK CONSTRAINT [CK_BillingAdjustments_Amount_NonZero]
GO

ALTER TABLE [dbo].[BillingAdjustments] WITH CHECK ADD 
CONSTRAINT [CK_BillingAdjustments_Description_NotEmpty] 
CHECK (LEN(TRIM([Description])) > 0)
GO
ALTER TABLE [dbo].[BillingAdjustments] CHECK CONSTRAINT [CK_BillingAdjustments_Description_NotEmpty]
GO

ALTER TABLE [dbo].[BillingAdjustments] WITH CHECK ADD 
CONSTRAINT [CK_BillingAdjustments_Percentage_Range] 
CHECK ([Percentage] IS NULL OR ([Percentage] >= 0 AND [Percentage] <= 100))
GO
ALTER TABLE [dbo].[BillingAdjustments] CHECK CONSTRAINT [CK_BillingAdjustments_Percentage_Range]
GO

-- PaymentRefunds constraints
ALTER TABLE [dbo].[PaymentRefunds] WITH CHECK ADD 
CONSTRAINT [CK_PaymentRefunds_Amount_Positive] 
CHECK ([Amount] > 0)
GO
ALTER TABLE [dbo].[PaymentRefunds] CHECK CONSTRAINT [CK_PaymentRefunds_Amount_Positive]
GO

ALTER TABLE [dbo].[PaymentRefunds] WITH CHECK ADD 
CONSTRAINT [CK_PaymentRefunds_Reason_NotEmpty] 
CHECK (LEN(TRIM([Reason])) > 0)
GO
ALTER TABLE [dbo].[PaymentRefunds] CHECK CONSTRAINT [CK_PaymentRefunds_Reason_NotEmpty]
GO

-- SubscriptionPlanPrivileges constraints
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlanPrivileges_DurationMonths_Positive] 
CHECK ([DurationMonths] > 0)
GO
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] CHECK CONSTRAINT [CK_SubscriptionPlanPrivileges_DurationMonths_Positive]
GO

ALTER TABLE [dbo].[SubscriptionPlanPrivileges] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlanPrivileges_UnitCost_NonNegative] 
CHECK ([UnitCost] >= 0)
GO
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] CHECK CONSTRAINT [CK_SubscriptionPlanPrivileges_UnitCost_NonNegative]
GO

ALTER TABLE [dbo].[SubscriptionPlanPrivileges] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionPlanPrivileges_ExpirationDate_Future] 
CHECK ([ExpirationDate] IS NULL OR [ExpirationDate] > GETUTCDATE())
GO
ALTER TABLE [dbo].[SubscriptionPlanPrivileges] CHECK CONSTRAINT [CK_SubscriptionPlanPrivileges_ExpirationDate_Future]
GO

-- UserSubscriptionPrivilegeUsages constraints
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] WITH CHECK ADD 
CONSTRAINT [CK_UserSubscriptionPrivilegeUsages_UsedValue_NonNegative] 
CHECK ([UsedValue] >= 0)
GO
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] CHECK CONSTRAINT [CK_UserSubscriptionPrivilegeUsages_UsedValue_NonNegative]
GO

ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] WITH CHECK ADD 
CONSTRAINT [CK_UserSubscriptionPrivilegeUsages_AllowedValue_Valid] 
CHECK ([AllowedValue] >= -1)
GO
ALTER TABLE [dbo].[UserSubscriptionPrivilegeUsages] CHECK CONSTRAINT [CK_UserSubscriptionPrivilegeUsages_AllowedValue_Valid]
GO

-- PrivilegeUsageHistories constraints
ALTER TABLE [dbo].[PrivilegeUsageHistories] WITH CHECK ADD 
CONSTRAINT [CK_PrivilegeUsageHistories_UsedValue_Positive] 
CHECK ([UsedValue] > 0)
GO
ALTER TABLE [dbo].[PrivilegeUsageHistories] CHECK CONSTRAINT [CK_PrivilegeUsageHistories_UsedValue_Positive]
GO

ALTER TABLE [dbo].[PrivilegeUsageHistories] WITH CHECK ADD 
CONSTRAINT [CK_PrivilegeUsageHistories_UsageWeek_Format] 
CHECK ([UsageWeek] LIKE '[0-9][0-9][0-9][0-9]-[0-9][0-9]')
GO
ALTER TABLE [dbo].[PrivilegeUsageHistories] CHECK CONSTRAINT [CK_PrivilegeUsageHistories_UsageWeek_Format]
GO

ALTER TABLE [dbo].[PrivilegeUsageHistories] WITH CHECK ADD 
CONSTRAINT [CK_PrivilegeUsageHistories_UsageMonth_Format] 
CHECK ([UsageMonth] LIKE '[0-9][0-9][0-9][0-9]-[0-9][0-9]')
GO
ALTER TABLE [dbo].[PrivilegeUsageHistories] CHECK CONSTRAINT [CK_PrivilegeUsageHistories_UsageMonth_Format]
GO

-- SubscriptionStatusHistories constraints
ALTER TABLE [dbo].[SubscriptionStatusHistories] WITH CHECK ADD 
CONSTRAINT [CK_SubscriptionStatusHistories_ToStatus_NotEmpty] 
CHECK (LEN(TRIM([ToStatus])) > 0)
GO
ALTER TABLE [dbo].[SubscriptionStatusHistories] CHECK CONSTRAINT [CK_SubscriptionStatusHistories_ToStatus_NotEmpty]
GO

-- ServiceConstraints constraints
ALTER TABLE [dbo].[ServiceConstraints] WITH CHECK ADD 
CONSTRAINT [CK_ServiceConstraints_ServiceName_NotEmpty] 
CHECK (LEN(TRIM([ServiceName])) > 0)
GO
ALTER TABLE [dbo].[ServiceConstraints] CHECK CONSTRAINT [CK_ServiceConstraints_ServiceName_NotEmpty]
GO

ALTER TABLE [dbo].[ServiceConstraints] WITH CHECK ADD 
CONSTRAINT [CK_ServiceConstraints_Description_NotEmpty] 
CHECK (LEN(TRIM([Description])) > 0)
GO
ALTER TABLE [dbo].[ServiceConstraints] CHECK CONSTRAINT [CK_ServiceConstraints_Description_NotEmpty]
GO

ALTER TABLE [dbo].[ServiceConstraints] WITH CHECK ADD 
CONSTRAINT [CK_ServiceConstraints_MaxSessionsPerMonth_NonNegative] 
CHECK ([MaxSessionsPerMonth] >= 0)
GO
ALTER TABLE [dbo].[ServiceConstraints] CHECK CONSTRAINT [CK_ServiceConstraints_MaxSessionsPerMonth_NonNegative]
GO

ALTER TABLE [dbo].[ServiceConstraints] WITH CHECK ADD 
CONSTRAINT [CK_ServiceConstraints_MaxDurationPerSession_NonNegative] 
CHECK ([MaxDurationPerSession] >= 0)
GO
ALTER TABLE [dbo].[ServiceConstraints] CHECK CONSTRAINT [CK_ServiceConstraints_MaxDurationPerSession_NonNegative]
GO

ALTER TABLE [dbo].[ServiceConstraints] WITH CHECK ADD 
CONSTRAINT [CK_ServiceConstraints_MaxConcurrentSessions_NonNegative] 
CHECK ([MaxConcurrentSessions] >= 0)
GO
ALTER TABLE [dbo].[ServiceConstraints] CHECK CONSTRAINT [CK_ServiceConstraints_MaxConcurrentSessions_NonNegative]
GO

ALTER TABLE [dbo].[ServiceConstraints] WITH CHECK ADD 
CONSTRAINT [CK_ServiceConstraints_TotalMinutesPerMonth_NonNegative] 
CHECK ([TotalMinutesPerMonth] IS NULL OR [TotalMinutesPerMonth] >= 0)
GO
ALTER TABLE [dbo].[ServiceConstraints] CHECK CONSTRAINT [CK_ServiceConstraints_TotalMinutesPerMonth_NonNegative]
GO

ALTER TABLE [dbo].[ServiceConstraints] WITH CHECK ADD 
CONSTRAINT [CK_ServiceConstraints_MaxMessageLength_Positive] 
CHECK ([MaxMessageLength] > 0)
GO
ALTER TABLE [dbo].[ServiceConstraints] CHECK CONSTRAINT [CK_ServiceConstraints_MaxMessageLength_Positive]
GO

-- =====================================================
-- 12. VERIFICATION QUERIES
-- =====================================================
PRINT 'All foreign keys, constraints, and validation rules added successfully!'
PRINT 'Verifying constraint creation...'

-- Check constraints
SELECT 
    'Check Constraints' as ConstraintType,
    COUNT(*) as Count
FROM sys.check_constraints 
WHERE parent_object_id IN (
    SELECT object_id FROM sys.objects 
    WHERE type = 'U' 
    AND name IN (
        'MasterPrivilegeTypes', 'MasterCurrencies', 'MasterBillingCycles',
        'Privileges', 'SubscriptionPlans', 'Subscriptions', 'SubscriptionPayments',
        'BillingRecords', 'BillingAdjustments', 'PaymentRefunds',
        'SubscriptionPlanPrivileges', 'UserSubscriptionPrivilegeUsages',
        'PrivilegeUsageHistories', 'SubscriptionStatusHistories', 'ServiceConstraints'
    )
)

UNION ALL

-- Check foreign keys
SELECT 
    'Foreign Keys' as ConstraintType,
    COUNT(*) as Count
FROM sys.foreign_keys 
WHERE parent_object_id IN (
    SELECT object_id FROM sys.objects 
    WHERE type = 'U' 
    AND name IN (
        'MasterPrivilegeTypes', 'MasterCurrencies', 'MasterBillingCycles',
        'Privileges', 'SubscriptionPlans', 'Subscriptions', 'SubscriptionPayments',
        'BillingRecords', 'BillingAdjustments', 'PaymentRefunds',
        'SubscriptionPlanPrivileges', 'UserSubscriptionPrivilegeUsages',
        'PrivilegeUsageHistories', 'SubscriptionStatusHistories', 'ServiceConstraints'
    )
)

PRINT 'Foreign keys and constraints verification completed!'
