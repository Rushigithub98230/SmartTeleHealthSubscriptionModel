import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';
import {
  BillingRecordDto,
  BillingFilterDto,
  ProcessOverageDto,
  PaymentResult,
  CreateBillingRecordDto
} from '../models';

/**
 * Billing Service
 * Uses CommonService for all HTTP calls - NO direct HttpClient usage
 * 
 * API Endpoints Used:
 * - GET /api/Billing/records
 * - GET /api/Billing/records/{id}
 * - POST /api/Billing/overage
 * - POST /api/Billing/export
 */
@Injectable({
  providedIn: 'root'
})
export class BillingService {
  constructor(private commonService: CommonService) {}

  /**
   * Get billing records with filters
   * API: GET /api/Billing/records
   * Used in: User Billing History, Admin Billing Management
   */
  getBillingRecords(
    userId?: number,
    page: number = 1,
    pageSize: number = 10,
    filters?: Partial<BillingFilterDto>
  ): Observable<ApiResponse<BillingRecordDto[]>> {
    const params: any = { page, pageSize };
    if (userId) params.userId = userId;
    if (filters) {
      if (filters.status) params.status = filters.status;
      if (filters.type) params.type = filters.type;
      if (filters.subscriptionId) params.subscriptionId = filters.subscriptionId;
      if (filters.startDate) params.startDate = filters.startDate;
      if (filters.endDate) params.endDate = filters.endDate;
      if (filters.sortBy) params.sortBy = filters.sortBy;
      if (filters.sortOrder) params.sortOrder = filters.sortOrder;
    }
    
    return this.commonService.get<BillingRecordDto[]>('Billing/records', params);
  }

  /**
   * Get billing record by ID
   * API: GET /api/Billing/records/{id}
   * Used in: Billing Detail Page, Invoice View
   */
  getBillingRecordById(id: string): Observable<ApiResponse<BillingRecordDto>> {
    return this.commonService.get<BillingRecordDto>(`Billing/records/${id}`);
  }

  /**
   * Get subscription billing history
   * API: GET /api/Billing/subscription/{subscriptionId}
   * Used in: Subscription Detail Page
   */
  getSubscriptionBillingHistory(subscriptionId: string): Observable<ApiResponse<BillingRecordDto[]>> {
    return this.commonService.get<BillingRecordDto[]>(`Billing/subscription/${subscriptionId}`);
  }

  /**
   * Process overage payment
   * API: POST /api/Billing/overage
   * Used in: Overage Purchase Modal
   */
  processOveragePayment(dto: ProcessOverageDto): Observable<ApiResponse<PaymentResult>> {
    return this.commonService.post<PaymentResult>('Billing/overage', dto);
  }

  /**
   * Export billing records (Admin Only)
   * API: POST /api/Billing/export
   * Used in: Admin Billing Management - Export
   */
  exportBillingRecords(format: string = 'csv', filters?: BillingFilterDto): Observable<any> {
    const params = { format, ...filters };
    return this.commonService.post('Billing/export', {}, params);
  }

  /**
   * Get billing statistics (Admin Only)
   * API: GET /api/Billing/statistics
   * Used in: Admin Dashboard
   */
  getBillingStatistics(startDate?: Date, endDate?: Date): Observable<ApiResponse<any>> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    
    return this.commonService.get('Billing/statistics', params);
  }
}


