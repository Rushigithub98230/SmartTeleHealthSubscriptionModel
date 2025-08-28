using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.Infrastructure.Services;

public class DocumentTypeSeedService
{
    private readonly IDocumentTypeService _documentTypeService;
    private readonly ILogger<DocumentTypeSeedService> _logger;

    public DocumentTypeSeedService(
        IDocumentTypeService documentTypeService,
        ILogger<DocumentTypeSeedService> logger)
    {
        _documentTypeService = documentTypeService;
        _logger = logger;
    }

    public async Task SeedSystemDocumentTypesAsync()
    {
        try
        {
            _logger.LogInformation("Starting to seed system document types...");

            var systemDocumentTypes = new List<CreateDocumentTypeRequest>
            {
                // Medical Documents
                new CreateDocumentTypeRequest
                {
                    Name = "Prescription",
                    Description = "Medical prescriptions from healthcare providers",
                    IsSystemDefined = true,
                    IsActive = true,
                    AllowedExtensions = ".pdf,.jpg,.jpeg,.png",
                    MaxFileSizeBytes = 5 * 1024 * 1024, // 5MB
                    RequireFileValidation = true,
                    Icon = "prescription-icon",
                    Color = "#4CAF50",
                    DisplayOrder = 1,
                    CreatedById = Guid.Empty
                },
                new CreateDocumentTypeRequest
                {
                    Name = "Lab Report",
                    Description = "Laboratory test results and reports",
                    IsSystemDefined = true,
                    IsActive = true,
                    AllowedExtensions = ".pdf,.jpg,.jpeg,.png",
                    MaxFileSizeBytes = 10 * 1024 * 1024, // 10MB
                    RequireFileValidation = true,
                    Icon = "lab-report-icon",
                    Color = "#2196F3",
                    DisplayOrder = 2,
                    CreatedById = Guid.Empty
                },
                new CreateDocumentTypeRequest
                {
                    Name = "Medical Certificate",
                    Description = "Official medical certificates from healthcare providers",
                    IsSystemDefined = true,
                    IsActive = true,
                    AllowedExtensions = ".pdf,.jpg,.jpeg,.png",
                    MaxFileSizeBytes = 5 * 1024 * 1024, // 5MB
                    RequireFileValidation = true,
                    Icon = "certificate-icon",
                    Color = "#FF9800",
                    DisplayOrder = 3,
                    CreatedById = Guid.Empty
                },
                new CreateDocumentTypeRequest
                {
                    Name = "Visit Summary",
                    Description = "Summary of medical visits and consultations",
                    IsSystemDefined = true,
                    IsActive = true,
                    AllowedExtensions = ".pdf,.doc,.docx",
                    MaxFileSizeBytes = 5 * 1024 * 1024, // 5MB
                    RequireFileValidation = true,
                    Icon = "visit-summary-icon",
                    Color = "#9C27B0",
                    DisplayOrder = 4,
                    CreatedById = Guid.Empty
                },

                // Identity Documents
                new CreateDocumentTypeRequest
                {
                    Name = "Government ID",
                    Description = "Government-issued identification documents",
                    IsSystemDefined = true,
                    IsActive = true,
                    AllowedExtensions = ".pdf,.jpg,.jpeg,.png",
                    MaxFileSizeBytes = 3 * 1024 * 1024, // 3MB
                    RequireFileValidation = true,
                    Icon = "id-icon",
                    Color = "#607D8B",
                    DisplayOrder = 5,
                    CreatedById = Guid.Empty
                },
                new CreateDocumentTypeRequest
                {
                    Name = "Driver's License",
                    Description = "Driver's license documents",
                    IsSystemDefined = true,
                    IsActive = true,
                    AllowedExtensions = ".pdf,.jpg,.jpeg,.png",
                    MaxFileSizeBytes = 3 * 1024 * 1024, // 3MB
                    RequireFileValidation = true,
                    Icon = "license-icon",
                    Color = "#795548",
                    DisplayOrder = 6,
                    CreatedById = Guid.Empty
                },
                new CreateDocumentTypeRequest
                {
                    Name = "Passport",
                    Description = "Passport documents",
                    IsSystemDefined = true,
                    IsActive = true,
                    AllowedExtensions = ".pdf,.jpg,.jpeg,.png",
                    MaxFileSizeBytes = 3 * 1024 * 1024, // 3MB
                    RequireFileValidation = true,
                    Icon = "passport-icon",
                    Color = "#E91E63",
                    DisplayOrder = 7,
                    CreatedById = Guid.Empty
                },

                // Insurance Documents
                new CreateDocumentTypeRequest
                {
                    Name = "Insurance Card",
                    Description = "Health insurance cards and documents",
                    IsSystemDefined = true,
                    IsActive = true,
                    AllowedExtensions = ".pdf,.jpg,.jpeg,.png",
                    MaxFileSizeBytes = 2 * 1024 * 1024, // 2MB
                    RequireFileValidation = true,
                    Icon = "insurance-icon",
                    Color = "#00BCD4",
                    DisplayOrder = 8,
                    CreatedById = Guid.Empty
                },
                new CreateDocumentTypeRequest
                {
                    Name = "Insurance Policy",
                    Description = "Insurance policy documents",
                    IsSystemDefined = true,
                    IsActive = true,
                    AllowedExtensions = ".pdf,.doc,.docx",
                    MaxFileSizeBytes = 10 * 1024 * 1024, // 10MB
                    RequireFileValidation = true,
                    Icon = "policy-icon",
                    Color = "#3F51B5",
                    DisplayOrder = 9,
                    CreatedById = Guid.Empty
                },

                // System Documents
                new CreateDocumentTypeRequest
                {
                    Name = "Invoice",
                    Description = "System-generated invoices and billing documents",
                    IsSystemDefined = true,
                    IsActive = true,
                    AllowedExtensions = ".pdf",
                    MaxFileSizeBytes = 2 * 1024 * 1024, // 2MB
                    RequireFileValidation = true,
                    Icon = "invoice-icon",
                    Color = "#F44336",
                    DisplayOrder = 10,
                    CreatedById = Guid.Empty
                },
                new CreateDocumentTypeRequest
                {
                    Name = "Receipt",
                    Description = "Payment receipts and confirmations",
                    IsSystemDefined = true,
                    IsActive = true,
                    AllowedExtensions = ".pdf,.jpg,.jpeg,.png",
                    MaxFileSizeBytes = 2 * 1024 * 1024, // 2MB
                    RequireFileValidation = true,
                    Icon = "receipt-icon",
                    Color = "#8BC34A",
                    DisplayOrder = 11,
                    CreatedById = Guid.Empty
                },

                // Profile Documents
                new CreateDocumentTypeRequest
                {
                    Name = "Profile Picture",
                    Description = "User profile pictures and avatars",
                    IsSystemDefined = true,
                    IsActive = true,
                    AllowedExtensions = ".jpg,.jpeg,.png,.gif",
                    MaxFileSizeBytes = 2 * 1024 * 1024, // 2MB
                    RequireFileValidation = true,
                    Icon = "profile-icon",
                    Color = "#FF5722",
                    DisplayOrder = 12,
                    CreatedById = Guid.Empty
                },

                // General Documents
                new CreateDocumentTypeRequest
                {
                    Name = "General Document",
                    Description = "General purpose documents",
                    IsSystemDefined = true,
                    IsActive = true,
                    AllowedExtensions = ".pdf,.doc,.docx,.txt,.jpg,.jpeg,.png",
                    MaxFileSizeBytes = 10 * 1024 * 1024, // 10MB
                    RequireFileValidation = true,
                    Icon = "document-icon",
                    Color = "#757575",
                    DisplayOrder = 13,
                    CreatedById = Guid.Empty
                }
            };

            foreach (var documentType in systemDocumentTypes)
            {
                try
                {
                    var result = await _documentTypeService.CreateSystemDocumentTypeAsync(documentType);
                    if (result.StatusCode == 200)
                    {
                        _logger.LogInformation("Successfully seeded document type");
                    }
                    else
                    {
                        _logger.LogWarning("Failed to seed document type");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error seeding document type");
                }
            }

            _logger.LogInformation("Completed seeding system document types");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during document type seeding");
        }
    }

    public async Task<bool> IsSeedingRequiredAsync()
    {
        try
        {
            var systemTypes = await _documentTypeService.GetSystemDocumentTypesAsync();
            
            // Check if data is a collection and has items
            if (systemTypes.data is IEnumerable<object> collection)
            {
                return systemTypes.StatusCode != 200 || !collection.Any();
            }
            
            // If data is not a collection, assume seeding is required
            return systemTypes.StatusCode != 200 || systemTypes.data == null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if seeding is required");
            return true; // Default to seeding if check fails
        }
    }
} 