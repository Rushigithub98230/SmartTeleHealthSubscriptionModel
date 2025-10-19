import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';
import { LoginDto, RegisterDto, LoginResponseDto, UserDto } from '../models';

/**
 * Authentication Service
 * Uses CommonService for all HTTP calls - NO direct HttpClient usage
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<UserDto | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private tokenSubject = new BehaviorSubject<string | null>(null);
  public token$ = this.tokenSubject.asObservable();

  constructor(private commonService: CommonService) {
    // Load user from localStorage on init
    this.loadUserFromStorage();
  }

  /**
   * User Login
   * API: POST /api/Auth/login
   */
  login(credentials: LoginDto): Observable<ApiResponse<LoginResponseDto>> {
    return this.commonService.post<LoginResponseDto>('Auth/login', credentials).pipe(
      tap(response => {
        if (response.statusCode === 200 && response.data) {
          this.storeAuthData(response.data);
        }
      })
    );
  }

  /**
   * User Registration
   * API: POST /api/Auth/register
   */
  register(userData: RegisterDto): Observable<ApiResponse<UserDto>> {
    return this.commonService.post<UserDto>('Auth/register', userData);
  }

  /**
   * Logout
   */
  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
    this.tokenSubject.next(null);
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return !!this.tokenSubject.value;
  }

  /**
   * Check if user is admin
   */
  isAdmin(): boolean {
    const user = this.currentUserSubject.value;
    return user?.role === 'Admin' || user?.userType === 'Admin';
  }

  /**
   * Get current user
   */
  getCurrentUser(): UserDto | null {
    return this.currentUserSubject.value;
  }

  /**
   * Get current token
   */
  getToken(): string | null {
    return this.tokenSubject.value;
  }

  /**
   * Store authentication data
   */
  private storeAuthData(loginResponse: LoginResponseDto): void {
    localStorage.setItem('token', loginResponse.token);
    localStorage.setItem('refreshToken', loginResponse.refreshToken);
    localStorage.setItem('user', JSON.stringify(loginResponse.user));
    
    this.tokenSubject.next(loginResponse.token);
    this.currentUserSubject.next(loginResponse.user);
  }

  /**
   * Load user from storage
   */
  private loadUserFromStorage(): void {
    const token = localStorage.getItem('token');
    const userJson = localStorage.getItem('user');

    if (token && userJson) {
      try {
        const user = JSON.parse(userJson) as UserDto;
        this.tokenSubject.next(token);
        this.currentUserSubject.next(user);
      } catch (error) {
        console.error('Error loading user from storage:', error);
        this.logout();
      }
    }
  }
}


