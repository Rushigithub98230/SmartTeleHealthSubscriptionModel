import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { Router } from '@angular/router';
import { tap, catchError, map } from 'rxjs/operators';

export interface AdminUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  permissions: string[];
}

export interface LoginResponse {
  statusCode: number;
  message: string;
  data: {
    token: string;
    user: AdminUser;
  };
}

export interface RefreshTokenResponse {
  statusCode: number;
  message: string;
  data: {
    token: string;
  };
}

export interface ValidateTokenResponse {
  valid: boolean;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  
  private baseUrl = 'http://localhost:61376/api/auth';
  
  // BehaviorSubject to track authentication state
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  private currentUserSubject = new BehaviorSubject<AdminUser | null>(null);
  
  // Public observables
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor() {
    // Check if user is already authenticated on service initialization
    this.checkAuthStatus();
  }

  /**
   * Check if user is already authenticated
   */
  private checkAuthStatus(): void {
    const token = this.getToken();
    const user = this.getCurrentUser();
    
    console.log('Checking auth status:', { hasToken: !!token, hasUser: !!user });
    
    if (token && user) {
      this.isAuthenticatedSubject.next(true);
      this.currentUserSubject.next(user);
      console.log('User is authenticated:', user);
    } else {
      this.clearAuth();
      console.log('User is not authenticated');
    }
  }

  /**
   * Login user
   */
  login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    console.log('Attempting login with:', credentials);
    
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, credentials).pipe(
      tap(response => {
        console.log('Login response received:', response);
        if (response?.data?.token && response?.data?.user) {
          this.setAuth(response.data.token, response.data.user);
          console.log('Login successful, token stored:', response.data.token.substring(0, 20) + '...');
          this.router.navigate(['/admin/dashboard']);
        }
      }),
      catchError(error => {
        console.error('Login error:', error);
        this.clearAuth();
        throw error;
      })
    );
  }

  /**
   * Register new admin user
   */
  register(userData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, userData);
  }

  /**
   * Logout user
   */
  logout(): void {
    console.log('Logging out user');
    this.clearAuth();
    this.router.navigate(['/admin/login']);
  }

  /**
   * Check if user has specific permission
   */
  hasPermission(permission: string): boolean {
    const user = this.getCurrentUser();
    return user?.permissions?.includes(permission) || false;
  }

  /**
   * Check if user has any of the specified permissions
   */
  hasAnyPermission(permissions: string[]): boolean {
    const user = this.getCurrentUser();
    return user?.permissions?.some(permission => permissions.includes(permission)) || false;
  }

  /**
   * Get current authentication token
   */
  getToken(): string | null {
    const token = localStorage.getItem('adminToken');
    console.log('Getting token:', token ? token.substring(0, 20) + '...' : 'No token');
    return token;
  }

  /**
   * Get current user
   */
  getCurrentUser(): AdminUser | null {
    const userStr = localStorage.getItem('adminUser');
    const user = userStr ? JSON.parse(userStr) : null;
    console.log('Getting current user:', user);
    return user;
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    const authenticated = this.isAuthenticatedSubject.value;
    console.log('Checking if authenticated:', authenticated);
    return authenticated;
  }

  /**
   * Set authentication data
   */
  private setAuth(token: string, user: AdminUser): void {
    console.log('Setting auth data:', { token: token.substring(0, 20) + '...', user });
    localStorage.setItem('adminToken', token);
    localStorage.setItem('adminUser', JSON.stringify(user));
    this.isAuthenticatedSubject.next(true);
    this.currentUserSubject.next(user);
  }

  /**
   * Clear authentication data
   */
  private clearAuth(): void {
    console.log('Clearing auth data');
    localStorage.removeItem('adminToken');
    localStorage.removeItem('adminUser');
    this.isAuthenticatedSubject.next(false);
    this.currentUserSubject.next(null);
  }

  /**
   * Refresh token (if needed)
   */
  refreshToken(): Observable<any> {
    const token = this.getToken();
    if (!token) {
      return of(null);
    }
    
    return this.http.post<RefreshTokenResponse>(`${this.baseUrl}/refresh-token`, { token }).pipe(
      tap(response => {
        if (response?.data?.token) {
          localStorage.setItem('adminToken', response.data.token);
        }
      }),
      catchError(() => {
        this.clearAuth();
        return of(null);
      })
    );
  }

  /**
   * Validate current token
   */
  validateToken(): Observable<boolean> {
    const token = this.getToken();
    if (!token) {
      return of(false);
    }

    return this.http.post<ValidateTokenResponse>(`${this.baseUrl}/validate-token`, { token }).pipe(
      map(response => response.valid),
      tap(isValid => {
        if (!isValid) {
          this.clearAuth();
        }
      }),
      catchError(() => {
        this.clearAuth();
        return of(false);
      })
    );
  }
}
