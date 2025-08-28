namespace SmartTelehealth.Application.DTOs;

public class AssessmentTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public IEnumerable<AssessmentQuestionDto> Questions { get; set; } = new List<AssessmentQuestionDto>();
}

public class CreateAssessmentTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public IEnumerable<CreateAssessmentQuestionDto> Questions { get; set; } = new List<CreateAssessmentQuestionDto>();
}

public class UpdateAssessmentTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IEnumerable<UpdateAssessmentQuestionDto> Questions { get; set; } = new List<UpdateAssessmentQuestionDto>();
}

public class AssessmentQuestionDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty; // Text, MultipleChoice, YesNo, Scale
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public IEnumerable<AssessmentQuestionOptionDto> Options { get; set; } = new List<AssessmentQuestionOptionDto>();
}

public class CreateAssessmentQuestionDto
{
    public string Question { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public IEnumerable<CreateAssessmentQuestionOptionDto> Options { get; set; } = new List<CreateAssessmentQuestionOptionDto>();
}

public class UpdateAssessmentQuestionDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public IEnumerable<UpdateAssessmentQuestionOptionDto> Options { get; set; } = new List<UpdateAssessmentQuestionOptionDto>();
}

public class AssessmentQuestionOptionDto
{
    public Guid Id { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public string OptionValue { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class CreateAssessmentQuestionOptionDto
{
    public string OptionText { get; set; } = string.Empty;
    public string OptionValue { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class UpdateAssessmentQuestionOptionDto
{
    public Guid Id { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public string OptionValue { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class AssessmentReportDto
{
    public Guid Id { get; set; }
    public Guid AssessmentId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsEligibleForTreatment { get; set; }
    public string ProviderNotes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public IEnumerable<AssessmentResponseDto> Responses { get; set; } = new List<AssessmentResponseDto>();
}

public class AssessmentResponseDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public DateTime AnsweredAt { get; set; }
} 