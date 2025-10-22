import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';

/**
 * Stripe Synchronization Service
 * Handles Stripe sync operations and discrepancy management
 * 
 * API Endpoints Used:
 * - POST /api/admin/AdminStripeSync/plans/{planId}/sync
 * - GET /api/admin/AdminStripeSync/plans/{planId}/validate
 * - POST /api/admin/AdminStripeSync/plans/{planId}/repair
 * - GET /api/admin/AdminStripeSync/status
 * - GET /api/admin/AdminStripeSync/discrepancies (Phase 5)
 * - POST /api/admin/AdminStripeSync/bulk-sync (Phase 5)
 * - GET /api/admin/AdminStripeSync/history (Phase 5)
 * - GET /api/admin/AdminStripeSync/webhook-status (Phase 5)
 */
@Injectable({
  providedIn: 'root'
})
export class StripeSyncService {
  private readonly baseUrl = 'api/admin/AdminStripeSync';

  constructor(private commonService: CommonService) {}

  /**
   * Synchronize a plan with Stripe
   * API: POST /api/admin/AdminStripeSync/plans/{planId}/sync
   */
  synchronizePlan(planId: string): Observable<ApiResponse<any>> {
    return this.commonService.post<any>(
      `${this.baseUrl}/plans/${planId}/sync`,
      {}
    );
  }

  /**
   * Validate plan synchronization
   * API: GET /api/admin/AdminStripeSync/plans/{planId}/validate
   */
  validatePlanSync(planId: string): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`${this.baseUrl}/plans/${planId}/validate`);
  }

  /**
   * Repair plan synchronization
   * API: POST /api/admin/AdminStripeSync/plans/{planId}/repair
   */
  repairPlanSync(planId: string): Observable<ApiResponse<any>> {
    return this.commonService.post<any>(
      `${this.baseUrl}/plans/${planId}/repair`,
      {}
    );
  }

  /**
   * Get overall sync status
   * API: GET /api/admin/AdminStripeSync/status
   */
  getSyncStatus(): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`${this.baseUrl}/status`);
  }

  // ===== PHASE 5: STRIPE SYNC DASHBOARD ENHANCEMENTS =====

  /**
   * Get all synchronization discrepancies (Admin only)
   * API: GET /api/admin/AdminStripeSync/discrepancies
   * Phase 5: Discrepancy detection and reporting
   */
  getAllDiscrepancies(): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`${this.baseUrl}/discrepancies`);
  }

  /**
   * Bulk synchronize multiple entities (Admin only)
   * API: POST /api/admin/AdminStripeSync/bulk-sync
   * Phase 5: Batch sync operations
   */
  bulkSync(request: any): Observable<ApiResponse<any>> {
    return this.commonService.post<any>(`${this.baseUrl}/bulk-sync`, request);
  }

  /**
   * Get synchronization history (Admin only)
   * API: GET /api/admin/AdminStripeSync/history
   * Phase 5: Audit trail for sync operations
   */
  getSyncHistory(page: number = 1, pageSize: number = 50): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(
      `${this.baseUrl}/history?page=${page}&pageSize=${pageSize}`
    );
  }

  /**
   * Get webhook health status (Admin only)
   * API: GET /api/admin/AdminStripeSync/webhook-status
   * Phase 5: Webhook monitoring
   */
  getWebhookStatus(): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`${this.baseUrl}/webhook-status`);
  }
}

