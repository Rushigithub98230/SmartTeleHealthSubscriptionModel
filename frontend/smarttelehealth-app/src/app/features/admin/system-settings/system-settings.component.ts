import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { SystemSettingsService, SystemSettingsDto, UpdateSystemSettingsDto } from '../../../core/services';

/**
 * Admin System Settings Component
 * Manage global system configuration values
 * 
 * APIs Used:
 * - GET /api/admin/SystemSettings
 * - PUT /api/admin/SystemSettings
 * - POST /api/admin/SystemSettings/reset
 * 
 * Route: /webadmin/system-settings
 * Access: Admin only
 */
@Component({
  selector: 'app-system-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './system-settings.component.html',
  styleUrls: ['./system-settings.component.scss']
})
export class SystemSettingsComponent implements OnInit {
  settingsForm!: FormGroup;
  systemSettings: SystemSettingsDto | null = null;
  loading = false;
  saving = false;
  error: string | null = null;
  success: string | null = null;

  constructor(
    private fb: FormBuilder,
    private systemSettingsService: SystemSettingsService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadSettings();
  }

  /**
   * Initialize the form
   */
  initForm(): void {
    this.settingsForm = this.fb.group({
      defaultAdminCommissionPercent: [20, [Validators.required, Validators.min(0), Validators.max(100)]],
      defaultPriceChangeNoticeDays: [10, [Validators.required, Validators.min(1), Validators.max(90)]],
      maxFailedPaymentAttempts: [3, [Validators.required, Validators.min(1), Validators.max(10)]]
    });
  }

  /**
   * Load current system settings
   * API: GET /api/admin/SystemSettings
   */
  loadSettings(): void {
    this.loading = true;
    this.error = null;

    this.systemSettingsService.getSettings().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.systemSettings = response.data;
          this.populateForm();
        } else {
          this.error = response.message || 'Failed to load system settings';
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to load system settings';
        this.loading = false;
      }
    });
  }

  /**
   * Populate form with current settings
   */
  populateForm(): void {
    if (!this.systemSettings) return;

    this.settingsForm.patchValue({
      defaultAdminCommissionPercent: this.systemSettings.defaultAdminCommissionPercent,
      defaultPriceChangeNoticeDays: this.systemSettings.defaultPriceChangeNoticeDays,
      maxFailedPaymentAttempts: this.systemSettings.maxFailedPaymentAttempts
    });
  }

  /**
   * Save system settings
   * API: PUT /api/admin/SystemSettings
   */
  saveSettings(): void {
    if (this.settingsForm.invalid) {
      this.markFormGroupTouched(this.settingsForm);
      return;
    }

    this.saving = true;
    this.error = null;
    this.success = null;

    const updateDto: UpdateSystemSettingsDto = {
      defaultAdminCommissionPercent: this.settingsForm.value.defaultAdminCommissionPercent,
      defaultPriceChangeNoticeDays: this.settingsForm.value.defaultPriceChangeNoticeDays,
      maxFailedPaymentAttempts: this.settingsForm.value.maxFailedPaymentAttempts
    };

    this.systemSettingsService.updateSettings(updateDto).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.systemSettings = response.data;
          this.success = 'System settings updated successfully!';
          setTimeout(() => this.success = null, 5000);
        } else {
          this.error = response.message || 'Failed to update system settings';
        }
        this.saving = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to update system settings';
        this.saving = false;
      }
    });
  }

  /**
   * Reset settings to defaults
   * API: POST /api/admin/SystemSettings/reset
   */
  resetToDefaults(): void {
    if (!confirm('Are you sure you want to reset all system settings to their default values?')) {
      return;
    }

    this.saving = true;
    this.error = null;
    this.success = null;

    this.systemSettingsService.resetToDefaults().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.systemSettings = response.data;
          this.populateForm();
          this.success = 'System settings reset to defaults successfully!';
          setTimeout(() => this.success = null, 5000);
        } else {
          this.error = response.message || 'Failed to reset system settings';
        }
        this.saving = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to reset system settings';
        this.saving = false;
      }
    });
  }

  /**
   * Mark all form controls as touched for validation
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      formGroup.get(key)?.markAsTouched();
    });
  }
}
