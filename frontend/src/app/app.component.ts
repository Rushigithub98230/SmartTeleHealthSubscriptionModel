import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { AuthService } from './admin/auth/auth.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  template: '<router-outlet></router-outlet>'
})
export class AppComponent implements OnInit {
  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit() {
    // Handle navigation and redirect logic
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event) => {
        if (event instanceof NavigationEnd) {
          this.handleNavigation(event.url);
        }
      });
  }

  private handleNavigation(url: string) {
    // If user is on home page and is admin, redirect to admin portal
    if (url === '/' || url === '/home') {
      if (this.authService.shouldRedirectToAdmin()) {
        this.router.navigate(['/admin/subscriptions']);
      }
    }
    // If user is trying to access admin routes but not authenticated, redirect to login
    // BUT exclude login and register pages from this redirect
    else if (url.startsWith('/admin') && 
             !url.includes('/admin/login') && 
             !url.includes('/admin/register') && 
             !this.authService.isAuthenticated()) {
      this.router.navigate(['/admin/login']);
    }
  }
}
