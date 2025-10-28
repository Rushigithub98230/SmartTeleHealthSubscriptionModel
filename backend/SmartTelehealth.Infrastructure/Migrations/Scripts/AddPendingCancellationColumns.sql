-- Migration: AddPendingCancellationToSubscriptions
-- Date: 2025-10-28
-- Description: Adds columns to support pending cancellation at renewal feature

-- Check if columns don't exist before adding them
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND name = 'PendingCancellationAtRenewal')
BEGIN
    ALTER TABLE [dbo].[Subscriptions]
    ADD [PendingCancellationAtRenewal] BIT NOT NULL DEFAULT 0;
    
    PRINT 'Column PendingCancellationAtRenewal added successfully';
END
ELSE
BEGIN
    PRINT 'Column PendingCancellationAtRenewal already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND name = 'PendingCancellationReason')
BEGIN
    ALTER TABLE [dbo].[Subscriptions]
    ADD [PendingCancellationReason] NVARCHAR(500) NULL;
    
    PRINT 'Column PendingCancellationReason added successfully';
END
ELSE
BEGIN
    PRINT 'Column PendingCancellationReason already exists';
END

PRINT 'Migration completed successfully';

