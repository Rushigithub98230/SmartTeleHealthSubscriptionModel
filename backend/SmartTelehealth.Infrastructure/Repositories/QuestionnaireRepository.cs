using SmartTelehealth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class QuestionnaireRepository : IQuestionnaireRepository
    {
        private readonly Data.ApplicationDbContext _context;
        public QuestionnaireRepository(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid id)
        {
            return await _context.QuestionnaireTemplates
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }

        public async Task<IEnumerable<QuestionnaireTemplate>> GetTemplatesByCategoryAsync(Guid categoryId)
        {
            return await _context.QuestionnaireTemplates
                .Where(t => t.CategoryId == categoryId && t.IsActive && !t.IsDeleted)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuestionnaireTemplate>> GetAllTemplatesAsync()
        {
            return await _context.QuestionnaireTemplates
                .Where(t => t.IsActive && !t.IsDeleted)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task AddTemplateAsync(QuestionnaireTemplate template)
        {
            // Validate template before saving
            if (string.IsNullOrWhiteSpace(template.Name))
                throw new ArgumentException("Template name is required");

            if (template.Questions == null || !template.Questions.Any())
                throw new ArgumentException("Template must have at least one question");

            // Validate question types
            foreach (var question in template.Questions)
            {
                if (!Enum.IsDefined(typeof(QuestionType), question.Type))
                    throw new ArgumentException($"Invalid question type: {question.Type}");

                // Validate range questions
                if (question.Type == QuestionType.Range)
                {
                    if (question.MinValue.HasValue && question.MaxValue.HasValue && 
                        question.MinValue >= question.MaxValue)
                    {
                        throw new ArgumentException("Range question must have MinValue < MaxValue");
                    }
                }

                // Validate multiple choice questions have options
                if (question.IsMultipleChoice && (!question.Options.Any()))
                {
                    throw new ArgumentException($"Multiple choice question '{question.Text}' must have options");
                }
            }

            _context.QuestionnaireTemplates.Add(template);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTemplateAsync(QuestionnaireTemplate template)
        {
            // Validate template before updating
            if (string.IsNullOrWhiteSpace(template.Name))
                throw new ArgumentException("Template name is required");

            var existingTemplate = await _context.QuestionnaireTemplates
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == template.Id);

            if (existingTemplate == null)
                throw new ArgumentException("Template not found");

            // Update basic properties
            existingTemplate.Name = template.Name;
            existingTemplate.Description = template.Description;
            existingTemplate.CategoryId = template.CategoryId;
            existingTemplate.IsActive = template.IsActive;
            existingTemplate.UpdatedDate = DateTime.UtcNow;

            _context.QuestionnaireTemplates.Update(existingTemplate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTemplateAsync(Guid id)
        {
            var template = await _context.QuestionnaireTemplates.FindAsync(id);
            if (template != null)
            {
                template.IsDeleted = true;
                template.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<UserResponse?> GetUserResponseAsync(int userId, Guid templateId)
        {
            return await _context.UserResponses
                .Include(r => r.Answers)
                    .ThenInclude(a => a.SelectedOptions)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.TemplateId == templateId && !r.IsDeleted);
        }

        public async Task<UserResponse?> GetUserResponseByIdAsync(Guid id)
        {
            return await _context.UserResponses
                .Include(r => r.Answers)
                    .ThenInclude(a => a.SelectedOptions)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<IEnumerable<UserResponse>> GetUserResponsesByCategoryAsync(int userId, Guid categoryId)
        {
            return await _context.UserResponses
                .Where(r => r.UserId == userId && r.CategoryId == categoryId && !r.IsDeleted)
                .Include(r => r.Answers)
                    .ThenInclude(a => a.SelectedOptions)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task AddUserResponseAsync(UserResponse response)
        {
            // Validate response before saving
            if (response.UserId <= 0)
                throw new ArgumentException("User ID is required");

            if (response.TemplateId == Guid.Empty)
                throw new ArgumentException("Template ID is required");

            if (!Enum.IsDefined(typeof(ResponseStatus), response.Status))
                throw new ArgumentException($"Invalid response status: {response.Status}");

            // Validate answers
            if (response.Answers != null)
            {
                foreach (var answer in response.Answers)
                {
                    if (answer.QuestionId == Guid.Empty)
                        throw new ArgumentException("Question ID is required for each answer");

                    // Validate that the question exists and is part of the template
                    var question = await _context.Questions
                        .FirstOrDefaultAsync(q => q.Id == answer.QuestionId && q.TemplateId == response.TemplateId);

                    if (question == null)
                        throw new ArgumentException($"Question with ID {answer.QuestionId} not found in template");

                    // Validate answer based on question type
                    if (question.IsMultipleChoice)
                    {
                        if (answer.SelectedOptions == null || !answer.SelectedOptions.Any())
                            throw new ArgumentException($"Multiple choice question requires selected options");

                        // Validate that selected options exist for this question
                        var validOptionIds = await _context.QuestionOptions
                            .Where(o => o.QuestionId == answer.QuestionId)
                            .Select(o => o.Id)
                            .ToListAsync();

                        var invalidOptions = answer.SelectedOptions
                            .Where(so => !validOptionIds.Contains(so.OptionId))
                            .ToList();

                        if (invalidOptions.Any())
                            throw new ArgumentException($"Invalid option IDs selected for question {answer.QuestionId}");
                    }
                    else if (question.IsTextBased)
                    {
                        if (string.IsNullOrWhiteSpace(answer.AnswerText))
                            throw new ArgumentException($"Text-based question requires answer text");
                    }
                    else if (question.IsRange)
                    {
                        if (!answer.NumericValue.HasValue)
                            throw new ArgumentException($"Range question requires numeric value");
                    }
                    else if (question.IsDateTimeBased)
                    {
                        if (!answer.DateTimeValue.HasValue)
                            throw new ArgumentException($"DateTime question requires date/time value");
                    }
                }
            }

            _context.UserResponses.Add(response);
            await _context.SaveChangesAsync();
        }
    }
} 