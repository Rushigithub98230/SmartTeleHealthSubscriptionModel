import { Injectable } from '@angular/core';
import { User } from './plan-category-list.component';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';

@Injectable({
  providedIn: 'root'
})
export class PlanCategoryAuthService {
 private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor() {
    this.checkExistingAuth();
  }

  private checkExistingAuth(): void {
    const token = localStorage.getItem('access-token');
    const userData = localStorage.getItem('user-data');
    
    if (token && userData) {
      try {
        const user = JSON.parse(userData);
        user.accessToken = token;
        this.currentUserSubject.next(user);
      } catch (error) {
        this.logout();
      }
    }
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('access-token');
  }

  login(email: string, password: string): Promise<boolean> {
    // Simulate login API call
    return new Promise((resolve) => {
      setTimeout(() => {
        if (email && password) {
          const user: User = {
            email: email,
            name: email.split('@')[0],
            accessToken: 'mock-token-' + Date.now()
          };
          
          localStorage.setItem('access-token', user.accessToken);
          localStorage.setItem('user-data', JSON.stringify({
            email: user.email,
            name: user.name
          }));
          
          this.currentUserSubject.next(user);
          resolve(true);
        } else {
          resolve(false);
        }
      }, 1000);
    });
  }

  logout(): void {
    localStorage.removeItem('access-token');
    localStorage.removeItem('user-data');
    this.currentUserSubject.next(null);
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }
}