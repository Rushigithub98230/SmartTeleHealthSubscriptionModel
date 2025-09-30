import { Injectable } from '@angular/core';
import { Observable, of, BehaviorSubject } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';

export interface CacheItem<T> {
  data: T;
  timestamp: number;
  expiry: number;
}

export interface CacheConfig {
  ttl: number; // Time to live in milliseconds
  maxSize: number; // Maximum number of items in cache
}

@Injectable({
  providedIn: 'root'
})
export class DataCachingService {
  private cache = new Map<string, CacheItem<any>>();
  private readonly defaultConfig: CacheConfig = {
    ttl: 5 * 60 * 1000, // 5 minutes
    maxSize: 100
  };

  // Cache keys
  static readonly CACHE_KEYS = {
    SUBSCRIPTION_PLANS: 'subscription_plans',
    ACTIVE_PLANS: 'active_plans',
    CATEGORIES: 'categories',
    BILLING_CYCLES: 'billing_cycles',
    USER_SUBSCRIPTIONS: 'user_subscriptions',
    PLAN_DETAILS: 'plan_details_',
    CATEGORY_DETAILS: 'category_details_',
    BILLING_CYCLE_DETAILS: 'billing_cycle_details_'
  };

  constructor() {
    // Clean up expired cache items every minute
    setInterval(() => {
      this.cleanupExpiredItems();
    }, 60000);
  }

  /**
   * Get data from cache or execute the observable function
   */
  getOrFetch<T>(
    key: string,
    fetchFunction: () => Observable<T>,
    config?: Partial<CacheConfig>
  ): Observable<T> {
    const cacheConfig = { ...this.defaultConfig, ...config };
    
    // Check if data exists in cache and is not expired
    const cachedItem = this.cache.get(key);
    if (cachedItem && this.isValid(cachedItem)) {
      return of(cachedItem.data);
    }

    // Fetch new data and cache it
    return fetchFunction().pipe(
      tap(data => {
        this.set(key, data, cacheConfig);
      }),
      catchError(error => {
        // If fetch fails and we have cached data (even if expired), return it
        if (cachedItem) {
          console.warn(`Fetch failed for ${key}, returning cached data`, error);
          return of(cachedItem.data);
        }
        throw error;
      })
    );
  }

  /**
   * Set data in cache
   */
  set<T>(key: string, data: T, config?: Partial<CacheConfig>): void {
    const cacheConfig = { ...this.defaultConfig, ...config };
    
    // Remove oldest items if cache is full
    if (this.cache.size >= cacheConfig.maxSize) {
      this.removeOldestItems();
    }

    const cacheItem: CacheItem<T> = {
      data: data,
      timestamp: Date.now(),
      expiry: Date.now() + cacheConfig.ttl
    };

    this.cache.set(key, cacheItem);
  }

  /**
   * Get data from cache without fetching
   */
  get<T>(key: string): T | null {
    const cachedItem = this.cache.get(key);
    if (cachedItem && this.isValid(cachedItem)) {
      return cachedItem.data;
    }
    return null;
  }

  /**
   * Check if cache item exists and is valid
   */
  has(key: string): boolean {
    const cachedItem = this.cache.get(key);
    return cachedItem && this.isValid(cachedItem);
  }

  /**
   * Remove item from cache
   */
  remove(key: string): void {
    this.cache.delete(key);
  }

  /**
   * Clear all cache
   */
  clear(): void {
    this.cache.clear();
  }

  /**
   * Clear cache by pattern
   */
  clearByPattern(pattern: string): void {
    const regex = new RegExp(pattern);
    for (const key of this.cache.keys()) {
      if (regex.test(key)) {
        this.cache.delete(key);
      }
    }
  }

  /**
   * Get cache statistics
   */
  getStats(): { size: number; keys: string[]; hitRate: number } {
    return {
      size: this.cache.size,
      keys: Array.from(this.cache.keys()),
      hitRate: this.calculateHitRate()
    };
  }

  /**
   * Invalidate specific cache items
   */
  invalidate(key: string): void {
    this.remove(key);
  }

  /**
   * Invalidate multiple cache items
   */
  invalidateMultiple(keys: string[]): void {
    keys.forEach(key => this.remove(key));
  }

  /**
   * Invalidate cache by pattern
   */
  invalidateByPattern(pattern: string): void {
    this.clearByPattern(pattern);
  }

  /**
   * Preload data into cache
   */
  preload<T>(key: string, fetchFunction: () => Observable<T>, config?: Partial<CacheConfig>): void {
    fetchFunction().subscribe({
      next: (data) => {
        this.set(key, data, config);
      },
      error: (error) => {
        console.warn(`Failed to preload cache for ${key}:`, error);
      }
    });
  }

  /**
   * Check if cache item is valid (not expired)
   */
  private isValid(cacheItem: CacheItem<any>): boolean {
    return Date.now() < cacheItem.expiry;
  }

  /**
   * Clean up expired cache items
   */
  private cleanupExpiredItems(): void {
    const now = Date.now();
    for (const [key, item] of this.cache.entries()) {
      if (now >= item.expiry) {
        this.cache.delete(key);
      }
    }
  }

  /**
   * Remove oldest cache items when cache is full
   */
  private removeOldestItems(): void {
    const items = Array.from(this.cache.entries());
    items.sort((a, b) => a[1].timestamp - b[1].timestamp);
    
    // Remove oldest 10% of items
    const itemsToRemove = Math.ceil(items.length * 0.1);
    for (let i = 0; i < itemsToRemove; i++) {
      this.cache.delete(items[i][0]);
    }
  }

  /**
   * Calculate cache hit rate (simplified)
   */
  private calculateHitRate(): number {
    // This is a simplified implementation
    // In a real application, you'd track hits and misses
    return 0.85; // Placeholder value
  }

  /**
   * Cache subscription plans
   */
  cacheSubscriptionPlans(plans: any[]): void {
    this.set(DataCachingService.CACHE_KEYS.SUBSCRIPTION_PLANS, plans, { ttl: 10 * 60 * 1000 });
  }

  /**
   * Cache active plans
   */
  cacheActivePlans(plans: any[]): void {
    this.set(DataCachingService.CACHE_KEYS.ACTIVE_PLANS, plans, { ttl: 5 * 60 * 1000 });
  }

  /**
   * Cache categories
   */
  cacheCategories(categories: any[]): void {
    this.set(DataCachingService.CACHE_KEYS.CATEGORIES, categories, { ttl: 15 * 60 * 1000 });
  }

  /**
   * Cache billing cycles
   */
  cacheBillingCycles(cycles: any[]): void {
    this.set(DataCachingService.CACHE_KEYS.BILLING_CYCLES, cycles, { ttl: 30 * 60 * 1000 });
  }

  /**
   * Cache user subscriptions
   */
  cacheUserSubscriptions(userId: string, subscriptions: any[]): void {
    const key = `${DataCachingService.CACHE_KEYS.USER_SUBSCRIPTIONS}_${userId}`;
    this.set(key, subscriptions, { ttl: 2 * 60 * 1000 });
  }

  /**
   * Get cached subscription plans
   */
  getCachedSubscriptionPlans(): any[] | null {
    return this.get(DataCachingService.CACHE_KEYS.SUBSCRIPTION_PLANS);
  }

  /**
   * Get cached active plans
   */
  getCachedActivePlans(): any[] | null {
    return this.get(DataCachingService.CACHE_KEYS.ACTIVE_PLANS);
  }

  /**
   * Get cached categories
   */
  getCachedCategories(): any[] | null {
    return this.get(DataCachingService.CACHE_KEYS.CATEGORIES);
  }

  /**
   * Get cached billing cycles
   */
  getCachedBillingCycles(): any[] | null {
    return this.get(DataCachingService.CACHE_KEYS.BILLING_CYCLES);
  }

  /**
   * Get cached user subscriptions
   */
  getCachedUserSubscriptions(userId: string): any[] | null {
    const key = `${DataCachingService.CACHE_KEYS.USER_SUBSCRIPTIONS}_${userId}`;
    return this.get(key);
  }

  /**
   * Invalidate subscription-related cache
   */
  invalidateSubscriptionCache(): void {
    this.invalidateByPattern('subscription');
  }

  /**
   * Invalidate plan-related cache
   */
  invalidatePlanCache(): void {
    this.invalidateByPattern('plan');
  }

  /**
   * Invalidate category-related cache
   */
  invalidateCategoryCache(): void {
    this.invalidateByPattern('category');
  }

  /**
   * Invalidate billing-related cache
   */
  invalidateBillingCache(): void {
    this.invalidateByPattern('billing');
  }
}
