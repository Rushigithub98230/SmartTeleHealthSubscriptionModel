import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../../core/services';
import { LoginDto } from '../../../../core/models';

/**
 * User Login Component
 * 
 * APIs Used:
 * - POST /api/Auth/login
 * 
 * Route: /web/login
 * Access: Public (no authentication required)
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  loading = false;
  error: string | null = null;
  returnUrl: string = '/web/dashboard';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Check if already logged in
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/web/dashboard']);
      return;
    }

    // Get return URL from route params
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/web/dashboard';

    // Initialize form
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  /**
   * Submit login form
   * API: POST /api/Auth/login
   */
  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.markFormGroupTouched(this.loginForm);
      return;
    }

    this.loading = true;
    this.error = null;

    const credentials: LoginDto = {
      email: this.loginForm.value.email,
      password: this.loginForm.value.password
    };

    this.authService.login(credentials).subscribe({
      next: (response) => {
        this.loading = false;
        
        if (response.statusCode === 200) {
          // Success - navigate to return URL
          this.router.navigate([this.returnUrl]);
        } else {
          this.error = response.message || 'Login failed';
        }
      },
      error: (error) => {
        this.loading = false;
        this.error = error.message || 'Invalid email or password';
      }
    });
  }

  /**
   * Helper to mark all fields as touched (for validation display)
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
    const control = this.loginForm.get(field);
    return !!control && control.hasError(error) && control.touched;
  }
}


