import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../admin/auth/auth.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {
  isAuthenticated$: Observable<boolean>;
  currentUser$: Observable<any>;

  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    this.isAuthenticated$ = this.authService.isAuthenticated$;
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit() {}

  onLogin() {
    this.router.navigate(['/admin/login']);
  }

  onSignup() {
    this.router.navigate(['/admin/register']);
  }

  onAdminPortal() {
    this.router.navigate(['/admin/subscriptions']);
  }

  onLogout() {
    this.authService.logout();
  }

  onHome() {
    this.router.navigate(['/home']);
  }
}
