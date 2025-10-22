/**
 * Authentication DTOs
 * Matches backend/SmartTelehealth.Application/DTOs/AuthDto.cs
 */

/**
 * Login Request DTO
 */
export interface LoginDto {
  email: string;           // Required, email format
  password: string;        // Required
}

/**
 * Register Request DTO
 */
export interface RegisterDto {
  firstName: string;       // Required
  lastName: string;        // Required
  email: string;           // Required, email format
  password: string;        // Required, min 6 characters
  confirmPassword: string; // Required, must match password
  phoneNumber: string;     // Required
  dateOfBirth: string;     // Required
  gender: string;          // Required
  address: string;         // Required
  city: string;            // Required
  state: string;           // Required
  zipCode: string;         // Required
  role?: string;           // Optional, defaults to "Client"
}

/**
 * Login Response DTO
 */
export interface LoginResponseDto {
  token: string;
  refreshToken: string;
  expiresAt: Date;
  user: UserDto;
  message: string;
}

/**
 * Refresh Token Request DTO
 */
export interface RefreshTokenDto {
  refreshToken: string;
}

/**
 * Forgot Password Request DTO
 */
export interface ForgotPasswordDto {
  email: string;
}

/**
 * Reset Password Request DTO
 */
export interface ResetPasswordDto {
  token: string;
  newPassword: string;
  confirmNewPassword: string;
}

/**
 * Change Password Request DTO
 */
export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

/**
 * User DTO (Response)
 * Matches backend/SmartTelehealth.Application/DTOs/UserDto.cs
 */
export interface UserDto {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phone: string;
  phoneNumber: string;
  userType: string;         // "Client", "Admin", "Provider"
  role: string;
  userRoleId: number;
  isActive: boolean;
  isVerified: boolean;
  isEmailVerified: boolean;
  isPhoneVerified: boolean;
  createdDate: Date;
  updatedDate: Date;
  lastLoginAt?: Date;
  profilePicture?: string;
  dateOfBirth?: Date;
  gender?: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  emergencyContact?: string;
  emergencyPhone?: string;
  stripeCustomerId?: string;
  
  // Subscription metadata (for admin portal)
  totalSubscriptions?: number;
  activeSubscriptions?: number;
  hasActiveSubscription?: boolean;
  currentSubscriptionStatus?: string;
  lastActivityDate?: Date;
}

/**
 * Update User Profile DTO
 */
export interface UpdateUserProfileDto {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  dateOfBirth?: Date;
  gender?: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  emergencyContact?: string;
  emergencyPhone?: string;
}


