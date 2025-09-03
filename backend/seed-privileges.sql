-- Insert additional privileges into the database
-- This script adds the missing privileges that are needed for the stepper form

-- First, let's get the privilege type IDs (these should already exist from seeding)
-- Consultation: fb67b6dc-2103-4c29-a5e7-938525661868
-- Messaging: ee190e72-e8df-44ae-baf7-f9de3da46fd5
-- Document: f1a946f5-f0d0-4d75-b1b4-776af69f012b
-- Video: 52702554-45c7-42a3-bba2-061946d0edb0
-- Prescription: 87971d8a-efca-463c-9d50-cd8346942c7e

-- Insert TeleConsultation privilege
INSERT INTO Privileges (Id, Name, Description, PrivilegeTypeId, IsActive, IsDeleted, CreatedBy, CreatedDate)
VALUES (
    NEWID(),
    'TeleConsultation',
    'Video consultation with healthcare providers',
    'fb67b6dc-2103-4c29-a5e7-938525661868', -- Consultation type
    1, -- IsActive
    0, -- IsDeleted
    1, -- CreatedBy (system user)
    GETUTCDATE()
);

-- Insert Medication privilege
INSERT INTO Privileges (Id, Name, Description, PrivilegeTypeId, IsActive, IsDeleted, CreatedBy, CreatedDate)
VALUES (
    NEWID(),
    'Medication',
    'Access to medication prescriptions and delivery',
    '87971d8a-efca-463c-9d50-cd8346942c7e', -- Prescription type
    1, -- IsActive
    0, -- IsDeleted
    1, -- CreatedBy (system user)
    GETUTCDATE()
);

-- Insert Unlimited Messaging privilege
INSERT INTO Privileges (Id, Name, Description, PrivilegeTypeId, IsActive, IsDeleted, CreatedBy, CreatedDate)
VALUES (
    NEWID(),
    'Unlimited Messaging',
    'Unlimited messaging with healthcare providers',
    'ee190e72-e8df-44ae-baf7-f9de3da46fd5', -- Messaging type
    1, -- IsActive
    0, -- IsDeleted
    1, -- CreatedBy (system user)
    GETUTCDATE()
);

-- Insert Document Access privilege
INSERT INTO Privileges (Id, Name, Description, PrivilegeTypeId, IsActive, IsDeleted, CreatedBy, CreatedDate)
VALUES (
    NEWID(),
    'Document Access',
    'Access to medical documents and reports',
    'f1a946f5-f0d0-4d75-b1b4-776af69f012b', -- Document type
    1, -- IsActive
    0, -- IsDeleted
    1, -- CreatedBy (system user)
    GETUTCDATE()
);

-- Insert Priority Support privilege
INSERT INTO Privileges (Id, Name, Description, PrivilegeTypeId, IsActive, IsDeleted, CreatedBy, CreatedDate)
VALUES (
    NEWID(),
    'Priority Support',
    'Priority customer support access',
    'fb67b6dc-2103-4c29-a5e7-938525661868', -- Consultation type
    1, -- IsActive
    0, -- IsDeleted
    1, -- CreatedBy (system user)
    GETUTCDATE()
);

-- Insert Lab Test Access privilege
INSERT INTO Privileges (Id, Name, Description, PrivilegeTypeId, IsActive, IsDeleted, CreatedBy, CreatedDate)
VALUES (
    NEWID(),
    'Lab Test Access',
    'Access to lab test results and recommendations',
    'f1a946f5-f0d0-4d75-b1b4-776af69f012b', -- Document type
    1, -- IsActive
    0, -- IsDeleted
    1, -- CreatedBy (system user)
    GETUTCDATE()
);
