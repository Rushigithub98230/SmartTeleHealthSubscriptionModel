import { Injectable } from '@angular/core';
import { User } from './plan-category-list.component';
import { BehaviorSubject, Observable } from 'rxjs';
import { AuthService as RealAuthService, AdminUser } from '../../admin/auth/auth.service';
import { map, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PlanCategoryAuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private realAuthService: RealAuthService) {
    this.checkExistingAuth();
  }

  private checkExistingAuth(): void {
    // Use the real auth service to check authentication status
    if (this.realAuthService.isAuthenticated()) {
      const adminUser = this.realAuthService.getCurrentUser();
      if (adminUser) {
        const user: User = {
          email: adminUser.email,
          name: `${adminUser.firstName} ${adminUser.lastName}`.trim(),
          accessToken: this.realAuthService.getToken() || ''
        };
        this.currentUserSubject.next(user);
      }
    }
  }

  isAuthenticated(): boolean {
    return this.realAuthService.isAuthenticated();
  }

  login(email: string, password: string): Observable<boolean> {
    return this.realAuthService.login({ email, password }).pipe(
      map(response => {
        if (response.statusCode === 200 && response.data) {
          const adminUser = response.data.user;
          const user: User = {
            email: adminUser.email,
            name: `${adminUser.firstName} ${adminUser.lastName}`.trim(),
            accessToken: response.data.token
          };
          this.currentUserSubject.next(user);
          return true;
        }
        return false;
      }),
      catchError(error => {
        console.error('Login error:', error);
        this.currentUserSubject.next(null);
        throw error;
      })
    );
  }

  logout(): void {
    this.realAuthService.logout();
    this.currentUserSubject.next(null);
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }
}