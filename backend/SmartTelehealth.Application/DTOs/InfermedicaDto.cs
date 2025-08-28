namespace SmartTelehealth.Application.DTOs;

public class InfermedicaParseResponseDto
{
    public List<InfermedicaMentionDto> Mentions { get; set; } = new();
}

public class InfermedicaMentionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CommonName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ChoiceId { get; set; } = string.Empty;
}

public class InfermedicaDiagnosisRequestDto
{
    public string Sex { get; set; } = "male";
    public int Age { get; set; } = 30;
    public List<InfermedicaEvidenceDto> Evidence { get; set; } = new();
}

public class InfermedicaEvidenceDto
{
    public string Id { get; set; } = string.Empty;
    public string ChoiceId { get; set; } = string.Empty;
}

public class InfermedicaDiagnosisResponseDto
{
    public List<InfermedicaConditionDto> Conditions { get; set; } = new();
    public List<InfermedicaSpecialistDto> Specialties { get; set; } = new();
}

public class InfermedicaConditionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float Probability { get; set; }
}

public class InfermedicaSpecialistDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CommonName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class InfermedicaSuggestSpecialistResponseDto
{
    public List<InfermedicaSpecialistDto> Specialties { get; set; } = new();
} 