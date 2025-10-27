/**
 * User Model
 * Basic user data structure
 */

export interface UserDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  isActive: boolean;
  createdDate: Date;
  lastLoginDate?: Date;
  role: string;
  profilePicture?: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  zipCode?: string;
  dateOfBirth?: Date;
  gender?: string;
  emergencyContact?: string;
  emergencyPhone?: string;
  medicalHistory?: string;
  allergies?: string;
  currentMedications?: string;
  insuranceProvider?: string;
  insuranceNumber?: string;
  preferredLanguage?: string;
  timeZone?: string;
  notificationPreferences?: any;
  privacySettings?: any;
  twoFactorEnabled: boolean;
  emailVerified: boolean;
  phoneVerified: boolean;
  lastPasswordChangeDate?: Date;
  failedLoginAttempts: number;
  accountLockedUntil?: Date;
  subscriptionCount: number;
  totalSpent: number;
  averageMonthlySpend: number;
  lifetimeValue: number;
  healthScore: number;
  lastActivityDate?: Date;
  preferredContactMethod?: string;
  marketingOptIn: boolean;
  dataProcessingConsent: boolean;
  termsAcceptedDate?: Date;
  privacyPolicyAcceptedDate?: Date;
  customFields?: any;
  tags?: string[];
  notes?: string;
  assignedAgent?: string;
  priorityLevel?: string;
  riskScore?: number;
  churnRisk?: string;
  engagementScore?: number;
  satisfactionScore?: number;
  referralCode?: string;
  referredBy?: string;
  referralCount: number;
  referralEarnings: number;
  loyaltyPoints: number;
  tier?: string;
  status: string;
  statusReason?: string;
  statusChangedDate?: Date;
  statusChangedBy?: string;
  createdBy?: string;
  updatedBy?: string;
  updatedDate?: Date;
  deletedDate?: Date;
  isDeleted: boolean;
  version: number;
  metadata?: any;
}
