import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { SubscriptionPlanService } from '../../../core/services';
import { SubscriptionPlanDto } from '../../../core/models';

/**
 * Marketing Home Page Component
 * Public landing page showcasing telehealth services
 * 
 * APIs Used:
 * - GET /api/SubscriptionPlans/active (featured plans)
 */
@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  featuredPlans: SubscriptionPlanDto[] = [];
  loading = false;

  constructor(private planService: SubscriptionPlanService) {}

  ngOnInit(): void {
    this.loadFeaturedPlans();
  }

  /**
   * Load featured plans for homepage display
   * API: GET /api/SubscriptionPlans/active?page=1&pageSize=3
   */
  loadFeaturedPlans(): void {
    this.loading = true;
    this.planService.getActivePlans(1, 3).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.featuredPlans = response.data;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading featured plans:', error);
        this.loading = false;
      }
    });
  }
}


