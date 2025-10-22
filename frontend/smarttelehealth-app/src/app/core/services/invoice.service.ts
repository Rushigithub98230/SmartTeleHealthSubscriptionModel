import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';

/**
 * Invoice Service
 * Handles all invoice-related API calls
 * 
 * API Endpoints Used:
 * - POST /api/Invoice/generate/{billingRecordId}
 * - GET /api/Invoice/{invoiceNumber}
 * - GET /api/Invoice/user/{userId}
 * - GET /api/Invoice/{invoiceNumber}/download
 * - POST /api/Invoice/{invoiceNumber}/send
 * - GET /api/Invoice/all (Admin)
 * - POST /api/Invoice/{invoiceNumber}/regenerate (Admin)
 * - GET /api/Invoice/stats (Admin)
 * - POST /api/Invoice/bulk-send (Admin)
 */
@Injectable({
  providedIn: 'root'
})
export class InvoiceService {
  constructor(private commonService: CommonService) {}

  /**
   * Generate invoice for a billing record
   * API: POST /api/Invoice/generate/{billingRecordId}
   */
  generateInvoice(billingRecordId: string): Observable<ApiResponse<any>> {
    return this.commonService.post<any>(
      `Invoice/generate/${billingRecordId}`,
      {}
    );
  }

  /**
   * Get invoice by invoice number
   * API: GET /api/Invoice/{invoiceNumber}
   */
  getInvoice(invoiceNumber: string): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(`Invoice/${invoiceNumber}`);
  }

  /**
   * Get all invoices for a user
   * API: GET /api/Invoice/user/{userId}
   */
  getUserInvoices(
    userId: number,
    page: number = 1,
    pageSize: number = 20
  ): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(
      `Invoice/user/${userId}?page=${page}&pageSize=${pageSize}`
    );
  }

  /**
   * Download invoice in specified format
   * API: GET /api/Invoice/{invoiceNumber}/download
   */
  downloadInvoice(invoiceNumber: string, format: string = 'pdf'): Observable<ApiResponse<any>> {
    return this.commonService.get<any>(
      `Invoice/${invoiceNumber}/download?format=${format}`
    );
  }

  /**
   * Send invoice to email address
   * API: POST /api/Invoice/{invoiceNumber}/send
   */
  sendInvoice(invoiceNumber: string, email: string): Observable<ApiResponse<any>> {
    return this.commonService.post<any>(
      `Invoice/${invoiceNumber}/send`,
      { email }
    );
  }

  // ===== PHASE 4: INVOICE MANAGEMENT ENHANCEMENTS =====

  /**
   * Get all invoices with filtering (Admin only)
   * API: GET /api/Invoice/all
   * Phase 4: Admin invoice management
   */
  getAllInvoices(
    page: number = 1,
    pageSize: number = 20,
    status?: string,
    startDate?: Date,
    endDate?: Date
  ): Observable<ApiResponse<any>> {
    let url = `Invoice/all?page=${page}&pageSize=${pageSize}`;
    
    if (status) url += `&status=${status}`;
    if (startDate) url += `&startDate=${startDate.toISOString()}`;
    if (endDate) url += `&endDate=${endDate.toISOString()}`;

    return this.commonService.get<any>(url);
  }

  /**
   * Regenerate an invoice (Admin only)
   * API: POST /api/Invoice/{invoiceNumber}/regenerate
   * Phase 4: Invoice correction
   */
  regenerateInvoice(invoiceNumber: string): Observable<ApiResponse<any>> {
    return this.commonService.post<any>(
      `Invoice/${invoiceNumber}/regenerate`,
      {}
    );
  }

  /**
   * Get invoice statistics (Admin only)
   * API: GET /api/Invoice/stats
   * Phase 4: Dashboard analytics
   */
  getInvoiceStats(): Observable<ApiResponse<any>> {
    return this.commonService.get<any>('Invoice/stats');
  }

  /**
   * Bulk send multiple invoices (Admin only)
   * API: POST /api/Invoice/bulk-send
   * Phase 4: Batch operations
   */
  bulkSendInvoices(request: any): Observable<ApiResponse<any>> {
    return this.commonService.post<any>('Invoice/bulk-send', request);
  }
}

