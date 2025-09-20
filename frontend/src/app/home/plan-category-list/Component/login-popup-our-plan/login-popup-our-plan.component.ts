import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PlanCategoryAuthService } from '../../plan-category-auth.service';

@Component({
  selector: "app-login-popup-our-plan",
  standalone: true,
  imports: [CommonModule, FormsModule],

  template: `
    <div class="popup-overlay" (click)="onOverlayClick($event)">
      <div class="popup-content" (click)="$event.stopPropagation()">
        <div class="popup-header">
          <h2>Login to Continue</h2>
          <button class="close-btn" (click)="onClose()">&times;</button>
        </div>

        <div class="popup-body">
          <p class="login-message">
            Please log in to proceed with your plan selection and payment.
          </p>

          <form (ngSubmit)="onLogin()" #loginForm="ngForm">
            <div class="form-group">
              <label for="email">Email Address</label>
              <input
                type="email"
                id="email"
                name="email"
                [(ngModel)]="email"
                required
                class="form-input"
                placeholder="Enter your email"
              />
            </div>

            <div class="form-group">
              <label for="password">Password</label>
              <input
                type="password"
                id="password"
                name="password"
                [(ngModel)]="password"
                required
                class="form-input"
                placeholder="Enter your password"
              />
            </div>

            <div class="error-message" *ngIf="errorMessage">
              {{ errorMessage }}
            </div>

            <button
              type="submit"
              class="btn btn-primary btn-full"
              [disabled]="isLoading || !loginForm.valid"
            >
              <span *ngIf="isLoading">Logging in...</span>
              <span *ngIf="!isLoading">Login</span>
            </button>
          </form>

          <div class="demo-credentials">
            <p><strong>Demo Credentials:</strong></p>
            <p>Email: demo&#64;example.com</p>
            <p>Password: any password</p>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .popup-overlay {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 1000;
      }

      .popup-content {
        background: white;
        border-radius: 12px;
        width: 90%;
        max-width: 400px;
        box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
      }

      .popup-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 24px;
        border-bottom: 1px solid #e5e7eb;
      }

      .popup-header h2 {
        margin: 0;
        color: #1f2937;
        font-size: 20px;
        font-weight: 600;
      }

      .close-btn {
        background: none;
        border: none;
        font-size: 24px;
        cursor: pointer;
        color: #6b7280;
        padding: 0;
        width: 32px;
        height: 32px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 50%;
        transition: all 0.2s;
      }

      .close-btn:hover {
        background: #f3f4f6;
        color: #374151;
      }

      .popup-body {
        padding: 24px;
      }

      .login-message {
        /* color: #6b7280; */
        margin-bottom: 24px;
        text-align: center;
      }

      .form-group {
        margin-bottom: 20px;
      }

      .form-group label {
        display: block;
        margin-bottom: 8px;
        color: #374151;
        font-weight: 500;
      }

      .form-input {
        width: 100%;
        padding: 12px;
        border: 2px solid #e5e7eb;
        border-radius: 8px;
        font-size: 16px;
        transition: border-color 0.2s;
        font-family: inherit;
      }

      .form-input:focus {
        outline: none;
        border-color: #3b82f6;
      }

      .error-message {
        color: #ef4444;
        font-size: 14px;
        margin-bottom: 16px;
        text-align: center;
      }

      .btn {
        padding: 12px 24px;
        border: none;
        border-radius: 8px;
        font-size: 16px;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.2s;
        font-family: inherit;
      }

      .btn-full {
        width: 100%;
      }

      .btn:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }

      .btn-primary {
        background: #322e9f;
        color: white;
      }

      .btn-primary:hover:not(:disabled) {
        background: #322e9f;
      }

      .demo-credentials {
        margin-top: 24px;
        padding: 16px;
        background: #f3f4f6;
        border-radius: 8px;
        font-size: 14px;
      }

      .demo-credentials p {
        margin: 4px 0;
        color: #6b7280;
      }

      .demo-credentials strong {
        color: #374151;
      }

      @media (max-width: 768px) {
        .popup-content {
          width: 95%;
          margin: 20px;
        }

        .popup-header,
        .popup-body {
          padding: 16px;
        }
      }
    `,
  ],
})
export class LoginPopupOurPlanComponent {
  @Output() close = new EventEmitter<void>();
  @Output() loginSuccess = new EventEmitter<void>();

  email = "";
  password = "";
  isLoading = false;
  errorMessage = "";

  constructor(private authService: PlanCategoryAuthService) {}

  onLogin(): void {
    this.isLoading = true;
    this.errorMessage = "";

    this.authService.login(this.email, this.password).subscribe({
      next: (success) => {
        this.isLoading = false;
        if (success) {
          this.loginSuccess.emit();
        } else {
          this.errorMessage = "Invalid email or password";
        }
      },
      error: (error) => {
        this.isLoading = false;
        console.error('Login error:', error);
        this.errorMessage = error?.error?.message || "Login failed. Please try again.";
      }
    });
  }

  onClose(): void {
    this.close.emit();
  }

  onOverlayClick(event: Event): void {
    if (event.target === event.currentTarget) {
      this.onClose();
    }
  }
}
