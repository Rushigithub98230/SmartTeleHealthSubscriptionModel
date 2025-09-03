import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';
import { MasterBillingCycle, MasterCurrency, MasterPrivilegeType, Privilege } from '../models/subscription.models';

@Injectable({
  providedIn: 'root'
})
export class MasterDataService {
  constructor(private commonService: CommonService) {}

  /**
   * Get all billing cycles
   */
  getBillingCycles(): Observable<ApiResponse<MasterBillingCycle[]>> {
    return this.commonService.getWithAuth<MasterBillingCycle[]>('/api/MasterData/billing-cycles');
  }

  /**
   * Get all currencies
   */
  getCurrencies(): Observable<ApiResponse<MasterCurrency[]>> {
    return this.commonService.getWithAuth<MasterCurrency[]>('/api/MasterData/currencies');
  }

  /**
   * Get all privilege types
   */
  getPrivilegeTypes(): Observable<ApiResponse<MasterPrivilegeType[]>> {
    return this.commonService.getWithAuth<MasterPrivilegeType[]>('/api/MasterData/privilege-types');
  }

  /**
   * Get all privileges
   */
  getPrivileges(): Observable<ApiResponse<Privilege[]>> {
    return this.commonService.getWithAuth<Privilege[]>('/api/Privileges');
  }

  /**
   * Get active billing cycles only
   */
  getActiveBillingCycles(): Observable<ApiResponse<MasterBillingCycle[]>> {
    return this.commonService.getWithAuth<MasterBillingCycle[]>('/api/MasterData/billing-cycles?isActive=true');
  }

  /**
   * Get active currencies only
   */
  getActiveCurrencies(): Observable<ApiResponse<MasterCurrency[]>> {
    return this.commonService.getWithAuth<MasterCurrency[]>('/api/MasterData/currencies?isActive=true');
  }

  /**
   * Get active privileges only
   */
  getActivePrivileges(): Observable<ApiResponse<Privilege[]>> {
    return this.commonService.getWithAuth<Privilege[]>('/api/Privileges?isActive=true');
  }
}
