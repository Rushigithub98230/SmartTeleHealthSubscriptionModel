/**
 * Master Data DTOs
 * For billing cycles, currencies, etc.
 */

/**
 * Billing Cycle DTO
 * Matches backend/SmartTelehealth.Core/Entities/MasterBillingCycle
 */
export interface BillingCycleDto {
  id: string;                    // GUID
  name: string;                  // "Monthly", "Quarterly", "Annual"
  durationInDays: number;        // 30, 90, 365
  isActive: boolean;
  displayOrder: number;
  description?: string;
}

/**
 * Currency DTO
 * Matches backend/SmartTelehealth.Core/Entities/MasterCurrency
 */
export interface CurrencyDto {
  id: string;
  code: string;                  // "USD", "EUR", etc.
  name: string;
  symbol: string;                // "$", "€", etc.
  isActive: boolean;
}

/**
 * Privilege Type DTO
 */
export interface PrivilegeTypeDto {
  id: string;
  name: string;
  description?: string;
  category: string;
  isActive: boolean;
}


