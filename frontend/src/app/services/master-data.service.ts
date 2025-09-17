import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { 
  MasterBillingCycle,
  MasterCurrency,
  MasterPrivilegeType,
  Privilege
} from '../models/subscription.models';
import { CommonService, ApiResponse } from './common.service';

@Injectable({
  providedIn: 'root'
})
export class MasterDataService {
  constructor(private commonService: CommonService) {}

  // Master Data APIs
  getBillingCycles(): Observable<ApiResponse<MasterBillingCycle[]>> {
    return this.commonService.getWithAuth<MasterBillingCycle[]>('/api/MasterData/billing-cycles');
  }

  getCurrencies(): Observable<ApiResponse<MasterCurrency[]>> {
    return this.commonService.getWithAuth<MasterCurrency[]>('/api/MasterData/currencies');
  }

  getPrivilegeTypes(): Observable<ApiResponse<MasterPrivilegeType[]>> {
    return this.commonService.getWithAuth<MasterPrivilegeType[]>('/api/MasterData/privilege-types');
  }

  getPrivileges(): Observable<ApiResponse<Privilege[]>> {
    return this.commonService.getWithAuth<Privilege[]>('/api/SubscriptionPlanPrivileges/privileges');
  }
}