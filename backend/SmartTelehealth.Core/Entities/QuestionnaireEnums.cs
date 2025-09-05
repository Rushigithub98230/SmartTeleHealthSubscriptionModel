namespace SmartTelehealth.Core.Entities
{
    /// <summary>
    /// Enumeration defining the possible types of questions in the system.
    /// Used for question type classification and validation.
    /// </summary>
    public enum QuestionType
    {
        /// <summary>Single-line text input question.</summary>
        Text = 1,
        /// <summary>Multi-line text input question.</summary>
        TextArea = 2,
        /// <summary>Single-choice radio button question.</summary>
        Radio = 3,
        /// <summary>Multiple-choice checkbox question.</summary>
        Checkbox = 4,
        /// <summary>Single-choice dropdown question.</summary>
        Dropdown = 5,
        /// <summary>Numeric range slider question.</summary>
        Range = 6,
        /// <summary>Date picker question.</summary>
        Date = 7,
        /// <summary>Date and time picker question.</summary>
        DateTime = 8,
        /// <summary>Time picker question.</summary>
        Time = 9
    }

    /// <summary>
    /// Enumeration defining the possible statuses of user responses in the system.
    /// Used for response status tracking and management.
    /// </summary>
    public enum ResponseStatus
    {
        /// <summary>Response is in draft status and not yet submitted.</summary>
        Draft = 1,
        /// <summary>Response is in progress and being completed.</summary>
        InProgress = 2,
        /// <summary>Response has been completed by the user.</summary>
        Completed = 3,
        /// <summary>Response has been submitted for review.</summary>
        Submitted = 4,
        /// <summary>Response has been reviewed by an administrator.</summary>
        Reviewed = 5,
        /// <summary>Response has been approved by an administrator.</summary>
        Approved = 6,
        /// <summary>Response has been rejected by an administrator.</summary>
        Rejected = 7
    }
} 