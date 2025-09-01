import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-admin-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    RouterModule
  ],
  template: `
    <div class="register-container">
      <div class="register-card-wrapper">
        <mat-card class="register-card">
          <div class="register-header">
            <mat-icon class="brand-icon">health_and_safety</mat-icon>
            <h1>Create Admin Account</h1>
            <p>Register for SmartTeleHealth Admin Portal</p>
          </div>

          <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="register-form">

            <div class="form-row">
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>First Name</mat-label>
                <input matInput formControlName="firstName" placeholder="Enter first name" [errorStateMatcher]="matcher">
                <mat-icon matSuffix>person</mat-icon>
                <mat-error *ngIf="registerForm.get('firstName')?.hasError('required')">First name is required</mat-error>
              </mat-form-field>
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>Last Name</mat-label>
                <input matInput formControlName="lastName" placeholder="Enter last name" [errorStateMatcher]="matcher">
                <mat-icon matSuffix>person</mat-icon>
                <mat-error *ngIf="registerForm.get('lastName')?.hasError('required')">Last name is required</mat-error>
              </mat-form-field>
            </div>

            <div class="form-row">
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>City</mat-label>
                <input matInput formControlName="city" placeholder="Enter city" [errorStateMatcher]="matcher">
                <mat-error *ngIf="registerForm.get('city')?.hasError('required')">City is required</mat-error>
              </mat-form-field>
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>State</mat-label>
                <input matInput formControlName="state" placeholder="Enter state" [errorStateMatcher]="matcher">
                <mat-error *ngIf="registerForm.get('state')?.hasError('required')">State is required</mat-error>
              </mat-form-field>
            </div>

            <div class="form-row">
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>Gender</mat-label>
                <input matInput formControlName="gender" placeholder="Enter gender" [errorStateMatcher]="matcher">
                <mat-error *ngIf="registerForm.get('gender')?.hasError('required')">Gender is required</mat-error>
              </mat-form-field>
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>Address</mat-label>
                <input matInput formControlName="address" placeholder="Enter address" [errorStateMatcher]="matcher">
                <mat-error *ngIf="registerForm.get('address')?.hasError('required')">Address is required</mat-error>
              </mat-form-field>
            </div>

            <div class="form-row">
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>Zip Code</mat-label>
                <input matInput formControlName="zipCode" placeholder="Enter zip code" [errorStateMatcher]="matcher">
                <mat-error *ngIf="registerForm.get('zipCode')?.hasError('required')">Zip code is required</mat-error>
              </mat-form-field>
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>Date of Birth</mat-label>
                <input matInput formControlName="dateOfBirth" placeholder="YYYY-MM-DD" [errorStateMatcher]="matcher" type="date">
                <mat-error *ngIf="registerForm.get('dateOfBirth')?.hasError('required')">Date of birth is required</mat-error>
              </mat-form-field>
            </div>

            <div class="form-row">
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>Phone Number</mat-label>
                <input matInput formControlName="phoneNumber" placeholder="Enter phone number" [errorStateMatcher]="matcher">
                <mat-error *ngIf="registerForm.get('phoneNumber')?.hasError('required')">Phone number is required</mat-error>
              </mat-form-field>
            </div>

            <mat-form-field appearance="outline" class="form-field">
              <mat-label>Email Address</mat-label>
              <input matInput type="email" formControlName="email" placeholder="admin@smarttelehealth.com" [errorStateMatcher]="matcher">
              <mat-icon matSuffix>email</mat-icon>
              <mat-error *ngIf="registerForm.get('email')?.hasError('required')">Email is required</mat-error>
              <mat-error *ngIf="registerForm.get('email')?.hasError('email')">Please enter a valid email address</mat-error>
            </mat-form-field>

            <div class="form-row">
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>Password</mat-label>
                <input matInput [type]="showPassword ? 'text' : 'password'" formControlName="password" placeholder="Enter password" [errorStateMatcher]="matcher">
                <button mat-icon-button matSuffix type="button" (click)="togglePasswordVisibility()" [attr.aria-label]="showPassword ? 'Hide password' : 'Show password'">
                  <mat-icon>{{ showPassword ? 'visibility_off' : 'visibility' }}</mat-icon>
                </button>
                <mat-error *ngIf="registerForm.get('password')?.hasError('required')">Password is required</mat-error>
                <mat-error *ngIf="registerForm.get('password')?.hasError('minlength')">Password must be at least 8 characters</mat-error>
              </mat-form-field>
              <mat-form-field appearance="outline" class="form-field">
                <mat-label>Confirm Password</mat-label>
                <input matInput [type]="showConfirmPassword ? 'text' : 'password'" formControlName="confirmPassword" placeholder="Confirm password" [errorStateMatcher]="matcher">
                <button mat-icon-button matSuffix type="button" (click)="toggleConfirmPasswordVisibility()" [attr.aria-label]="showConfirmPassword ? 'Hide password' : 'Show password'">
                  <mat-icon>{{ showConfirmPassword ? 'visibility_off' : 'visibility' }}</mat-icon>
                </button>
                <mat-error *ngIf="registerForm.get('confirmPassword')?.hasError('required')">Please confirm your password</mat-error>
                <mat-error *ngIf="registerForm.get('confirmPassword')?.hasError('passwordMismatch')">Passwords do not match</mat-error>
              </mat-form-field>
            </div>

            <div class="form-actions">
              <button 
                mat-raised-button 
                color="primary" 
                type="submit" 
                class="register-button"
                [disabled]="registerForm.invalid || loading">
                <mat-spinner *ngIf="loading" diameter="20" class="spinner"></mat-spinner>
                <span *ngIf="!loading">Create Account</span>
              </button>
            </div>

            <div class="form-footer">
              <p>Already have an account? 
                <a routerLink="/admin/login" class="login-link">Sign in here</a>
              </p>
            </div>
          </form>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .register-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }

    .register-card-wrapper {
      width: 100%;
      max-width: 500px;
    }

    .register-card {
      padding: 40px;
      border-radius: 16px;
      box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
    }

    .register-header {
      text-align: center;
      margin-bottom: 32px;
    }

    .brand-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      color: #667eea;
      margin-bottom: 16px;
    }

    .register-header h1 {
      margin: 0 0 8px 0;
      font-size: 28px;
      font-weight: 700;
      color: #333;
    }

    .register-header p {
      margin: 0;
      color: #666;
      font-size: 16px;
    }

    .register-form {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .form-field {
      width: 100%;
    }

    .form-actions {
      margin-top: 8px;
    }

    .register-button {
      width: 100%;
      height: 48px;
      font-size: 16px;
      font-weight: 600;
      border-radius: 8px;
      position: relative;
    }

    .spinner {
      margin-right: 8px;
    }

    .form-footer {
      text-align: center;
      margin-top: 24px;
    }

    .form-footer p {
      margin: 0;
      color: #666;
      font-size: 14px;
    }

    .login-link {
      color: #667eea;
      text-decoration: none;
      font-weight: 600;
    }

    .login-link:hover {
      text-decoration: underline;
    }

    /* Error state styling */
    .mat-form-field.mat-form-field-invalid .mat-form-field-outline {
      color: #f44336;
    }

    .mat-form-field.mat-form-field-invalid .mat-form-field-label {
      color: #f44336;
    }

    /* Responsive design */
    @media (max-width: 600px) {
      .form-row {
        grid-template-columns: 1fr;
      }
    }

    @media (max-width: 480px) {
      .register-card {
        padding: 24px;
      }

      .register-header h1 {
        font-size: 24px;
      }

      .brand-icon {
        font-size: 48px;
        width: 48px;
        height: 48px;
      }
    }
  `]
})
export class AdminRegisterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  registerForm: FormGroup;
  loading = false;
  showPassword = false;
  showConfirmPassword = false;

  constructor() {
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      city: ['', [Validators.required]],
      state: ['', [Validators.required]],
      gender: ['', [Validators.required]],
      address: ['', [Validators.required]],
      zipCode: ['', [Validators.required]],
      dateOfBirth: ['', [Validators.required]],
      phoneNumber: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit() {
    // Check if user is already authenticated
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/admin/dashboard']);
    }
  }

  onSubmit() {
    if (this.registerForm.invalid) return;

    this.loading = true;
    const userData = {
      firstName: this.registerForm.value.firstName,
      lastName: this.registerForm.value.lastName,
      email: this.registerForm.value.email,
      password: this.registerForm.value.password,
      confirmPassword: this.registerForm.value.confirmPassword,
      city: this.registerForm.value.city,
      state: this.registerForm.value.state,
      gender: this.registerForm.value.gender,
      address: this.registerForm.value.address,
      zipCode: this.registerForm.value.zipCode,
      dateOfBirth: this.registerForm.value.dateOfBirth,
      phoneNumber: this.registerForm.value.phoneNumber,
      role: 'Admin'
    };

    this.authService.register(userData).subscribe({
      next: (response) => {
        this.loading = false;
        if (response.statusCode === 200 || response.statusCode === 201) {
          this.snackBar.open('Registration successful! Please sign in.', 'Close', { duration: 5000 });
          this.router.navigate(['/admin/login']);
        } else {
          this.snackBar.open(response.message || 'Registration failed', 'Close', { duration: 3000 });
        }
      },
      error: (error) => {
        this.loading = false;
        const errorMessage = error?.error?.message || 'Registration failed. Please try again.';
        this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
      }
    });
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility() {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  // Custom password match validator
  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }
    
    return null;
  }

  // Custom error state matcher for better UX
  matcher = {
    isErrorState: (control: any) => {
      return control && control.invalid && (control.dirty || control.touched);
    }
  };
}
