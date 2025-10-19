import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../../core/services';
import { LoginDto } from '../../../../core/models';

/**
 * Admin Login Component
 * 
 * APIs Used:
 * - POST /api/Auth/login
 * 
 * Route: /webadmin/login
 * Access: Public (no authentication required)
 * Redirects: /webadmin/dashboard (on success)
 */
@Component({
  selector: 'app-admin-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './admin-login.component.html',
  styleUrls: ['./admin-login.component.scss']
})
export class AdminLoginComponent implements OnInit {
  loginForm!: FormGroup;
  loading = false;
  error: string | null = null;
  returnUrl: string = '/webadmin/dashboard';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Check if already logged in as admin
    if (this.authService.isAuthenticated() && this.authService.isAdmin()) {
      this.router.navigate(['/webadmin/dashboard']);
      return;
    }

    // Get return URL
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/webadmin/dashboard';

    // Initialize form
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  /**
   * Submit admin login
   * API: POST /api/Auth/login
   */
  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.markFormGroupTouched(this.loginForm);
      return;
    }

    this.loading = true;
    this.error = null;

    const credentials: LoginDto = this.loginForm.value;

    this.authService.login(credentials).subscribe({
      next: (response) => {
        this.loading = false;
        
        if (response.statusCode === 200) {
          // Verify admin role
          if (this.authService.isAdmin()) {
            this.router.navigate([this.returnUrl]);
          } else {
            this.error = 'Access denied. Admin privileges required.';
            this.authService.logout();
          }
        } else {
          this.error = response.message || 'Login failed';
        }
      },
      error: (error) => {
        this.loading = false;
        this.error = error.message || 'Invalid credentials';
      }
    });
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      formGroup.get(key)?.markAsTouched();
    });
  }

  hasError(field: string, error: string): boolean {
    const control = this.loginForm.get(field);
    return !!control && control.hasError(error) && control.touched;
  }
}


