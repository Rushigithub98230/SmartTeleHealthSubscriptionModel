namespace SmartTelehealth.Core.Entities
{
    public enum QuestionType
    {
        Text = 1,
        TextArea = 2,
        Radio = 3,
        Checkbox = 4,
        Dropdown = 5,
        Range = 6,
        Date = 7,
        DateTime = 8,
        Time = 9
    }

    public enum ResponseStatus
    {
        Draft = 1,
        InProgress = 2,
        Completed = 3,
        Submitted = 4,
        Reviewed = 5,
        Approved = 6,
        Rejected = 7
    }
} 