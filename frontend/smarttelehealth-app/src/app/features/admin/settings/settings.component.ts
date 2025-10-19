import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

/**
 * Admin Settings Component
 * System configuration and settings
 * 
 * Route: /webadmin/settings
 * Access: Admin only
 */
@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class AdminSettingsComponent implements OnInit {
  settingsForm!: FormGroup;
  saving = false;
  success: string | null = null;

  activeTab = 'general'; // 'general', 'billing', 'notifications'

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.initForm();
  }

  initForm(): void {
    this.settingsForm = this.fb.group({
      siteName: ['SmartTelehealth', Validators.required],
      supportEmail: ['support@smarttelehealth.com', [Validators.required, Validators.email]],
      defaultCurrency: ['USD', Validators.required],
      defaultBillingCycle: ['Monthly', Validators.required],
      enableTrials: [true],
      defaultTrialDays: [7, [Validators.min(0), Validators.max(30)]],
      enableAutoRenew: [true],
      gracePeriodDays: [3, [Validators.min(0), Validators.max(30)]],
      enableNotifications: [true],
      enableEmailNotifications: [true],
      enableSMSNotifications: [false]
    });
  }

  saveSettings(): void {
    if (this.settingsForm.invalid) return;

    this.saving = true;
    
    // Simulate save
    setTimeout(() => {
      this.saving = false;
      this.success = 'Settings saved successfully!';
      setTimeout(() => this.success = null, 3000);
    }, 1000);

    // Real implementation:
    // this.commonService.put('Admin/Settings', this.settingsForm.value).subscribe(...)
  }

  switchTab(tab: string): void {
    this.activeTab = tab;
  }
}


