import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from '../../../core/services';
import { UserDto } from '../../../core/models';

/**
 * Global Navbar Component
 * Responsive navigation header
 * 
 * Usage:
 * <app-navbar></app-navbar>
 */
@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {
  currentUser: UserDto | null = null;
  isAuthenticated = false;
  isAdmin = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Subscribe to user changes
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
      this.isAuthenticated = this.authService.isAuthenticated();
      this.isAdmin = this.authService.isAdmin();
    });
  }

  /**
   * Logout user
   */
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }
}

