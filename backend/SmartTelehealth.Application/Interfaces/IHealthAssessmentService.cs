using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IHealthAssessmentService
{
    // CRUD Operations
    Task<JsonModel> CreateAssessmentAsync(CreateHealthAssessmentDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetAssessmentByIdAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetUserAssessmentsAsync(int userId, TokenModel tokenModel);
    Task<JsonModel> GetPendingAssessmentsAsync(TokenModel tokenModel);
    Task<JsonModel> UpdateAssessmentAsync(Guid id, UpdateHealthAssessmentDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeleteAssessmentAsync(Guid id, TokenModel tokenModel);
    
    // Assessment Management
    Task<JsonModel> ReviewAssessmentAsync(Guid id, int providerId, bool isEligible, string notes, TokenModel tokenModel);
    Task<JsonModel> CompleteAssessmentAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> CancelAssessmentAsync(Guid id, string reason, TokenModel tokenModel);
    
    // Assessment Templates
    Task<JsonModel> CreateAssessmentTemplateAsync(CreateAssessmentTemplateDto createDto, TokenModel tokenModel);
    Task<JsonModel> GetAssessmentTemplateAsync(Guid id, TokenModel tokenModel);
    Task<JsonModel> GetAssessmentTemplatesByCategoryAsync(Guid categoryId, TokenModel tokenModel);
    Task<JsonModel> UpdateAssessmentTemplateAsync(Guid id, UpdateAssessmentTemplateDto updateDto, TokenModel tokenModel);
    Task<JsonModel> DeleteAssessmentTemplateAsync(Guid id, TokenModel tokenModel);
    
    // Assessment Reports
    Task<JsonModel> GenerateAssessmentReportAsync(Guid assessmentId, TokenModel tokenModel);
    Task<JsonModel> ExportAssessmentReportAsync(Guid assessmentId, string format, TokenModel tokenModel);
    Task<JsonModel> GetAssessmentReportsAsync(int userId, DateTime? startDate, DateTime? endDate, TokenModel tokenModel);
    
    // Provider Workflow
    Task<JsonModel> GetProviderPendingAssessmentsAsync(int providerId, TokenModel tokenModel);
    Task<JsonModel> GetProviderReviewedAssessmentsAsync(int providerId, TokenModel tokenModel);
    Task<JsonModel> AssignAssessmentToProviderAsync(Guid assessmentId, int providerId, TokenModel tokenModel);
} 