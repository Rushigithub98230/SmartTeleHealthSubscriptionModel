using SmartTelehealth.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTelehealth.Application.Interfaces
{
    public interface IQuestionnaireRepository
    {
        Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid id);
        Task<IEnumerable<QuestionnaireTemplate>> GetTemplatesByCategoryAsync(Guid categoryId);
        Task<IEnumerable<QuestionnaireTemplate>> GetAllTemplatesAsync();
        Task AddTemplateAsync(QuestionnaireTemplate template);
        Task UpdateTemplateAsync(QuestionnaireTemplate template);
        Task DeleteTemplateAsync(Guid id);
        Task<UserResponse?> GetUserResponseAsync(int userId, Guid templateId);
        Task<UserResponse?> GetUserResponseByIdAsync(Guid id);
        Task<IEnumerable<UserResponse>> GetUserResponsesByCategoryAsync(int userId, Guid categoryId);
        Task AddUserResponseAsync(UserResponse response);
    }
} 