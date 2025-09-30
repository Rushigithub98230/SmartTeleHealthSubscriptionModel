import { Injectable } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';

export interface ErrorMessage {
  title: string;
  message: string;
  type: 'error' | 'warning' | 'info' | 'success';
  duration?: number;
  action?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlingService {
  private readonly defaultConfig: MatSnackBarConfig = {
    duration: 5000,
    horizontalPosition: 'right',
    verticalPosition: 'top',
    panelClass: []
  };

  constructor(private snackBar: MatSnackBar) {}

  /**
   * Handle HTTP errors with standardized error messages
   */
  handleHttpError(error: HttpErrorResponse, context?: string): ErrorMessage {
    let errorMessage: ErrorMessage;

    if (error.error?.message) {
      // Backend error with message
      errorMessage = {
        title: 'Operation Failed',
        message: error.error.message,
        type: 'error'
      };
    } else if (error.status === 0) {
      // Network error
      errorMessage = {
        title: 'Network Error',
        message: 'Unable to connect to the server. Please check your internet connection.',
        type: 'error'
      };
    } else if (error.status >= 500) {
      // Server error
      errorMessage = {
        title: 'Server Error',
        message: 'An internal server error occurred. Please try again later.',
        type: 'error'
      };
    } else if (error.status === 401) {
      // Unauthorized
      errorMessage = {
        title: 'Authentication Required',
        message: 'Please log in to continue.',
        type: 'warning'
      };
    } else if (error.status === 403) {
      // Forbidden
      errorMessage = {
        title: 'Access Denied',
        message: 'You do not have permission to perform this action.',
        type: 'warning'
      };
    } else if (error.status === 404) {
      // Not found
      errorMessage = {
        title: 'Not Found',
        message: 'The requested resource was not found.',
        type: 'warning'
      };
    } else if (error.status === 409) {
      // Conflict
      errorMessage = {
        title: 'Conflict',
        message: 'The operation conflicts with existing data.',
        type: 'warning'
      };
    } else if (error.status === 422) {
      // Validation error
      errorMessage = {
        title: 'Validation Error',
        message: 'Please check your input and try again.',
        type: 'warning'
      };
    } else {
      // Generic error
      errorMessage = {
        title: 'Error',
        message: 'An unexpected error occurred. Please try again.',
        type: 'error'
      };
    }

    // Add context if provided
    if (context) {
      errorMessage.title = `${context}: ${errorMessage.title}`;
    }

    this.showErrorMessage(errorMessage);
    return errorMessage;
  }

  /**
   * Handle subscription-related errors
   */
  handleSubscriptionError(error: any, operation: string): ErrorMessage {
    const context = `Subscription ${operation}`;
    
    if (error.error?.message) {
      const message = this.parseSubscriptionErrorMessage(error.error.message, operation);
      const errorMessage: ErrorMessage = {
        title: `${context} Failed`,
        message: message,
        type: 'error'
      };
      this.showErrorMessage(errorMessage);
      return errorMessage;
    }

    return this.handleHttpError(error, context);
  }

  /**
   * Handle plan-related errors
   */
  handlePlanError(error: any, operation: string): ErrorMessage {
    const context = `Plan ${operation}`;
    
    if (error.error?.message) {
      const message = this.parsePlanErrorMessage(error.error.message, operation);
      const errorMessage: ErrorMessage = {
        title: `${context} Failed`,
        message: message,
        type: 'error'
      };
      this.showErrorMessage(errorMessage);
      return errorMessage;
    }

    return this.handleHttpError(error, context);
  }

  /**
   * Handle payment-related errors
   */
  handlePaymentError(error: any, operation: string): ErrorMessage {
    const context = `Payment ${operation}`;
    
    if (error.error?.message) {
      const message = this.parsePaymentErrorMessage(error.error.message, operation);
      const errorMessage: ErrorMessage = {
        title: `${context} Failed`,
        message: message,
        type: 'error'
      };
      this.showErrorMessage(errorMessage);
      return errorMessage;
    }

    return this.handleHttpError(error, context);
  }

  /**
   * Show success message
   */
  showSuccess(message: string, title: string = 'Success'): void {
    const successMessage: ErrorMessage = {
      title: title,
      message: message,
      type: 'success',
      duration: 3000
    };
    this.showErrorMessage(successMessage);
  }

  /**
   * Show warning message
   */
  showWarning(message: string, title: string = 'Warning'): void {
    const warningMessage: ErrorMessage = {
      title: title,
      message: message,
      type: 'warning',
      duration: 4000
    };
    this.showErrorMessage(warningMessage);
  }

  /**
   * Show info message
   */
  showInfo(message: string, title: string = 'Information'): void {
    const infoMessage: ErrorMessage = {
      title: title,
      message: message,
      type: 'info',
      duration: 4000
    };
    this.showErrorMessage(infoMessage);
  }

  /**
   * Show error message
   */
  showError(message: string, title: string = 'Error'): void {
    const errorMessage: ErrorMessage = {
      title: title,
      message: message,
      type: 'error',
      duration: 5000
    };
    this.showErrorMessage(errorMessage);
  }

  /**
   * Display error message using MatSnackBar
   */
  private showErrorMessage(errorMessage: ErrorMessage): void {
    const config: MatSnackBarConfig = {
      ...this.defaultConfig,
      duration: errorMessage.duration || this.defaultConfig.duration,
      panelClass: [`snackbar-${errorMessage.type}`]
    };

    const message = `${errorMessage.title}: ${errorMessage.message}`;
    
    this.snackBar.open(message, errorMessage.action || 'Close', config);
  }

  /**
   * Parse subscription-specific error messages
   */
  private parseSubscriptionErrorMessage(message: string, operation: string): string {
    const lowerMessage = message.toLowerCase();
    
    if (lowerMessage.includes('subscription not found')) {
      return 'The subscription was not found. It may have been deleted.';
    }
    
    if (lowerMessage.includes('subscription already exists')) {
      return 'A subscription for this plan already exists.';
    }
    
    if (lowerMessage.includes('subscription is not active')) {
      return 'The subscription is not active and cannot be modified.';
    }
    
    if (lowerMessage.includes('subscription is paused')) {
      return 'The subscription is paused and cannot be modified.';
    }
    
    if (lowerMessage.includes('subscription is cancelled')) {
      return 'The subscription is cancelled and cannot be modified.';
    }
    
    if (lowerMessage.includes('billing cycle')) {
      return 'There was an issue with the billing cycle. Please contact support.';
    }
    
    if (lowerMessage.includes('payment method')) {
      return 'There was an issue with the payment method. Please update your payment information.';
    }
    
    if (lowerMessage.includes('stripe')) {
      return 'There was an issue with the payment processor. Please try again later.';
    }
    
    return message;
  }

  /**
   * Parse plan-specific error messages
   */
  private parsePlanErrorMessage(message: string, operation: string): string {
    const lowerMessage = message.toLowerCase();
    
    if (lowerMessage.includes('plan not found')) {
      return 'The subscription plan was not found.';
    }
    
    if (lowerMessage.includes('plan already exists')) {
      return 'A plan with this name already exists.';
    }
    
    if (lowerMessage.includes('plan is not active')) {
      return 'The plan is not active and cannot be modified.';
    }
    
    if (lowerMessage.includes('plan has active subscriptions')) {
      return 'Cannot delete a plan that has active subscriptions.';
    }
    
    if (lowerMessage.includes('category')) {
      return 'There was an issue with the plan category.';
    }
    
    if (lowerMessage.includes('price')) {
      return 'There was an issue with the plan pricing.';
    }
    
    return message;
  }

  /**
   * Parse payment-specific error messages
   */
  private parsePaymentErrorMessage(message: string, operation: string): string {
    const lowerMessage = message.toLowerCase();
    
    if (lowerMessage.includes('payment method')) {
      return 'There was an issue with your payment method. Please update your payment information.';
    }
    
    if (lowerMessage.includes('insufficient funds')) {
      return 'Your payment method has insufficient funds.';
    }
    
    if (lowerMessage.includes('expired')) {
      return 'Your payment method has expired. Please update your payment information.';
    }
    
    if (lowerMessage.includes('declined')) {
      return 'Your payment was declined. Please contact your bank or try a different payment method.';
    }
    
    if (lowerMessage.includes('stripe')) {
      return 'There was an issue with the payment processor. Please try again later.';
    }
    
    return message;
  }

  /**
   * Get user-friendly error message for common scenarios
   */
  getUserFriendlyMessage(error: any): string {
    if (error.error?.message) {
      return error.error.message;
    }
    
    if (error.message) {
      return error.message;
    }
    
    if (error.status === 0) {
      return 'Unable to connect to the server. Please check your internet connection.';
    }
    
    if (error.status >= 500) {
      return 'An internal server error occurred. Please try again later.';
    }
    
    return 'An unexpected error occurred. Please try again.';
  }

  /**
   * Log error for debugging purposes
   */
  logError(error: any, context?: string): void {
    const errorInfo = {
      context: context || 'Unknown',
      error: error,
      timestamp: new Date().toISOString(),
      userAgent: navigator.userAgent,
      url: window.location.href
    };
    
    console.error('Error logged:', errorInfo);
    
    // In production, you might want to send this to a logging service
    // this.loggingService.logError(errorInfo);
  }
}
