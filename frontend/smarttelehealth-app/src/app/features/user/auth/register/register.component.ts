import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services';
import { RegisterDto } from '../../../../core/models';

/**
 * User Registration Component
 * 
 * APIs Used:
 * - POST /api/Auth/register
 * 
 * Route: /web/register
 * Access: Public (no authentication required)
 */
@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  loading = false;
  error: string | null = null;
  success = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Check if already logged in
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/web/dashboard']);
      return;
    }

    // Initialize registration form
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      dateOfBirth: ['', Validators.required],
      gender: ['', Validators.required],
      address: ['', Validators.required],
      city: ['', Validators.required],
      state: ['', Validators.required],
      zipCode: ['', [Validators.required, Validators.pattern(/^\d{5}$/)]],
      agreeToTerms: [false, Validators.requiredTrue]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  /**
   * Custom validator to check if passwords match
   */
  passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  /**
   * Submit registration form
   * API: POST /api/Auth/register
   */
  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.markFormGroupTouched(this.registerForm);
      return;
    }

    this.loading = true;
    this.error = null;

    const registerData: RegisterDto = {
      firstName: this.registerForm.value.firstName,
      lastName: this.registerForm.value.lastName,
      email: this.registerForm.value.email,
      password: this.registerForm.value.password,
      confirmPassword: this.registerForm.value.confirmPassword,
      phoneNumber: this.registerForm.value.phoneNumber,
      dateOfBirth: this.registerForm.value.dateOfBirth,
      gender: this.registerForm.value.gender,
      address: this.registerForm.value.address,
      city: this.registerForm.value.city,
      state: this.registerForm.value.state,
      zipCode: this.registerForm.value.zipCode,
      role: 'Client'
    };

    this.authService.register(registerData).subscribe({
      next: (response) => {
        this.loading = false;
        
        if (response.statusCode === 200 || response.statusCode === 201) {
          this.success = true;
          // Redirect to login after 2 seconds
          setTimeout(() => {
            this.router.navigate(['/web/login'], {
              queryParams: { registered: 'true' }
            });
          }, 2000);
        } else {
          this.error = response.message || 'Registration failed';
        }
      },
      error: (error) => {
        this.loading = false;
        this.error = error.message || 'An error occurred during registration';
      }
    });
  }

  /**
   * Helper to mark all fields as touched
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  /**
   * Check if field has error
   */
  hasError(field: string, error: string): boolean {
    const control = this.registerForm.get(field);
    return !!control && control.hasError(error) && control.touched;
  }

  /**
   * Check if passwords match error
   */
  hasPasswordMismatch(): boolean {
    return this.registerForm.hasError('passwordMismatch') && 
           this.registerForm.get('confirmPassword')?.touched || false;
  }
}


