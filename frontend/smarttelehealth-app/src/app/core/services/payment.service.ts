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
   * API: GET /api/Payment/methods
   * Used in: Payment Methods Page, Checkout
   */
  getPaymentMethods(userId: number): Observable<ApiResponse<PaymentMethodDto[]>> {
    return this.commonService.get<PaymentMethodDto[]>('Payment/methods', { userId });
  }

  /**
   * Add new payment method
   * API: POST /api/Payment/methods
   * Used in: Add Card Form
   */
  addPaymentMethod(dto: AddPaymentMethodDto): Observable<ApiResponse<PaymentMethodDto>> {
    return this.commonService.post<PaymentMethodDto>('Payment/methods', dto);
  }

  /**
   * Set default payment method
   * API: PUT /api/Payment/methods/default
   * Used in: Payment Methods Page
   */
  setDefaultPaymentMethod(paymentMethodId: string): Observable<ApiResponse<any>> {
    return this.commonService.put('Payment/methods/default', { paymentMethodId });
  }

  /**
   * Delete payment method
   * API: DELETE /api/Payment/methods/{id}
   * Used in: Payment Methods Page
   */
  deletePaymentMethod(paymentMethodId: string): Observable<ApiResponse<any>> {
    return this.commonService.delete(`Payment/methods/${paymentMethodId}`);
  }

  /**
   * Process payment
   * API: POST /api/Payment/process
   * Used in: Manual payment processing
   */
  processPayment(dto: ProcessPaymentRequestDto): Observable<ApiResponse<any>> {
    return this.commonService.post('Payment/process', dto);
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
}


