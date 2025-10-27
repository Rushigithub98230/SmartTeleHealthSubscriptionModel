import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';
import {
  PaymentMethodDto,
  AddPaymentMethodDto,
  ProcessPaymentRequestDto,
  PaymentHistoryDto
} from '../models';

/**
 * Payment Service
 * Uses CommonService for all HTTP calls - NO direct HttpClient usage
 * 
 * API Endpoints Used:
 * - GET /api/Payment/methods
 * - POST /api/Payment/methods
 * - PUT /api/Payment/methods/default
 * - DELETE /api/Payment/methods/{id}
 * - POST /api/Payment/process
 */
@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  constructor(private commonService: CommonService) {}

  /**
   * Get user's payment methods
   * API: GET /api/payments/payment-methods
   * Used in: Payment Methods Page, Checkout
   * FIXED: Updated endpoint to match backend
   */
  getPaymentMethods(userId: number): Observable<ApiResponse<PaymentMethodDto[]>> {
    return this.commonService.get<PaymentMethodDto[]>('payments/payment-methods');
  }

  /**
   * Add new payment method
   * API: POST /api/payments/payment-methods
   * Used in: Add Card Form
   * FIXED: Simplified to only send paymentMethodId (backend requirement)
   */
  addPaymentMethod(paymentMethodId: string): Observable<ApiResponse<PaymentMethodDto>> {
    return this.commonService.post<PaymentMethodDto>('payments/payment-methods', { paymentMethodId });
  }

  /**
   * Set default payment method
   * API: PUT /api/payments/payment-methods/{paymentMethodId}/default
   * Used in: Payment Methods Page
   * FIXED: Updated to use URL parameter instead of body (backend requirement)
   */
  setDefaultPaymentMethod(paymentMethodId: string): Observable<ApiResponse<any>> {
    return this.commonService.put(`payments/payment-methods/${paymentMethodId}/default`, {});
  }

  /**
   * Delete payment method
   * API: DELETE /api/payments/payment-methods/{id}
   * Used in: Payment Methods Page
   * FIXED: Updated endpoint to match backend
   */
  deletePaymentMethod(paymentMethodId: string): Observable<ApiResponse<any>> {
    return this.commonService.delete(`payments/payment-methods/${paymentMethodId}`);
  }

  /**
   * Remove payment method (alias for deletePaymentMethod)
   * API: DELETE /api/payments/payment-methods/{id}
   * Used in: Payment Methods Page
   */
  removePaymentMethod(paymentMethodId: string): Observable<ApiResponse<any>> {
    return this.deletePaymentMethod(paymentMethodId);
  }

  /**
   * Process payment (Manual Renewal)
   * API: POST /api/payments/process-payment
   * Used in: Manual payment processing for failed renewals
   * FIXED: Updated endpoint to match backend
   */
  processPayment(dto: ProcessPaymentRequestDto): Observable<ApiResponse<any>> {
    return this.commonService.post('payments/process-payment', dto);
  }

  /**
   * Get payment history
   * API: GET /api/Payment/history
   * Used in: User Payment History Page
   */
  getPaymentHistory(
    userId: number,
    page: number = 1,
    pageSize: number = 10
  ): Observable<ApiResponse<PaymentHistoryDto[]>> {
    return this.commonService.get<PaymentHistoryDto[]>('Payment/history', { userId, page, pageSize });
  }

  // ===== PHASE 3: FAILED PAYMENT MANAGEMENT =====

  /**
   * Get all failed payments (Admin only)
   * API: GET /api/Payment/failed
   * Phase 3: Failed Payment Management
   */
  getFailedPayments(): Observable<ApiResponse<any>> {
    return this.commonService.get<any>('Payment/failed');
  }

  /**
   * Retry a failed payment (Admin only)
   * API: POST /api/Payment/retry-payment/{billingRecordId}
   * Phase 3: Manual retry for failed payments
   */
  retryPayment(billingRecordId: string): Observable<ApiResponse<any>> {
    return this.commonService.post<any>(
      `Payment/retry-payment/${billingRecordId}`,
      {}
    );
  }

  /**
   * Send payment reminder email to customer (Admin only)
   * API: POST /api/Payment/{id}/send-reminder
   * Phase 3: Customer communication
   */
  sendPaymentReminder(billingRecordId: string, request: any): Observable<ApiResponse<any>> {
    return this.commonService.post<any>(
      `Payment/${billingRecordId}/send-reminder`,
      request
    );
  }

  /**
   * Bulk retry multiple failed payments (Admin only)
   * API: POST /api/Payment/bulk-retry
   * Phase 3: Batch processing
   */
  bulkRetryPayments(request: any): Observable<ApiResponse<any>> {
    return this.commonService.post<any>('Payment/bulk-retry', request);
  }
}


