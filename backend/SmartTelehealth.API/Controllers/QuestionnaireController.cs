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
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionnaireController : BaseController
    {
        private readonly IQuestionnaireService _questionnaireService;
        private readonly IFileStorageService _fileStorageService;

        public QuestionnaireController(IQuestionnaireService questionnaireService, IFileStorageService fileStorageService)
        {
            _questionnaireService = questionnaireService;
            _fileStorageService = fileStorageService;
        }

        // Template Management
        [HttpGet("templates")]
        public async Task<JsonModel> GetAllTemplates()
        {
            return await _questionnaireService.GetAllTemplatesAsync();
        }

        [HttpGet("templates/{id}")]
        public async Task<JsonModel> GetTemplateById(Guid id)
        {
            return await _questionnaireService.GetTemplateByIdAsync(id);
        }

        [HttpGet("templates/by-category/{categoryId}")]
        public async Task<JsonModel> GetTemplatesByCategory(Guid categoryId)
        {
            return await _questionnaireService.GetTemplatesByCategoryAsync(categoryId);
        }

        [HttpPost("templates")]
        public async Task<JsonModel> CreateTemplate([FromBody] CreateQuestionnaireTemplateDto dto)
        {
            return await _questionnaireService.CreateTemplateAsync(dto, new List<IFormFile>());
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

                return await _questionnaireService.CreateTemplateAsync(dto, files);
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
            
            return await _questionnaireService.SubmitUserResponseAsync(dto);
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