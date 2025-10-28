# SQL Fix Required for Admin Logs System

## Issue
The `ApplicationLogs.AdditionalData` column is currently set to `nvarchar(2000)` which is too small for large JSON log data. This causes truncation errors when storing logs with extensive additional information.

## Solution
Run the following SQL command on your SQL Server database:

```sql
USE SmartTelehealthDblatest;
GO

-- Alter the AdditionalData column to allow unlimited text
ALTER TABLE ApplicationLogs 
ALTER COLUMN AdditionalData nvarchar(max) NULL;
GO
```

## Verification
After running the SQL command:

1. Check the column type in SQL Server Management Studio
2. Restart the backend application
3. The logs should now be captured without truncation errors
4. Navigate to `/webadmin/logs` in the admin portal to view logs in real-time

## Alternative Approach
If you prefer to drop and recreate the table (this will delete existing logs):

```sql
USE SmartTelehealthDblatest;
GO

-- Drop the ApplicationLogs table
DROP TABLE IF EXISTS ApplicationLogs;
GO

-- Recreate with correct schema
CREATE TABLE ApplicationLogs (
    Id bigint IDENTITY(1,1) PRIMARY KEY,
    Timestamp datetime2 NOT NULL,
    LogLevel nvarchar(50) NOT NULL,
    Source nvarchar(200) NOT NULL,
    Message nvarchar(max) NOT NULL,
    Exception nvarchar(max) NULL,
    UserId int NULL,
    Operation nvarchar(100) NULL,
    AdditionalData nvarchar(max) NULL,  -- Fixed to nvarchar(max)
    CorrelationId nvarchar(100) NULL,
    IsActive bit NOT NULL DEFAULT 1,
    IsDeleted bit NOT NULL DEFAULT 0,
    CreatedBy int NULL,
    CreatedDate datetime2 NULL,
    UpdatedBy int NULL,
    UpdatedDate datetime2 NULL,
    DeletedBy int NULL,
    DeletedDate datetime2 NULL,
    CONSTRAINT FK_ApplicationLogs_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

-- Create indexes
CREATE INDEX IX_ApplicationLogs_Timestamp ON ApplicationLogs(Timestamp);
CREATE INDEX IX_ApplicationLogs_LogLevel ON ApplicationLogs(LogLevel);
CREATE INDEX IX_ApplicationLogs_Source ON ApplicationLogs(Source);
CREATE INDEX IX_ApplicationLogs_UserId ON ApplicationLogs(UserId);
GO
```

## What's Already Done
- ✅ DatabaseLogSink is enabled in Program.cs
- ✅ Real-time SignalR broadcasting is configured
- ✅ Frontend components are ready
- ✅ All services are properly registered

## Next Steps
1. Run the SQL command above
2. Run `npm install` in the frontend directory to install @microsoft/signalr
3. Restart the backend application
4. Start the frontend and navigate to `/webadmin/logs`
5. You should see real-time logs streaming in!

