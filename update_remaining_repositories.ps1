# PowerShell script to update remaining repositories with custom DeleteAsync methods

$repositories = @(
    "backend/SmartTelehealth.Infrastructure/Repositories/MessageReactionRepository.cs",
    "backend/SmartTelehealth.Infrastructure/Repositories/AppointmentPaymentLogRepository.cs",
    "backend/SmartTelehealth.Infrastructure/Repositories/ProviderPayoutRepository.cs",
    "backend/SmartTelehealth.Infrastructure/Repositories/ChatSessionRepository.cs",
    "backend/SmartTelehealth.Infrastructure/Repositories/MedicationShipmentRepository.cs",
    "backend/SmartTelehealth.Infrastructure/Repositories/QuestionnaireRepository.cs",
    "backend/SmartTelehealth.Infrastructure/Repositories/AuditLogRepository.cs",
    "backend/SmartTelehealth.Infrastructure/Repositories/AppointmentInvitationRepository.cs",
    "backend/SmartTelehealth.Infrastructure/Repositories/AppointmentParticipantRepository.cs",
    "backend/SmartTelehealth.Infrastructure/Repositories/ParticipantRoleRepository.cs",
    "backend/SmartTelehealth.Infrastructure/Repositories/MedicationDeliveryRepository.cs",
    "backend/SmartTelehealth.Infrastructure/Repositories/HealthAssessmentRepository.cs",
    "backend/SmartTelehealth.Infrastructure/Repositories/PrivilegeUsageHistoryRepository.cs"
)

foreach ($repo in $repositories) {
    Write-Host "Updating $repo..."
    
    # Read the file content
    $content = Get-Content $repo -Raw
    
    # Pattern to match DeleteAsync method implementations
    $pattern = '(?s)public.*DeleteAsync.*?\{.*?\}'
    
    # Replace with comment
    $replacement = "    // Note: DeleteAsync is inherited from RepositoryBase<T>`n    // Service layer should handle audit properties and use UpdateAsync for soft deletes"
    
    $newContent = $content -replace $pattern, $replacement
    
    # Write back to file
    Set-Content $repo -Value $newContent -NoNewline
    
    Write-Host "Updated $repo"
}

Write-Host "All repositories updated!"

