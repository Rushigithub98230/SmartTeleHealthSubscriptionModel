import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';

/**
 * Plan Versioning Service
 * Handles plan version management and user migration
 * 
 * API Endpoints Used:
 * - GET /api/SubscriptionPlans/{planId}/versions
 * - POST /api/SubscriptionPlans/{planId}/create-version
 * - GET /api/SubscriptionPlans/{planId}/grandfathered-users
 * - POST /api/SubscriptionPlans/{planId}/migrate-users
 * - GET /api/SubscriptionPlans/migrations/scheduled
 * - POST /api/SubscriptionPlans/migrations/{id}/execute
 * - DELETE /api/SubscriptionPlans/migrations/{id}
 */
@Injectable({
  providedIn: 'root'
})
export class PlanVersioningService {
  private readonly baseUrl = 'api/SubscriptionPlans';

  constructor(private commonService: CommonService) {}

  /**
   * Get all versions of a plan
   * API: GET /api/SubscriptionPlans/{planId}/versions
   * Phase 6: Plan version history
   */
  getPlanVersions(planId: string): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`${this.baseUrl}/${planId}/versions`);
  }

  /**
   * Create a new version of a plan
   * API: POST /api/SubscriptionPlans/{planId}/create-version
   * Phase 6: Version creation
   */
  createPlanVersion(planId: string, changes: any): Observable<ApiResponse<any>> {
    return this.commonService.post<any>(
      `${this.baseUrl}/${planId}/create-version`,
      { changes }
    );
  }

  /**
   * Get users still on old versions (grandfathered)
   * API: GET /api/SubscriptionPlans/{planId}/grandfathered-users
   * Phase 6: User management
   */
  getGrandfatheredUsers(planId: string): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`${this.baseUrl}/${planId}/grandfathered-users`);
  }

  /**
   * Migrate users to latest version
   * API: POST /api/SubscriptionPlans/{planId}/migrate-users
   * Phase 6: User migration
   */
  migrateUsers(planId: string, request: any): Observable<ApiResponse<any>> {
    return this.commonService.post<any>(
      `${this.baseUrl}/${planId}/migrate-users`,
      request
    );
  }

  /**
   * Get all scheduled migrations
   * API: GET /api/SubscriptionPlans/migrations/scheduled
   * Phase 6: Migration tracking
   */
  getScheduledMigrations(): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`${this.baseUrl}/migrations/scheduled`);
  }

  /**
   * Execute a scheduled migration
   * API: POST /api/SubscriptionPlans/migrations/{id}/execute
   * Phase 6: Migration execution
   */
  executeMigration(migrationId: string): Observable<ApiResponse<any>> {
    return this.commonService.post<any>(
      `${this.baseUrl}/migrations/${migrationId}/execute`,
      {}
    );
  }

  /**
   * Cancel a scheduled migration
   * API: DELETE /api/SubscriptionPlans/migrations/{id}
   * Phase 6: Migration cancellation
   */
  cancelMigration(migrationId: string): Observable<ApiResponse<any>> {
    return this.commonService.delete<any>(`${this.baseUrl}/migrations/${migrationId}`);
  }
}

