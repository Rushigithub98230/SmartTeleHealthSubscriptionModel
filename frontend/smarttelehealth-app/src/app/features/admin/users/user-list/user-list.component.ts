import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../../core/services';
import { UserDto } from '../../../../core/models';

/**
 * Admin User List Component
 * View and manage all system users with advanced filtering
 * 
 * APIs Used:
 * - GET /api/Users (admin endpoint with filters)
 * 
 * Route: /webadmin/users
 * Access: Admin only
 */
@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit {
  Math = Math; // Expose Math to template
  
  users: UserDto[] = [];
  loading = false;
  error: string | null = null;

  // Filters
  searchTerm = '';
  selectedRole: string = '';
  selectedStatus: string = '';
  selectedSubscriptionStatus: string = '';

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalRecords = 0;
  totalPages = 0;

  // Filter options
  roleOptions = ['All', 'Client', 'Provider', 'Admin'];
  statusOptions = ['All', 'Active', 'Inactive'];
  subscriptionStatusOptions = ['All', 'Active Subscription', 'No Subscription', 'Expired', 'Cancelled'];

  // Quick stats (calculated from loaded users)
  stats = {
    totalUsers: 0,
    activeSubscribers: 0,
    inactiveUsers: 0,
    totalRevenue: 0
  };

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  /**
   * Load all users with filtering
   * API: GET /api/Users
   */
  loadUsers(): void {
    this.loading = true;
    this.error = null;

    const params: any = {
      page: this.currentPage,
      pageSize: this.pageSize
    };

    if (this.searchTerm) params.searchTerm = this.searchTerm;
    if (this.selectedRole && this.selectedRole !== 'All') params.role = this.selectedRole;
    if (this.selectedStatus && this.selectedStatus !== 'All') {
      params.isActive = this.selectedStatus === 'Active';
    }

    this.userService.getAllUsers(params).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.users = response.data;
          
          if (response.meta) {
            this.totalRecords = response.meta.totalRecords;
            this.totalPages = response.meta.totalPages;
          }

          // Calculate stats
          this.calculateStats();
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to load users';
        this.loading = false;
      }
    });
  }

  /**
   * Calculate quick stats from loaded users
   */
  calculateStats(): void {
    this.stats.totalUsers = this.totalRecords;
    this.stats.activeSubscribers = this.users.filter(u => u.hasActiveSubscription === true).length;
    this.stats.inactiveUsers = this.users.filter(u => !u.isActive).length;
    // Revenue would need to be calculated from backend analytics
  }

  /**
   * Apply filters
   */
  applyFilters(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  /**
   * Clear all filters
   */
  clearFilters(): void {
    this.searchTerm = '';
    this.selectedRole = '';
    this.selectedStatus = '';
    this.selectedSubscriptionStatus = '';
    this.currentPage = 1;
    this.loadUsers();
  }

  /**
   * Change page
   */
  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadUsers();
  }

  /**
   * Change page size
   */
  changePageSize(newSize: number): void {
    this.pageSize = newSize;
    this.currentPage = 1;
    this.loadUsers();
  }

  /**
   * Get role badge class
   */
  getRoleBadgeClass(role: string): string {
    const map: { [key: string]: string } = {
      'Admin': 'bg-danger',
      'Provider': 'bg-primary',
      'Client': 'bg-success',
      'User': 'bg-info'
    };
    return map[role] || 'bg-secondary';
  }

  /**
   * Get subscription status badge
   */
  getSubscriptionBadgeClass(user: UserDto): string {
    if (user.hasActiveSubscription === true) {
      return user.currentSubscriptionStatus === 'Active' ? 'bg-success' : 'bg-warning';
    }
    return 'bg-secondary';
  }

  getSubscriptionBadgeText(user: UserDto): string {
    if (user.hasActiveSubscription === true) {
      return user.currentSubscriptionStatus || 'Active';
    }
    if ((user.totalSubscriptions || 0) > 0) {
      return 'Expired';
    }
    return 'No Subscription';
  }

  /**
   * Export users
   */
  exportUsers(): void {
    console.log('Export users to CSV');
    // TODO: Implement export API
  }

  /**
   * Get page numbers for pagination
   */
  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);
    
    if (endPage - startPage < maxPagesToShow - 1) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }
    
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    
    return pages;
  }
}
