using System;

namespace SmartTelehealth.Application.Constants
{
    /// <summary>
    /// Centralized constants for subscription management to ensure consistency across all services
    /// </summary>
    public static class SubscriptionConstants
    {
        #region Payment Retry Constants
        
        /// <summary>
        /// Maximum number of payment retry attempts
        /// </summary>
        public const int MAX_PAYMENT_RETRY_ATTEMPTS = 3;
        
        /// <summary>
        /// Base delay in milliseconds for payment retry exponential backoff
        /// </summary>
        public const int PAYMENT_RETRY_BASE_DELAY_MS = 1000;
        
        /// <summary>
        /// Maximum number of failed payment attempts before subscription suspension
        /// </summary>
        public const int MAX_FAILED_PAYMENT_ATTEMPTS = 3;
        
        #endregion
        
        #region Billing Constants
        
        /// <summary>
        /// Default grace period in days for billing
        /// </summary>
        public const int DEFAULT_BILLING_GRACE_PERIOD_DAYS = 7;
        
        /// <summary>
        /// Default billing cycle fallback in months
        /// </summary>
        public const int DEFAULT_BILLING_CYCLE_MONTHS = 1;
        
        /// <summary>
        /// Default trial duration in days
        /// </summary>
        public const int DEFAULT_TRIAL_DURATION_DAYS = 14;
        
        #endregion
        
        #region Privilege Constants
        
        /// <summary>
        /// Value representing unlimited privilege usage
        /// </summary>
        public const int UNLIMITED_PRIVILEGE_VALUE = -1;
        
        /// <summary>
        /// Default privilege reset period in days
        /// </summary>
        public const int DEFAULT_PRIVILEGE_RESET_PERIOD_DAYS = 30;
        
        #endregion
        
        #region Status Transition Constants
        
        /// <summary>
        /// Maximum number of status transitions per subscription per day
        /// </summary>
        public const int MAX_STATUS_TRANSITIONS_PER_DAY = 10;
        
        #endregion
        
        #region Validation Constants
        
        /// <summary>
        /// Minimum subscription duration in days
        /// </summary>
        public const int MIN_SUBSCRIPTION_DURATION_DAYS = 1;
        
        /// <summary>
        /// Maximum subscription duration in days
        /// </summary>
        public const int MAX_SUBSCRIPTION_DURATION_DAYS = 3650; // 10 years
        
        #endregion
    }
}
