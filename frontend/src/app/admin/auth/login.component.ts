import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
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
  selector: 'app-admin-login',
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
    <div class="login-container">
      <div class="login-card-wrapper">
        <mat-card class="login-card">
          <div class="login-header">
            <mat-icon class="brand-icon">health_and_safety</mat-icon>
            <h1>SmartTeleHealth Admin</h1>
            <p>Sign in to access the admin portal</p>
          </div>

          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="login-form">
            <mat-form-field appearance="outline" class="form-field">
              <mat-label>Email Address</mat-label>
              <input 
                matInput 
                type="email" 
                formControlName="email" 
                placeholder="admin@smarttelehealth.com"
                [errorStateMatcher]="matcher">
              <mat-icon matSuffix>email</mat-icon>
              <mat-error *ngIf="loginForm.get('email')?.hasError('required')">
                Email is required
              </mat-error>
              <mat-error *ngIf="loginForm.get('email')?.hasError('email')">
                Please enter a valid email address
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="form-field">
              <mat-label>Password</mat-label>
              <input 
                matInput 
                [type]="showPassword ? 'text' : 'password'" 
                formControlName="password" 
                placeholder="Enter your password"
                [errorStateMatcher]="matcher">
              <button 
                mat-icon-button 
                matSuffix 
                type="button" 
                (click)="togglePasswordVisibility()"
                [attr.aria-label]="showPassword ? 'Hide password' : 'Show password'">
                <mat-icon>{{ showPassword ? 'visibility_off' : 'visibility' }}</mat-icon>
              </button>
              <mat-error *ngIf="loginForm.get('password')?.hasError('required')">
                Password is required
              </mat-error>
            </mat-form-field>

            <div class="form-actions">
              <button 
                mat-raised-button 
                color="primary" 
                type="submit" 
                class="login-button"
                [disabled]="loginForm.invalid || loading">
                <mat-spinner *ngIf="loading" diameter="20" class="spinner"></mat-spinner>
                <span *ngIf="!loading">Sign In</span>
              </button>
            </div>

            <div class="form-footer">
              <p>Don't have an account? 
                <a routerLink="/admin/register" class="register-link">Register here</a>
              </p>
            </div>
          </form>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }

    .login-card-wrapper {
      width: 100%;
      max-width: 400px;
    }

    .login-card {
      padding: 40px;
      border-radius: 16px;
      box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
    }

    .login-header {
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

    .login-header h1 {
      margin: 0 0 8px 0;
      font-size: 28px;
      font-weight: 700;
      color: #333;
    }

    .login-header p {
      margin: 0;
      color: #666;
      font-size: 16px;
    }

    .login-form {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .form-field {
      width: 100%;
    }

    .form-actions {
      margin-top: 8px;
    }

    .login-button {
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

    .register-link {
      color: #667eea;
      text-decoration: none;
      font-weight: 600;
    }

    .register-link:hover {
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
    @media (max-width: 480px) {
      .login-card {
        padding: 24px;
      }

      .login-header h1 {
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
export class AdminLoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  loginForm: FormGroup;
  loading = false;
  showPassword = false;

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit() {
    // Check if user is already authenticated
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/admin/dashboard']);
    }
  }

  onSubmit() {
    if (this.loginForm.invalid) return;

    this.loading = true;
    this.authService.login(this.loginForm.value).subscribe({
      next: (response) => {
        this.loading = false;
        if (response.statusCode === 200) {
          this.snackBar.open('Login successful!', 'Close', { duration: 3000 });
          // Navigation is handled by the auth service
        } else {
          this.snackBar.open(response.message || 'Login failed', 'Close', { duration: 3000 });
        }
      },
      error: (error) => {
        this.loading = false;
        const errorMessage = error?.error?.message || 'Login failed. Please check your credentials.';
        this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
      }
    });
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  // Custom error state matcher for better UX
  matcher = {
    isErrorState: (control: any) => {
      return control && control.invalid && (control.dirty || control.touched);
    }
  };
}
