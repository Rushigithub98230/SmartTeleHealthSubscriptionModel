import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';
import { BillingCycleDto, CurrencyDto, PrivilegeTypeDto } from '../models/master-data.model';

/**
 * Master Data Service
 * Provides access to system-wide master data (billing cycles, currencies, privilege types)
 * Uses CommonService for all HTTP calls - NO direct HttpClient usage
 * 
 * API Endpoints Used:
 * - GET /api/MasterData/billing-cycles
 * - GET /api/MasterData/currencies
 * - GET /api/MasterData/privilege-types
 */
@Injectable({
  providedIn: 'root'
})
export class MasterDataService {
  constructor(private commonService: CommonService) {}

  /**
   * Get all billing cycles
   * API: GET /api/MasterData/billing-cycles
   * Used in: Plan creation, subscription purchase, admin forms
   */
  getBillingCycles(): Observable<ApiResponse<BillingCycleDto[]>> {
    return this.commonService.get<BillingCycleDto[]>('MasterData/billing-cycles');
  }

  /**
   * Get all supported currencies
   * API: GET /api/MasterData/currencies
   * Used in: Plan creation, billing management, payment forms
   */
  getCurrencies(): Observable<ApiResponse<CurrencyDto[]>> {
    return this.commonService.get<CurrencyDto[]>('MasterData/currencies');
  }

  /**
   * Get all privilege types
   * API: GET /api/MasterData/privilege-types
   * Used in: Privilege management, admin forms
   */
  getPrivilegeTypes(): Observable<ApiResponse<PrivilegeTypeDto[]>> {
    return this.commonService.get<PrivilegeTypeDto[]>('MasterData/privilege-types');
  }
}

