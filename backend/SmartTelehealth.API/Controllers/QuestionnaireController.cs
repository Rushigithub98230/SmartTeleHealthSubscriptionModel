using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace SmartTelehealth.API.Controllers
{
    /// <summary>
    /// Controller responsible for comprehensive questionnaire management and user response processing.
    /// This controller provides extensive functionality for managing questionnaire templates, user responses,
    /// file attachments, analytics, and reporting. It handles the complete questionnaire lifecycle from
    /// template creation to response analysis and data export for healthcare assessment and data collection.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionnaireController : BaseController
    {
        private readonly IQuestionnaireService _questionnaireService;
        private readonly IFileStorageService _fileStorageService;

        /// <summary>
        /// Initializes a new instance of the QuestionnaireController with required services.
        /// </summary>
        /// <param name="questionnaireService">Service for handling questionnaire-related business logic</param>
        /// <param name="fileStorageService">Service for handling file storage operations</param>
        public QuestionnaireController(IQuestionnaireService questionnaireService, IFileStorageService fileStorageService)
        {
            _questionnaireService = questionnaireService;
            _fileStorageService = fileStorageService;
        }

        // Template Management

        /// <summary>
        /// Retrieves all questionnaire templates available in the system.
        /// This endpoint provides a comprehensive list of all questionnaire templates including
        /// template details, categories, and configuration information for template selection.
        /// </summary>
        /// <returns>JsonModel containing all questionnaire templates</returns>
        /// <remarks>
        /// This endpoint:
        /// - Returns all questionnaire templates in the system
        /// - Includes template details, categories, and configuration
        /// - Shows template status and availability information
        /// - Access restricted to authenticated users
        /// - Used for template selection and questionnaire management
        /// - Includes comprehensive template information and metadata
        /// - Provides data for questionnaire template browsing
        /// - Handles template data retrieval and error responses
        /// </remarks>
        [HttpGet("templates")]
        public async Task<JsonModel> GetAllTemplates()
        {
            return await _questionnaireService.GetAllTemplatesAsync();
        }

        /// <summary>
        /// Retrieves detailed information about a specific questionnaire template by its ID.
        /// This endpoint provides comprehensive template details including questions, configuration,
        /// and template metadata for detailed template management and usage.
        /// </summary>
        /// <param name="id">The unique identifier of the questionnaire template</param>
        /// <returns>JsonModel containing the template details</returns>
        /// <remarks>
        /// This endpoint:
        /// - Returns detailed questionnaire template information by ID
        /// - Includes template questions, configuration, and metadata
        /// - Shows template structure and usage information
        /// - Access restricted to authenticated users
        /// - Used for template details and questionnaire creation
        /// - Includes comprehensive template information and metadata
        /// - Provides secure access to template information
        /// - Handles authorization validation and error responses
        /// </remarks>
        [HttpGet("templates/{id}")]
        public async Task<JsonModel> GetTemplateById(Guid id)
        {
            return await _questionnaireService.GetTemplateByIdAsync(id);
        }

        /// <summary>
        /// Retrieves questionnaire templates filtered by a specific category.
        /// This endpoint provides a list of questionnaire templates within a specific category
        /// including template details and category-specific information for targeted template selection.
        /// </summary>
        /// <param name="categoryId">The unique identifier of the category</param>
        /// <returns>JsonModel containing templates for the specified category</returns>
        /// <remarks>
        /// This endpoint:
        /// - Returns questionnaire templates for the specified category
        /// - Includes template details and category-specific information
        /// - Shows template availability and configuration
        /// - Access restricted to authenticated users
        /// - Used for category-based template selection
        /// - Includes comprehensive template information and metadata
        /// - Provides data for category-specific questionnaire management
        /// - Handles category template retrieval and error responses
        /// </remarks>
        [HttpGet("templates/by-category/{categoryId}")]
        public async Task<JsonModel> GetTemplatesByCategory(Guid categoryId)
        {
            return await _questionnaireService.GetTemplatesByCategoryAsync(categoryId);
        }

        /// <summary>
        /// Creates a new questionnaire template with specified configuration.
        /// This endpoint handles questionnaire template creation including question setup,
        /// configuration, and initial template setup for questionnaire management.
        /// </summary>
        /// <param name="dto">DTO containing questionnaire template creation details</param>
        /// <returns>JsonModel containing the created template information</returns>
        /// <remarks>
        /// This endpoint:
        /// - Creates a new questionnaire template with configuration
        /// - Validates template structure and question setup
        /// - Sets up template for questionnaire management
        /// - Access restricted to authenticated users
        /// - Used for questionnaire template creation and management
        /// - Includes comprehensive validation and error handling
        /// - Provides detailed feedback on template creation
        /// - Maintains template creation audit trails
        /// </remarks>
        [HttpPost("templates")]
        public async Task<JsonModel> CreateTemplate([FromBody] CreateQuestionnaireTemplateDto dto)
        {
            return await _questionnaireService.CreateTemplateAsync(dto, new List<IFormFile>(),  GetToken(HttpContext));
        }

        [HttpPost("templates/with-files")]
        public async Task<JsonModel> CreateTemplateWithFiles([FromForm] string templateJson, [FromForm] List<IFormFile> files)
        {
            if (string.IsNullOrWhiteSpace(templateJson))
                return new JsonModel { data = new object(), Message = "Template JSON is required", StatusCode = 400 };

            try
            {
                var dto = JsonConvert.DeserializeObject<CreateQuestionnaireTemplateDto>(templateJson);
                if (dto == null)
                    return new JsonModel { data = new object(), Message = "Invalid template JSON", StatusCode = 400 };

                var tokenModel = GetToken(HttpContext);
                return await _questionnaireService.CreateTemplateAsync(dto, files, tokenModel);
            }
            catch (JsonException)
            {
                return new JsonModel { data = new object(), Message = "Invalid template JSON format", StatusCode = 400 };
            }
        }

        [HttpPut("templates/{id}")]
        public async Task<JsonModel> UpdateTemplate(Guid id, [FromBody] CreateQuestionnaireTemplateDto dto)
        {
            return await _questionnaireService.UpdateTemplateAsync(id, dto, new List<IFormFile>());
        }

        [HttpPut("templates/{id}/with-files")]
        public async Task<JsonModel> UpdateTemplateWithFiles(Guid id, [FromForm] string templateJson, [FromForm] List<IFormFile> files)
        {
            if (string.IsNullOrWhiteSpace(templateJson))
                return new JsonModel { data = new object(), Message = "Template JSON is required", StatusCode = 400 };

            try
            {
                var dto = JsonConvert.DeserializeObject<CreateQuestionnaireTemplateDto>(templateJson);
                if (dto == null)
                    return new JsonModel { data = new object(), Message = "Invalid template JSON", StatusCode = 400 };

                return await _questionnaireService.UpdateTemplateAsync(id, dto, files);
            }
            catch (JsonException)
            {
                return new JsonModel { data = new object(), Message = "Invalid template JSON format", StatusCode = 400 };
            }
        }

        [HttpDelete("templates/{id}")]
        public async Task<JsonModel> DeleteTemplate(Guid id)
        {
            return await _questionnaireService.DeleteTemplateAsync(id);
        }

        // User Response Management
        [HttpPost("responses")]
        public async Task<JsonModel> SubmitResponse([FromBody] CreateUserResponseDto dto)
        {
            if (dto == null)
                return new JsonModel { data = new object(), Message = "Invalid request data", StatusCode = 400 };
            
            
            return await _questionnaireService.SubmitUserResponseAsync(dto, GetToken(HttpContext));
        }

        [HttpGet("responses/{id}")]
        public async Task<JsonModel> GetUserResponseById(Guid id)
        {
            return await _questionnaireService.GetUserResponseByIdAsync(id);
        }

        [HttpGet("responses/user/{userId}/template/{templateId}")]
        public async Task<JsonModel> GetUserResponse(int userId, Guid templateId)
        {
            return await _questionnaireService.GetUserResponseAsync(userId, templateId);
        }

        [HttpGet("responses/user/{userId}/category/{categoryId}")]
        public async Task<JsonModel> GetUserResponsesByCategory(int userId, Guid categoryId)
        {
            return await _questionnaireService.GetUserResponsesByCategoryAsync(userId, categoryId);
        }

        // File Management
        [HttpPost("templates/{id}/files")]
        public async Task<JsonModel> UploadTemplateFiles(Guid id, [FromForm] List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return new JsonModel { data = new object(), Message = "No files provided", StatusCode = 400 };

            // TODO: Implement file upload logic in service
            return new JsonModel { data = new object(), Message = "File upload not implemented", StatusCode = 501 };
        }

        [HttpGet("templates/{id}/files")]
        public async Task<JsonModel> GetTemplateFiles(Guid id)
        {
            // TODO: Implement file retrieval logic in service
            return new JsonModel { data = new object(), Message = "File retrieval not implemented", StatusCode = 501 };
        }

        [HttpDelete("templates/{id}/files/{fileId}")]
        public async Task<JsonModel> DeleteTemplateFile(Guid id, string fileId)
        {
            // TODO: Implement file deletion logic in service
            return new JsonModel { data = new object(), Message = "File deletion not implemented", StatusCode = 501 };
        }

        // Analytics and Reporting
        [HttpGet("templates/{id}/analytics")]
        public async Task<JsonModel> GetTemplateAnalytics(Guid id)
        {
            // TODO: Implement analytics logic in service
            return new JsonModel { data = new object(), Message = "Analytics not implemented", StatusCode = 501 };
        }

        [HttpGet("responses/analytics")]
        public async Task<JsonModel> GetResponseAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            // TODO: Implement response analytics logic in service
            return new JsonModel { data = new object(), Message = "Response analytics not implemented", StatusCode = 501 };
        }

        [HttpGet("export/responses")]
        public async Task<JsonModel> ExportResponses([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string format = "csv")
        {
            // TODO: Implement export logic in service
            return new JsonModel { data = new object(), Message = "Export not implemented", StatusCode = 501 };
        }
    }
} 