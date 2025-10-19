import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PrivilegeService, SubscriptionService, AuthService } from '../../../../core/services';
import { PrivilegeUsageHistory, SubscriptionDto, UserDto } from '../../../../core/models';

/**
 * Usage History Component
 * Display detailed privilege usage history
 * 
 * APIs Used:
 * - GET /api/Subscriptions/user/{userId}
 * - GET /api/Privileges/history
 * 
 * Route: /web/privileges/history
 * Access: Authenticated users
 */
@Component({
  selector: 'app-usage-history',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './usage-history.component.html',
  styleUrls: ['./usage-history.component.scss']
})
export class UsageHistoryComponent implements OnInit {
  Math = Math;  // Expose Math to template
  currentUser: UserDto | null = null;
  activeSubscription: SubscriptionDto | null = null;
  usageHistory: PrivilegeUsageHistory[] = [];
  loading = false;
  error: string | null = null;

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalRecords = 0;
  totalPages = 0;

  constructor(
    private authService: AuthService,
    private subscriptionService: SubscriptionService,
    private privilegeService: PrivilegeService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    
    if (this.currentUser) {
      this.loadData();
    }
  }

  /**
   * Load subscription and usage history
   */
  loadData(): void {
    if (!this.currentUser) return;

    this.subscriptionService.getUserSubscriptions(this.currentUser.id).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.activeSubscription = response.data.find(
            s => s.status === 'Active' || s.status === 'TrialActive'
          ) || null;

          if (this.activeSubscription) {
            this.loadUsageHistory(this.activeSubscription.id);
          }
        }
      }
    });
  }

  /**
   * Load usage history
   * API: GET /api/Privileges/history
   */
  loadUsageHistory(subscriptionId: string): void {
    this.loading = true;

    this.privilegeService.getUsageHistory(subscriptionId, this.currentPage, this.pageSize).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.usageHistory = response.data;
          
          if (response.meta) {
            this.totalRecords = response.meta.totalRecords;
            this.totalPages = response.meta.totalPages;
          }
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message;
        this.loading = false;
      }
    });
  }

  changePage(page: number): void {
    this.currentPage = page;
    if (this.activeSubscription) {
      this.loadUsageHistory(this.activeSubscription.id);
    }
  }
}

