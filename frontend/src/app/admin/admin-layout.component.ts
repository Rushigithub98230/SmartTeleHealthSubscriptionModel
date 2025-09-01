import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatBadgeModule } from '@angular/material/badge';
import { AuthService } from './auth/auth.service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule,
    MatBadgeModule
  ],
  template: `
    <div class="admin-layout">
      <!-- Top Navigation Bar -->
      <mat-toolbar class="admin-toolbar" color="primary">
        <button mat-icon-button (click)="toggleSidenav()" class="menu-button">
          <mat-icon>menu</mat-icon>
        </button>
        
        <div class="toolbar-brand">
          <mat-icon class="brand-icon">health_and_safety</mat-icon>
          <span class="brand-text">SmartTeleHealth Admin</span>
        </div>

        <span class="toolbar-spacer"></span>

        <div class="toolbar-actions">
          <button mat-icon-button [matMenuTriggerFor]="notificationsMenu" class="notification-btn">
            <mat-icon [matBadge]="notificationCount" matBadgeColor="warn">notifications</mat-icon>
          </button>
          
          <button mat-icon-button [matMenuTriggerFor]="userMenu" class="user-menu-btn">
            <mat-icon>account_circle</mat-icon>
          </button>
        </div>

        <!-- Notifications Menu -->
        <mat-menu #notificationsMenu="matMenu" class="notifications-menu">
          <div class="menu-header">
            <h3>Notifications</h3>
          </div>
          <mat-divider></mat-divider>
          <div class="notification-item" *ngFor="let notification of notifications">
            <mat-icon class="notification-icon">{{ notification.icon }}</mat-icon>
            <div class="notification-content">
              <p class="notification-text">{{ notification.message }}</p>
              <span class="notification-time">{{ notification.time }}</span>
            </div>
          </div>
          <div class="menu-footer">
            <button mat-button color="primary">View All</button>
          </div>
        </mat-menu>

        <!-- User Menu -->
        <mat-menu #userMenu="matMenu" class="user-menu">
          <div class="menu-header">
            <h3>Admin User</h3>
            <p>admin@smarttelehealth.com</p>
          </div>
          <mat-divider></mat-divider>
          <button mat-menu-item (click)="navigateToProfile()">
            <mat-icon>person</mat-icon>
            <span>Profile</span>
          </button>
          <button mat-menu-item (click)="navigateToSettings()">
            <mat-icon>settings</mat-icon>
            <span>Settings</span>
          </button>
          <mat-divider></mat-divider>
          <button mat-menu-item (click)="logout()" class="logout-btn">
            <mat-icon>exit_to_app</mat-icon>
            <span>Logout</span>
          </button>
        </mat-menu>
      </mat-toolbar>

      <!-- Side Navigation -->
      <mat-sidenav-container class="admin-sidenav-container">
        <mat-sidenav #sidenav mode="side" opened class="admin-sidenav" [fixedInViewport]="true">
          <div class="sidenav-header">
            <mat-icon class="sidenav-icon">admin_panel_settings</mat-icon>
            <span class="sidenav-title">Admin Panel</span>
          </div>
          
          <mat-nav-list class="sidenav-list">
            <a mat-list-item routerLink="/admin/dashboard" routerLinkActive="active-link" class="nav-item">
              <mat-icon matListItemIcon>dashboard</mat-icon>
              <span matListItemTitle>Dashboard</span>
            </a>
            
            <a mat-list-item routerLink="/admin/subscriptions" routerLinkActive="active-link" class="nav-item">
              <mat-icon matListItemIcon>subscriptions</mat-icon>
              <span matListItemTitle>Subscriptions</span>
            </a>
            
            <a mat-list-item routerLink="/admin/plans" routerLinkActive="active-link" class="nav-item">
              <mat-icon matListItemIcon>card_membership</mat-icon>
              <span matListItemTitle>Subscription Plans</span>
            </a>
            
            <a mat-list-item routerLink="/admin/users" routerLinkActive="active-link" class="nav-item">
              <mat-icon matListItemIcon>people</mat-icon>
              <span matListItemTitle>User Management</span>
            </a>
            
            <a mat-list-item routerLink="/admin/analytics" routerLinkActive="active-link" class="nav-item">
              <mat-icon matListItemIcon>analytics</mat-icon>
              <span matListItemTitle>Analytics</span>
            </a>
            
            <a mat-list-item routerLink="/admin/manual-actions" routerLinkActive="active-link" class="nav-item">
              <mat-icon matListItemIcon>settings</mat-icon>
              <span matListItemTitle>Manual Actions</span>
            </a>
            
            <mat-divider></mat-divider>
            
            <a mat-list-item routerLink="/admin/reports" routerLinkActive="active-link" class="nav-item">
              <mat-icon matListItemIcon>assessment</mat-icon>
              <span matListItemTitle>Reports</span>
            </a>
            
            <a mat-list-item routerLink="/admin/settings" routerLinkActive="active-link" class="nav-item">
              <mat-icon matListItemIcon>admin_panel_settings</mat-icon>
              <span matListItemTitle>System Settings</span>
            </a>
          </mat-nav-list>
        </mat-sidenav>

        <!-- Main Content Area -->
        <mat-sidenav-content class="admin-content">
          <div class="content-wrapper">
            <router-outlet></router-outlet>
          </div>
        </mat-sidenav-content>
      </mat-sidenav-container>
    </div>
  `,
  styles: [`
    .admin-layout {
      height: 100vh;
      display: flex;
      flex-direction: column;
    }

    .admin-toolbar {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      z-index: 1000;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .menu-button {
      margin-right: 16px;
    }

    .toolbar-brand {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .brand-icon {
      font-size: 28px;
      width: 28px;
      height: 28px;
    }

    .brand-text {
      font-size: 20px;
      font-weight: 600;
    }

    .toolbar-spacer {
      flex: 1 1 auto;
    }

    .toolbar-actions {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .notification-btn, .user-menu-btn {
      color: white;
    }

    .admin-sidenav-container {
      flex: 1;
      margin-top: 64px;
    }

    .admin-sidenav {
      width: 280px;
      background: #fafafa;
      border-right: 1px solid #e0e0e0;
    }

    .sidenav-header {
      padding: 24px 16px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      text-align: center;
    }

    .sidenav-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 8px;
    }

    .sidenav-title {
      font-size: 18px;
      font-weight: 600;
    }

    .sidenav-list {
      padding-top: 16px;
    }

    .nav-item {
      margin: 4px 8px;
      border-radius: 8px;
      transition: background-color 0.2s ease;
    }

    .nav-item:hover {
      background-color: rgba(102, 126, 234, 0.1);
    }

    .nav-item.active-link {
      background-color: rgba(102, 126, 234, 0.2);
      color: #667eea;
    }

    .nav-item.active-link mat-icon {
      color: #667eea;
    }

    .admin-content {
      background: #f5f5f5;
      min-height: calc(100vh - 64px);
    }

    .content-wrapper {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }

    /* Menu Styles */
    .notifications-menu, .user-menu {
      min-width: 300px;
      max-width: 400px;
    }

    .menu-header {
      padding: 16px;
      background: #f5f5f5;
    }

    .menu-header h3 {
      margin: 0 0 4px 0;
      font-size: 16px;
      font-weight: 600;
      color: #333;
    }

    .menu-header p {
      margin: 0;
      color: #666;
      font-size: 14px;
    }

    .notification-item {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 12px 16px;
      border-bottom: 1px solid #f0f0f0;
    }

    .notification-icon {
      color: #667eea;
      margin-top: 2px;
    }

    .notification-content {
      flex: 1;
    }

    .notification-text {
      margin: 0 0 4px 0;
      font-size: 14px;
      color: #333;
      line-height: 1.4;
    }

    .notification-time {
      font-size: 12px;
      color: #999;
    }

    .menu-footer {
      padding: 12px 16px;
      text-align: center;
      border-top: 1px solid #f0f0f0;
    }

    .logout-btn {
      color: #f44336;
    }

    .logout-btn mat-icon {
      color: #f44336;
    }

    /* Responsive Design */
    @media (max-width: 768px) {
      .admin-sidenav {
        width: 100%;
        max-width: 280px;
      }

      .content-wrapper {
        padding: 16px;
      }

      .brand-text {
        display: none;
      }
    }

    @media (max-width: 480px) {
      .toolbar-actions {
        gap: 4px;
      }
    }
  `]
})
export class AdminLayoutComponent implements OnInit {
  private router = inject(Router);
  private authService = inject(AuthService);

  notificationCount = 3;
  notifications = [
    {
      icon: 'notifications',
      message: 'New subscription request from user@example.com',
      time: '2 minutes ago'
    },
    {
      icon: 'warning',
      message: 'Payment failed for subscription #12345',
      time: '1 hour ago'
    },
    {
      icon: 'info',
      message: 'System maintenance scheduled for tonight',
      time: '3 hours ago'
    }
  ];

  ngOnInit() {
    // Initialize component
  }

  toggleSidenav() {
    // Toggle sidenav visibility
  }

  navigateToProfile() {
    this.router.navigate(['/admin/profile']);
  }

  navigateToSettings() {
    this.router.navigate(['/admin/settings']);
  }

  logout() {
    localStorage.removeItem('adminToken');
    this.router.navigate(['/admin/login']);
  }
}
