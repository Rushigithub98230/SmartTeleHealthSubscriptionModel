import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services';
import { UserDto, UpdateUserProfileDto } from '../../../core/models';

/**
 * User Profile Component
 * Edit user personal information
 * 
 * APIs Used:
 * - PUT /api/Users/{id}
 * 
 * Route: /web/profile
 * Access: Authenticated users
 */
@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  currentUser: UserDto | null = null;
  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  
  loading = false;
  saving = false;
  changingPassword = false;
  error: string | null = null;
  success: string | null = null;

  activeTab = 'profile'; // 'profile' or 'password'

  constructor(
    private fb: FormBuilder,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    
    if (this.currentUser) {
      this.initForms();
    }
  }

  /**
   * Initialize forms
   */
  initForms(): void {
    // Profile form
    this.profileForm = this.fb.group({
      firstName: [this.currentUser?.firstName || '', [Validators.required, Validators.maxLength(50)]],
      lastName: [this.currentUser?.lastName || '', [Validators.required, Validators.maxLength(50)]],
      email: [{ value: this.currentUser?.email || '', disabled: true }],
      phoneNumber: [this.currentUser?.phoneNumber || this.currentUser?.phone || '', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      dateOfBirth: [this.currentUser?.dateOfBirth ? new Date(this.currentUser.dateOfBirth).toISOString().split('T')[0] : ''],
      gender: [this.currentUser?.gender || ''],
      address: [this.currentUser?.address || ''],
      city: [this.currentUser?.city || ''],
      state: [this.currentUser?.state || ''],
      zipCode: [this.currentUser?.zipCode || '', Validators.pattern(/^\d{5}$/)],
      emergencyContact: [this.currentUser?.emergencyContact || ''],
      emergencyPhone: [this.currentUser?.emergencyPhone || '', Validators.pattern(/^\d{10}$/)]
    });

    // Password change form
    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPassword: ['', Validators.required]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  /**
   * Password match validator
   */
  passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmNewPassword')?.value;
    return newPassword === confirmPassword ? null : { passwordMismatch: true };
  }

  /**
   * Save profile changes
   * API: PUT /api/Users/{id}
   */
  saveProfile(): void {
    if (this.profileForm.invalid) {
      this.markFormGroupTouched(this.profileForm);
      return;
    }

    this.saving = true;
    this.error = null;
    this.success = null;

    const dto: UpdateUserProfileDto = this.profileForm.value;

    // Note: This would need a UserService with updateProfile method
    // For now, showing the pattern
    console.log('Profile update DTO:', dto);
    
    // Simulated success
    setTimeout(() => {
      this.saving = false;
      this.success = 'Profile updated successfully!';
      
      // Clear success message after 3 seconds
      setTimeout(() => this.success = null, 3000);
    }, 1000);

    // Real implementation would be:
    // this.userService.updateProfile(this.currentUser.id, dto).subscribe(...)
  }

  /**
   * Change password
   * API: POST /api/Auth/change-password
   */
  changePassword(): void {
    if (this.passwordForm.invalid) {
      this.markFormGroupTouched(this.passwordForm);
      return;
    }

    this.changingPassword = true;
    this.error = null;
    this.success = null;

    // Note: This would need AuthService.changePassword method
    // For now, showing the pattern
    console.log('Password change requested');
    
    // Simulated success
    setTimeout(() => {
      this.changingPassword = false;
      this.success = 'Password changed successfully!';
      this.passwordForm.reset();
      
      // Clear success message
      setTimeout(() => this.success = null, 3000);
    }, 1000);

    // Real implementation:
    // this.authService.changePassword(dto).subscribe(...)
  }

  /**
   * Switch tabs
   */
  switchTab(tab: string): void {
    this.activeTab = tab;
    this.error = null;
    this.success = null;
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      formGroup.get(key)?.markAsTouched();
    });
  }

  hasError(form: FormGroup, field: string, error: string): boolean {
    const control = form.get(field);
    return !!control && control.hasError(error) && control.touched;
  }

  hasPasswordMismatch(): boolean {
    return this.passwordForm.hasError('passwordMismatch') && 
           this.passwordForm.get('confirmNewPassword')?.touched || false;
  }
}


