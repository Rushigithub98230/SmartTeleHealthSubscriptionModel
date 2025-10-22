import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PlanVersioningService } from '../../../../core/services';

/**
 * Plan Version History Component
 * View version timeline and grandfathered users
 * 
 * APIs Used:
 * - GET /api/SubscriptionPlans/{planId}/versions
 * - GET /api/SubscriptionPlans/{planId}/grandfathered-users
 * - POST /api/SubscriptionPlans/{planId}/create-version
 * - POST /api/SubscriptionPlans/{planId}/migrate-users
 * 
 * Route: /webadmin/plans/:id/versions
 * Access: Admin only
 */
@Component({
  selector: 'app-plan-version-history',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './plan-version-history.component.html',
  styleUrls: ['./plan-version-history.component.scss']
})
export class PlanVersionHistoryComponent implements OnInit {
  planId: string = '';
  versions: any[] = [];
  grandfatheredUsers: any[] = [];
  loading = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private versioningService: PlanVersioningService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.planId = params['id'];
      this.loadVersions();
      this.loadGrandfatheredUsers();
    });
  }

  /**
   * Load plan versions
   */
  loadVersions(): void {
    this.loading = true;
    this.error = null;

    this.versioningService.getPlanVersions(this.planId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.versions = response.data || [];
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load versions';
        this.loading = false;
      }
    });
  }

  /**
   * Load grandfathered users
   */
  loadGrandfatheredUsers(): void {
    this.versioningService.getGrandfatheredUsers(this.planId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.grandfatheredUsers = response.data || [];
        }
      },
      error: (err) => {
        console.error('Error loading grandfathered users:', err);
      }
    });
  }

  /**
   * Trigger migration to latest version
   */
  migrateUsers(): void {
    if (!confirm('Migrate all grandfathered users to the latest version?')) {
      return;
    }

    this.versioningService.migrateUsers(this.planId, {
      migrateImmediately: false,
      notifyUsers: true,
      noticeDays: 10
    }).subscribe({
      next: () => {
        alert('Migration scheduled successfully');
        this.loadGrandfatheredUsers();
      },
      error: (err) => alert(err.message || 'Failed to schedule migration')
    });
  }

  /**
   * Get version badge class
   */
  getVersionBadgeClass(version: any): string {
    return version.isLatestVersion ? 'bg-success' : 'bg-secondary';
  }

  /**
   * Get total grandfathered users count
   */
  get totalGrandfatheredUsers(): number {
    return this.grandfatheredUsers.reduce((sum, g) => sum + (g.userCount || 0), 0);
  }
}

