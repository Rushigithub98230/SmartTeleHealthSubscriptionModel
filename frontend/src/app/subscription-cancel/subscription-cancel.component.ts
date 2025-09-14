import { Component } from '@angular/core';

@Component({
  selector: 'app-subscription-cancel',
  template: `
    <div class="cancel-container">
      <div class="cancel-card">
        <div class="cancel-icon">❌</div>
        <h1>Subscription Cancelled</h1>
        <p>Your subscription process was cancelled. No charges have been made.</p>
        <p>You can try again anytime or contact our support team if you need assistance.</p>
        <div class="button-group">
          <button class="btn btn-primary" (click)="goBackToPlans()">
            Try Again
          </button>
          <button class="btn btn-secondary" (click)="goToHome()">
            Go to Home
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .cancel-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background-color: #f8f9fa;
      padding: 20px;
    }
    
    .cancel-card {
      background: white;
      border-radius: 12px;
      padding: 40px;
      text-align: center;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      max-width: 500px;
      width: 100%;
    }
    
    .cancel-icon {
      font-size: 64px;
      margin-bottom: 20px;
    }
    
    h1 {
      color: #dc2626;
      margin-bottom: 16px;
      font-size: 28px;
    }
    
    p {
      color: #6b7280;
      margin-bottom: 16px;
      font-size: 16px;
    }
    
    .button-group {
      display: flex;
      gap: 12px;
      justify-content: center;
      margin-top: 24px;
    }
    
    .btn {
      padding: 12px 24px;
      border: none;
      border-radius: 6px;
      font-size: 16px;
      cursor: pointer;
      transition: background-color 0.2s;
    }
    
    .btn-primary {
      background-color: #322e9f;
      color: white;
    }
    
    .btn-primary:hover {
      background-color: #2a2580;
    }
    
    .btn-secondary {
      background-color: #6b7280;
      color: white;
    }
    
    .btn-secondary:hover {
      background-color: #4b5563;
    }
  `]
})
export class SubscriptionCancelComponent {
  goBackToPlans(): void {
    // Navigate back to plans page
    window.location.href = '/';
  }

  goToHome(): void {
    // Navigate to home page
    window.location.href = '/';
  }
}
