import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-subscription-success',
  template: `
    <div class="success-container">
      <div class="success-card">
        <div class="success-icon">✅</div>
        <h1>Subscription Successful!</h1>
        <p>Thank you for subscribing to our service. Your subscription is now active.</p>
        <div class="session-info" *ngIf="sessionId">
          <p><strong>Session ID:</strong> {{ sessionId }}</p>
        </div>
        <button class="btn btn-primary" (click)="goToDashboard()">
          Go to Dashboard
        </button>
      </div>
    </div>
  `,
  styles: [`
    .success-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background-color: #f8f9fa;
      padding: 20px;
    }
    
    .success-card {
      background: white;
      border-radius: 12px;
      padding: 40px;
      text-align: center;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      max-width: 500px;
      width: 100%;
    }
    
    .success-icon {
      font-size: 64px;
      margin-bottom: 20px;
    }
    
    h1 {
      color: #059669;
      margin-bottom: 16px;
      font-size: 28px;
    }
    
    p {
      color: #6b7280;
      margin-bottom: 24px;
      font-size: 16px;
    }
    
    .session-info {
      background: #f3f4f6;
      padding: 12px;
      border-radius: 6px;
      margin-bottom: 24px;
      font-size: 14px;
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
  `]
})
export class SubscriptionSuccessComponent implements OnInit {
  sessionId: string | null = null;

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.sessionId = this.route.snapshot.queryParamMap.get('session_id');
  }

  goToDashboard(): void {
    // Navigate to dashboard or home page
    window.location.href = '/';
  }
}
