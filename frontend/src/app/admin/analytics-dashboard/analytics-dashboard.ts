import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatGridListModule } from '@angular/material/grid-list';
import { Router } from '@angular/router';

@Component({
  selector: 'app-analytics-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatGridListModule
  ],
  template: `
    <div class="dashboard-container">
      <mat-card class="welcome-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>dashboard</mat-icon>
            Welcome to Admin Portal
          </mat-card-title>
          <mat-card-subtitle>
            Manage your telehealth subscription system
          </mat-card-subtitle>
        </mat-card-header>
      </mat-card>

      <div class="stats-grid">
        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <mat-icon class="stat-icon">subscriptions</mat-icon>
              <div class="stat-details">
                <h3>Total Subscriptions</h3>
                <p class="stat-number">1,247</p>
                <p class="stat-change positive">+12% from last month</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <mat-icon class="stat-icon">people</mat-icon>
              <div class="stat-details">
                <h3>Active Users</h3>
                <p class="stat-number">892</p>
                <p class="stat-change positive">+8% from last month</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <mat-icon class="stat-icon">attach_money</mat-icon>
              <div class="stat-details">
                <h3>Monthly Revenue</h3>
                <p class="stat-number">$45,230</p>
                <p class="stat-change positive">+15% from last month</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-content">
              <mat-icon class="stat-icon">trending_up</mat-icon>
              <div class="stat-details">
                <h3>Growth Rate</h3>
                <p class="stat-number">23.5%</p>
                <p class="stat-change positive">+2.1% from last month</p>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <div class="quick-actions">
        <h2>Quick Actions</h2>
        <div class="actions-grid">
          <mat-card class="action-card" (click)="navigateTo('subscriptions')">
            <mat-card-content>
              <mat-icon>subscriptions</mat-icon>
              <h3>Manage Subscriptions</h3>
              <p>View and manage user subscriptions</p>
            </mat-card-content>
          </mat-card>

          <mat-card class="action-card" (click)="navigateTo('plans')">
            <mat-card-content>
              <mat-icon>card_membership</mat-icon>
              <h3>Subscription Plans</h3>
              <p>Create and manage subscription plans</p>
            </mat-card-content>
          </mat-card>

          <mat-card class="action-card" (click)="navigateTo('analytics')">
            <mat-card-content>
              <mat-icon>analytics</mat-icon>
              <h3>Analytics</h3>
              <p>View detailed analytics and reports</p>
            </mat-card-content>
          </mat-card>

          <mat-card class="action-card" (click)="navigateTo('manual-actions')">
            <mat-card-content>
              <mat-icon>settings</mat-icon>
              <h3>Manual Actions</h3>
              <p>Perform manual administrative actions</p>
            </mat-card-content>
          </mat-card>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }

    .welcome-card {
      margin-bottom: 32px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
    }

    .welcome-card mat-card-title {
      color: white;
      font-size: 28px;
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .welcome-card mat-card-subtitle {
      color: rgba(255, 255, 255, 0.8);
      font-size: 16px;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 24px;
      margin-bottom: 32px;
    }

    .stat-card {
      transition: transform 0.2s ease, box-shadow 0.2s ease;
    }

    .stat-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
    }

    .stat-content {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .stat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #667eea;
    }

    .stat-details h3 {
      margin: 0 0 8px 0;
      font-size: 16px;
      color: #666;
      font-weight: 500;
    }

    .stat-number {
      margin: 0 0 4px 0;
      font-size: 28px;
      font-weight: 700;
      color: #333;
    }

    .stat-change {
      margin: 0;
      font-size: 14px;
      font-weight: 500;
    }

    .stat-change.positive {
      color: #4caf50;
    }

    .stat-change.negative {
      color: #f44336;
    }

    .quick-actions h2 {
      margin: 0 0 24px 0;
      font-size: 24px;
      color: #333;
      font-weight: 600;
    }

    .actions-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 24px;
    }

    .action-card {
      cursor: pointer;
      transition: transform 0.2s ease, box-shadow 0.2s ease;
      text-align: center;
      padding: 24px;
    }

    .action-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
    }

    .action-card mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #667eea;
      margin-bottom: 16px;
    }

    .action-card h3 {
      margin: 0 0 12px 0;
      font-size: 18px;
      color: #333;
      font-weight: 600;
    }

    .action-card p {
      margin: 0;
      color: #666;
      line-height: 1.5;
    }

    @media (max-width: 768px) {
      .dashboard-container {
        padding: 16px;
      }
      
      .stats-grid {
        grid-template-columns: 1fr;
      }
      
      .actions-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class AnalyticsDashboardComponent implements OnInit {
  private router = inject(Router);

  ngOnInit() {
    // Initialize dashboard data
  }

  navigateTo(route: string) {
    this.router.navigate([`/admin/${route}`]);
  }
}
