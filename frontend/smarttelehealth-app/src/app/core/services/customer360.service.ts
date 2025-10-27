import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { ApiResponse } from '../models/api-response.model';
import { SubscriptionDto } from '../models/subscription.model';
import { BillingRecordDto } from '../models/billing.model';
import { UserDto } from '../models/user.model';

/**
 * Customer 360 Service
 * Aggregates comprehensive user data for Customer 360 view
 */
@Injectable({
  providedIn: 'root'
})
export class Customer360Service {
  private readonly apiUrl = '/api';

  constructor(private http: HttpClient) {}

  /**
   * Get complete user profile with all related data
   */
  getUserCompleteProfile(userId: number): Observable<Customer360Data> {
    return forkJoin({
      userProfile: this.getUserProfile(userId),
      subscriptions: this.getUserSubscriptions(userId),
      billingHistory: this.getUserBillingHistory(userId),
      invoices: this.getUserInvoices(userId),
      privilegeUsage: this.getUserPrivilegeUsage(userId)
    }).pipe(
      map(data => {
        const customer360Data: Customer360Data = {
          userProfile: data.userProfile,
          subscriptions: data.subscriptions,
          billingHistory: data.billingHistory,
          invoices: data.invoices,
          privilegeUsage: data.privilegeUsage,
          healthScore: this.calculateHealthScore(data),
          summary: this.generateSummary(data)
        };
        return customer360Data;
      }),
      catchError(this.handleError)
    );
  }

  /**
   * Get user profile
   */
  getUserProfile(userId: number): Observable<UserDto> {
    return this.http.get<ApiResponse<UserDto>>(`${this.apiUrl}/Users/${userId}`)
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  /**
   * Get user subscription history
   */
  getUserSubscriptions(userId: number): Observable<SubscriptionDto[]> {
    return this.http.get<ApiResponse<SubscriptionDto[]>>(`${this.apiUrl}/Subscriptions/user/${userId}`)
      .pipe(
        map(response => response.data || []),
        catchError(this.handleError)
      );
  }

  /**
   * Get user billing history
   */
  getUserBillingHistory(userId: number): Observable<BillingRecordDto[]> {
    return this.http.get<ApiResponse<BillingRecordDto[]>>(`${this.apiUrl}/Billing/user/${userId}`)
      .pipe(
        map(response => response.data || []),
        catchError(this.handleError)
      );
  }

  /**
   * Get user invoices
   */
  getUserInvoices(userId: number): Observable<any[]> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/Invoices/user/${userId}`)
      .pipe(
        map(response => response.data || []),
        catchError(this.handleError)
      );
  }

  /**
   * Get user privilege usage
   */
  getUserPrivilegeUsage(userId: number): Observable<PrivilegeUsageDto[]> {
    // This would need to be implemented based on available endpoints
    // For now, return empty array
    return of([]);
  }

  /**
   * Calculate customer health score
   */
  calculateHealthScore(data: any): number {
    let score = 100;

    // Payment history (40% weight)
    const totalPayments = data.billingHistory.length;
    const successfulPayments = data.billingHistory.filter((b: any) => b.status === 'Paid').length;
    if (totalPayments > 0) {
      const paymentSuccessRate = successfulPayments / totalPayments;
      score -= (1 - paymentSuccessRate) * 40;
    }

    // Subscription status (30% weight)
    const activeSubscriptions = data.subscriptions.filter((s: any) => s.status === 'Active').length;
    const pausedSubscriptions = data.subscriptions.filter((s: any) => s.status === 'Paused').length;
    const cancelledSubscriptions = data.subscriptions.filter((s: any) => s.status === 'Cancelled').length;

    if (activeSubscriptions === 0) {
      score -= 30; // No active subscriptions
    } else if (pausedSubscriptions > 0) {
      score -= 15; // Some paused subscriptions
    }

    if (cancelledSubscriptions > activeSubscriptions) {
      score -= 20; // More cancelled than active
    }

    // Usage patterns (20% weight)
    const totalUsage = data.privilegeUsage.reduce((sum: number, usage: any) => sum + usage.usedValue, 0);
    const totalAllotted = data.privilegeUsage.reduce((sum: number, usage: any) => sum + usage.allowedValue, 0);
    
    if (totalAllotted > 0) {
      const usageRate = totalUsage / totalAllotted;
      if (usageRate < 0.3) {
        score -= 20; // Underutilizing
      } else if (usageRate > 0.9) {
        score += 5; // Actively using
      }
    }

    // Account tenure (10% weight)
    const accountAge = this.calculateAccountAge(data.userProfile.createdDate);
    if (accountAge > 365) {
      score += 10; // Loyal customer
    } else if (accountAge < 30) {
      score -= 5; // New customer
    }

    // Recent activity bonus
    const recentActivity = this.calculateRecentActivity(data);
    if (recentActivity > 0.8) {
      score += 5; // High recent activity
    }

    return Math.max(0, Math.min(100, Math.round(score)));
  }

  /**
   * Generate customer summary
   */
  generateSummary(data: any): CustomerSummary {
    const totalSpent = data.billingHistory
      .filter((b: any) => b.status === 'Paid')
      .reduce((sum: number, b: any) => sum + b.totalAmount, 0);

    const lifetimeValue = totalSpent;
    const averageMonthlySpend = this.calculateAverageMonthlySpend(data.billingHistory);
    const accountAge = this.calculateAccountAge(data.userProfile.createdDate);

    return {
      totalSubscriptions: data.subscriptions.length,
      activeSubscriptions: data.subscriptions.filter((s: any) => s.status === 'Active').length,
      totalSpent,
      lifetimeValue,
      averageMonthlySpend,
      accountAge,
      lastActivity: this.getLastActivityDate(data),
      paymentSuccessRate: this.calculatePaymentSuccessRate(data.billingHistory),
      subscriptionRetentionRate: this.calculateSubscriptionRetentionRate(data.subscriptions)
    };
  }

  /**
   * Get health score color class
   */
  getHealthScoreClass(score: number): string {
    if (score >= 90) return 'health-excellent';
    if (score >= 70) return 'health-good';
    if (score >= 50) return 'health-fair';
    return 'health-poor';
  }

  /**
   * Get health score label
   */
  getHealthScoreLabel(score: number): string {
    if (score >= 90) return 'Excellent';
    if (score >= 70) return 'Good';
    if (score >= 50) return 'Fair';
    return 'Poor';
  }

  // Private helper methods
  private calculateAccountAge(createdDate: Date): number {
    const now = new Date();
    const created = new Date(createdDate);
    return Math.floor((now.getTime() - created.getTime()) / (1000 * 60 * 60 * 24));
  }

  private calculateRecentActivity(data: any): number {
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);

    const recentBilling = data.billingHistory.filter((b: any) => 
      new Date(b.createdDate) >= thirtyDaysAgo
    ).length;

    const recentSubscriptions = data.subscriptions.filter((s: any) => 
      new Date(s.createdDate) >= thirtyDaysAgo
    ).length;

    return Math.min(1, (recentBilling + recentSubscriptions) / 10);
  }

  private calculateAverageMonthlySpend(billingHistory: any[]): number {
    if (billingHistory.length === 0) return 0;

    const paidBills = billingHistory.filter((b: any) => b.status === 'Paid');
    if (paidBills.length === 0) return 0;

    const totalSpent = paidBills.reduce((sum: number, b: any) => sum + b.totalAmount, 0);
    const months = this.calculateMonthsBetween(
      new Date(Math.min(...paidBills.map((b: any) => new Date(b.createdDate).getTime()))),
      new Date(Math.max(...paidBills.map((b: any) => new Date(b.createdDate).getTime())))
    );

    return months > 0 ? totalSpent / months : totalSpent;
  }

  private calculateMonthsBetween(startDate: Date, endDate: Date): number {
    const yearDiff = endDate.getFullYear() - startDate.getFullYear();
    const monthDiff = endDate.getMonth() - startDate.getMonth();
    return yearDiff * 12 + monthDiff + 1;
  }

  private getLastActivityDate(data: any): Date {
    const dates: Date[] = [];

    // Add subscription dates
    data.subscriptions.forEach((s: any) => {
      dates.push(new Date(s.createdDate));
      if (s.updatedDate) dates.push(new Date(s.updatedDate));
    });

    // Add billing dates
    data.billingHistory.forEach((b: any) => {
      dates.push(new Date(b.createdDate));
      if (b.paidAt) dates.push(new Date(b.paidAt));
    });

    return dates.length > 0 ? new Date(Math.max(...dates.map(d => d.getTime()))) : new Date();
  }

  private calculatePaymentSuccessRate(billingHistory: any[]): number {
    if (billingHistory.length === 0) return 0;
    const successful = billingHistory.filter((b: any) => b.status === 'Paid').length;
    return (successful / billingHistory.length) * 100;
  }

  private calculateSubscriptionRetentionRate(subscriptions: any[]): number {
    if (subscriptions.length === 0) return 0;
    const active = subscriptions.filter((s: any) => s.status === 'Active').length;
    return (active / subscriptions.length) * 100;
  }

  private handleError = (error: any): Observable<never> => {
    console.error('Customer 360 service error:', error);
    throw error;
  };
}

// Data Models
export interface Customer360Data {
  userProfile: UserDto;
  subscriptions: SubscriptionDto[];
  billingHistory: BillingRecordDto[];
  invoices: any[];
  privilegeUsage: PrivilegeUsageDto[];
  healthScore: number;
  summary: CustomerSummary;
}

export interface CustomerSummary {
  totalSubscriptions: number;
  activeSubscriptions: number;
  totalSpent: number;
  lifetimeValue: number;
  averageMonthlySpend: number;
  accountAge: number;
  lastActivity: Date;
  paymentSuccessRate: number;
  subscriptionRetentionRate: number;
}

export interface PrivilegeUsageDto {
  privilegeId: string;
  privilegeName: string;
  allowedValue: number;
  usedValue: number;
  remainingValue: number;
  lastUsed: Date;
}
