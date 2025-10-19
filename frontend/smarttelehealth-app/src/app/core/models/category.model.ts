/**
 * Category DTOs
 * Matches backend/SmartTelehealth.Application/DTOs/CategoryDto.cs
 */

import { SubscriptionPlanDto } from './subscription-plan.model';

/**
 * Category DTO (Response)
 */
export interface CategoryDto {
  id: string;                    // GUID
  name: string;
  description?: string;
  icon?: string;
  color?: string;
  isActive: boolean;
  displayOrder: number;
  
  // Category Features
  features?: string;
  consultationDescription?: string;
  basePrice: number;
  consultationFee: number;
  consultationDurationMinutes: number;
  requiresHealthAssessment: boolean;
  allowsMedicationDelivery: boolean;
  allowsFollowUpMessaging: boolean;
  allowsOneTimeConsultation: boolean;
  oneTimeConsultationFee: number;
  oneTimeConsultationDurationMinutes: number;
  
  // Marketing Properties
  isMostPopular: boolean;
  isTrending: boolean;
  
  // Relationships
  subscriptionPlans: SubscriptionPlanDto[];
}

/**
 * Create Category DTO (Request - Admin Only)
 */
export interface CreateCategoryDto {
  name: string;
  description: string;
  icon?: string;
  color?: string;
  isActive: boolean;
  displayOrder: number;
  
  // Marketing Properties
  isMostPopular: boolean;
  isTrending: boolean;
}

/**
 * Update Category DTO (Request - Admin Only)
 */
export interface UpdateCategoryDto {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  
  // Marketing Properties
  isMostPopular: boolean;
  isTrending: boolean;
}


