import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { StripeSyncService } from '../../../../core/services';

/**
 * Stripe Sync Dashboard Component
 * Monitor and manage Stripe synchronization
 * 
 * APIs Used:
 * - GET /api/admin/AdminStripeSync/discrepancies
 * - GET /api/admin/AdminStripeSync/webhook-status
 * - GET /api/admin/AdminStripeSync/status
 * - POST /api/admin/AdminStripeSync/bulk-sync
 * - GET /api/admin/AdminStripeSync/history
 * 
 * Route: /webadmin/stripe-sync
 * Access: Admin only
 */
@Component({
  selector: 'app-stripe-sync-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './sync-dashboard.component.html',
  styleUrls: ['./sync-dashboard.component.scss']
})
export class StripeSyncDashboardComponent implements OnInit {
  discrepancies: any[] = [];
  webhookStatus: any = null;
  syncStatus: any = null;
  syncHistory: any[] = [];
  loading = false;
  error: string | null = null;
  selectedDiscrepancies: Set<string> = new Set();

  // Pagination for history
  currentPage = 1;
  pageSize = 20;
  totalPages = 0;

  constructor(private stripeSyncService: StripeSyncService) {}

  ngOnInit(): void {
    this.loadSyncStatus();
    this.loadDiscrepancies();
    this.loadWebhookStatus();
    this.loadSyncHistory();
  }

  /**
   * Load overall sync status
   */
  loadSyncStatus(): void {
    this.stripeSyncService.getSyncStatus().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.syncStatus = response.data;
        }
      },
      error: (err) => {
        console.error('Error loading sync status:', err);
      }
    });
  }

  /**
   * Load all discrepancies
   */
  loadDiscrepancies(): void {
    this.loading = true;
    this.error = null;

    this.stripeSyncService.getAllDiscrepancies().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.discrepancies = response.data || [];
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load discrepancies';
        this.loading = false;
      }
    });
  }

  /**
   * Load webhook status
   */
  loadWebhookStatus(): void {
    this.stripeSyncService.getWebhookStatus().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.webhookStatus = response.data;
        }
      },
      error: (err) => {
        console.error('Error loading webhook status:', err);
      }
    });
  }

  /**
   * Load sync history
   */
  loadSyncHistory(): void {
    this.stripeSyncService.getSyncHistory(this.currentPage, this.pageSize).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.syncHistory = response.data || [];
          if (response.meta) {
            this.totalPages = response.meta.totalPages;
          }
        }
      },
      error: (err) => {
        console.error('Error loading sync history:', err);
      }
    });
  }

  /**
   * Refresh all data
   */
  refreshAll(): void {
    this.loadSyncStatus();
    this.loadDiscrepancies();
    this.loadWebhookStatus();
    this.loadSyncHistory();
  }

  /**
   * Toggle selection for bulk sync
   */
  toggleSelection(discrepancyId: string): void {
    if (this.selectedDiscrepancies.has(discrepancyId)) {
      this.selectedDiscrepancies.delete(discrepancyId);
    } else {
      this.selectedDiscrepancies.add(discrepancyId);
    }
  }

  /**
   * Bulk sync selected discrepancies
   */
  bulkSync(): void {
    if (this.selectedDiscrepancies.size === 0) {
      alert('Please select discrepancies to sync');
      return;
    }

    if (!confirm(`Sync ${this.selectedDiscrepancies.size} items with Stripe?`)) {
      return;
    }

    const entityIds = Array.from(this.selectedDiscrepancies);
    this.stripeSyncService.bulkSync({
      entityType: 'Mixed',
      entityIds,
      continueOnError: true
    }).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          const result = response.data;
          alert(`Bulk sync complete: ${result.successCount} synced, ${result.failureCount} failed`);
          this.selectedDiscrepancies.clear();
          this.refreshAll();
        }
      },
      error: (err) => alert(err.message || 'Failed to bulk sync')
    });
  }

  /**
   * Get selected count
   */
  get selectedCount(): number {
    return this.selectedDiscrepancies.size;
  }

  /**
   * Get discrepancy severity class
   */
  getSeverityClass(severity: string): string {
    const map: { [key: string]: string } = {
      'Critical': 'bg-danger',
      'Warning': 'bg-warning text-dark',
      'Info': 'bg-info'
    };
    return map[severity] || 'bg-secondary';
  }

  /**
   * Get webhook health class
   */
  getWebhookHealthClass(status: string): string {
    return status === 'Healthy' ? 'text-success' : 'text-danger';
  }
}

