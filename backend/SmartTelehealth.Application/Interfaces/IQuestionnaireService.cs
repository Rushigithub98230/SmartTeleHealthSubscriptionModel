using SmartTelehealth.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IQuestionnaireService
    {
        // Template CRUD
        Task<JsonModel> GetAllTemplatesAsync();
        Task<JsonModel> GetTemplateByIdAsync(Guid id);
        Task<JsonModel> GetTemplatesByCategoryAsync(Guid categoryId);
        Task<JsonModel> CreateTemplateAsync(CreateQuestionnaireTemplateDto dto, List<IFormFile> files);
        Task<JsonModel> UpdateTemplateAsync(Guid id, CreateQuestionnaireTemplateDto dto, List<IFormFile> files);
        Task<JsonModel> DeleteTemplateAsync(Guid id);

        // User responses
        Task<JsonModel> GetUserResponseAsync(int userId, Guid templateId);
        Task<JsonModel> GetUserResponseByIdAsync(Guid id);
        Task<JsonModel> GetUserResponsesByCategoryAsync(int userId, Guid categoryId);
        Task<JsonModel> SubmitUserResponseAsync(CreateUserResponseDto dto);
    }
} 