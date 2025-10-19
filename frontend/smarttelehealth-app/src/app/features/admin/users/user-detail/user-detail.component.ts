import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonService } from '../../../../core/services/common.service';
import { SubscriptionService } from '../../../../core/services';
import { UserDto, SubscriptionDto } from '../../../../core/models';

/**
 * Admin User Detail Component
 * View full user profile and subscription history
 * 
 * APIs Used:
 * - GET /api/Users/{id}
 * - GET /api/Subscriptions/user/{userId}
 * 
 * Route: /webadmin/users/:id
 * Access: Admin only
 */
@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './user-detail.component.html',
  styleUrls: ['./user-detail.component.scss']
})
export class UserDetailComponent implements OnInit {
  userId!: number;
  user: UserDto | null = null;
  subscriptions: SubscriptionDto[] = [];
  loading = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private commonService: CommonService,
    private subscriptionService: SubscriptionService
  ) {}

  ngOnInit(): void {
    this.userId = +this.route.snapshot.params['id'];
    this.loadUser();
    this.loadUserSubscriptions();
  }

  /**
   * Load user details
   * API: GET /api/Users/{id}
   */
  loadUser(): void {
    this.loading = true;

    this.commonService.get<UserDto>(`Users/${this.userId}`).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.user = response.data;
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message;
        this.loading = false;
      }
    });
  }

  /**
   * Load user's subscriptions
   */
  loadUserSubscriptions(): void {
    this.subscriptionService.getUserSubscriptions(this.userId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.subscriptions = response.data;
        }
      },
      error: (error) => console.error('Error loading subscriptions:', error)
    });
  }

  getRoleBadgeClass(role: string): string {
    const map: { [key: string]: string } = {
      'Admin': 'bg-danger',
      'Provider': 'bg-primary',
      'Client': 'bg-success'
    };
    return map[role] || 'bg-secondary';
  }
}


