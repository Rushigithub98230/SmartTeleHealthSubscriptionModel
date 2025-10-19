import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonService } from '../../../../core/services/common.service';
import { UserDto } from '../../../../core/models';

/**
 * Admin User List Component
 * View and manage all system users
 * 
 * APIs Used:
 * - GET /api/Users (admin endpoint)
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
  users: UserDto[] = [];
  loading = false;
  error: string | null = null;

  // Filters
  searchTerm = '';
  selectedRole: string = '';
  selectedStatus: string = '';

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalRecords = 0;
  totalPages = 0;

  // Filter options
  roleOptions = ['All', 'Client', 'Provider', 'Admin'];
  statusOptions = ['All', 'Active', 'Inactive'];

  constructor(private commonService: CommonService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  /**
   * Load all users
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
    if (this.selectedStatus && this.selectedStatus !== 'All') params.isActive = this.selectedStatus === 'Active';

    this.commonService.get<UserDto[]>('Users', params).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.users = response.data;
          
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
        this.error = error.message || 'Failed to load users';
        this.loading = false;
      }
    });
  }

  /**
   * Apply filters
   */
  applyFilters(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  /**
   * Change page
   */
  changePage(page: number): void {
    this.currentPage = page;
    this.loadUsers();
  }

  /**
   * Get role badge class
   */
  getRoleBadgeClass(role: string): string {
    const map: { [key: string]: string } = {
      'Admin': 'bg-danger',
      'Provider': 'bg-primary',
      'Client': 'bg-success'
    };
    return map[role] || 'bg-secondary';
  }

  /**
   * Export users
   */
  exportUsers(): void {
    console.log('Export users to CSV');
    // Implementation: Call export API
  }
}


