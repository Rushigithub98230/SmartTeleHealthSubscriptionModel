import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-skeleton-loading',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="skeleton-container" [class]="containerClass">
      <!-- Table Skeleton -->
      <div *ngIf="type === 'table'" class="skeleton-table">
        <div class="skeleton-header">
          <div *ngFor="let col of columns" class="skeleton-header-cell"></div>
        </div>
        <div *ngFor="let row of rows" class="skeleton-row">
          <div *ngFor="let col of columns" class="skeleton-cell"></div>
        </div>
      </div>

      <!-- Card Skeleton -->
      <div *ngIf="type === 'card'" class="skeleton-card">
        <div class="skeleton-card-header">
          <div class="skeleton-avatar"></div>
          <div class="skeleton-text-group">
            <div class="skeleton-text skeleton-text--title"></div>
            <div class="skeleton-text skeleton-text--subtitle"></div>
          </div>
        </div>
        <div class="skeleton-card-content">
          <div class="skeleton-text skeleton-text--line"></div>
          <div class="skeleton-text skeleton-text--line"></div>
          <div class="skeleton-text skeleton-text--line skeleton-text--short"></div>
        </div>
        <div class="skeleton-card-actions">
          <div class="skeleton-button"></div>
          <div class="skeleton-button"></div>
        </div>
      </div>

      <!-- List Skeleton -->
      <div *ngIf="type === 'list'" class="skeleton-list">
        <div *ngFor="let item of listItems" class="skeleton-list-item">
          <div class="skeleton-avatar"></div>
          <div class="skeleton-content">
            <div class="skeleton-text skeleton-text--title"></div>
            <div class="skeleton-text skeleton-text--subtitle"></div>
          </div>
          <div class="skeleton-actions">
            <div class="skeleton-button"></div>
          </div>
        </div>
      </div>

      <!-- Plan Card Skeleton -->
      <div *ngIf="type === 'plan-card'" class="skeleton-plan-card">
        <div class="skeleton-plan-header">
          <div class="skeleton-text skeleton-text--title"></div>
          <div class="skeleton-badge"></div>
        </div>
        <div class="skeleton-plan-price">
          <div class="skeleton-text skeleton-text--price"></div>
          <div class="skeleton-text skeleton-text--billing"></div>
        </div>
        <div class="skeleton-plan-features">
          <div *ngFor="let feature of planFeatures" class="skeleton-feature">
            <div class="skeleton-icon"></div>
            <div class="skeleton-text skeleton-text--feature"></div>
          </div>
        </div>
        <div class="skeleton-plan-actions">
          <div class="skeleton-button skeleton-button--primary"></div>
        </div>
      </div>

      <!-- Category Card Skeleton -->
      <div *ngIf="type === 'category-card'" class="skeleton-category-card">
        <div class="skeleton-category-icon">
          <div class="skeleton-icon skeleton-icon--large"></div>
        </div>
        <div class="skeleton-category-content">
          <div class="skeleton-text skeleton-text--title"></div>
          <div class="skeleton-text skeleton-text--description"></div>
          <div class="skeleton-text skeleton-text--count"></div>
        </div>
      </div>

      <!-- Generic Skeleton -->
      <div *ngIf="type === 'generic'" class="skeleton-generic">
        <div *ngFor="let line of genericLines" 
             class="skeleton-text" 
             [class]="'skeleton-text--' + line.type">
        </div>
      </div>
    </div>
  `,
  styles: [`
    .skeleton-container {
      width: 100%;
      animation: skeleton-loading 1.5s ease-in-out infinite;
    }

    @keyframes skeleton-loading {
      0% {
        opacity: 1;
      }
      50% {
        opacity: 0.4;
      }
      100% {
        opacity: 1;
      }
    }

    /* Base skeleton elements */
    .skeleton-text,
    .skeleton-button,
    .skeleton-avatar,
    .skeleton-icon,
    .skeleton-badge,
    .skeleton-header-cell,
    .skeleton-cell {
      background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
      background-size: 200% 100%;
      animation: skeleton-shimmer 1.5s infinite;
      border-radius: 4px;
    }

    @keyframes skeleton-shimmer {
      0% {
        background-position: -200% 0;
      }
      100% {
        background-position: 200% 0;
      }
    }

    /* Text variations */
    .skeleton-text {
      height: 16px;
      margin-bottom: 8px;
    }

    .skeleton-text--title {
      height: 20px;
      width: 60%;
    }

    .skeleton-text--subtitle {
      height: 14px;
      width: 40%;
    }

    .skeleton-text--line {
      width: 100%;
    }

    .skeleton-text--short {
      width: 70%;
    }

    .skeleton-text--price {
      height: 24px;
      width: 50%;
    }

    .skeleton-text--billing {
      height: 12px;
      width: 30%;
    }

    .skeleton-text--feature {
      height: 14px;
      width: 80%;
    }

    .skeleton-text--description {
      height: 16px;
      width: 90%;
    }

    .skeleton-text--count {
      height: 12px;
      width: 25%;
    }

    /* Button variations */
    .skeleton-button {
      height: 36px;
      width: 100px;
      border-radius: 18px;
    }

    .skeleton-button--primary {
      height: 40px;
      width: 120px;
    }

    /* Avatar and icons */
    .skeleton-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
    }

    .skeleton-icon {
      width: 16px;
      height: 16px;
      border-radius: 2px;
    }

    .skeleton-icon--large {
      width: 48px;
      height: 48px;
      border-radius: 8px;
    }

    .skeleton-badge {
      width: 60px;
      height: 20px;
      border-radius: 10px;
    }

    /* Table skeleton */
    .skeleton-table {
      width: 100%;
    }

    .skeleton-header {
      display: flex;
      gap: 16px;
      margin-bottom: 16px;
    }

    .skeleton-header-cell {
      height: 20px;
      flex: 1;
    }

    .skeleton-row {
      display: flex;
      gap: 16px;
      margin-bottom: 12px;
    }

    .skeleton-cell {
      height: 16px;
      flex: 1;
    }

    /* Card skeleton */
    .skeleton-card {
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      padding: 16px;
      margin-bottom: 16px;
    }

    .skeleton-card-header {
      display: flex;
      align-items: center;
      margin-bottom: 16px;
    }

    .skeleton-text-group {
      flex: 1;
      margin-left: 12px;
    }

    .skeleton-card-content {
      margin-bottom: 16px;
    }

    .skeleton-card-actions {
      display: flex;
      gap: 8px;
    }

    /* List skeleton */
    .skeleton-list {
      width: 100%;
    }

    .skeleton-list-item {
      display: flex;
      align-items: center;
      padding: 12px 0;
      border-bottom: 1px solid #f0f0f0;
    }

    .skeleton-content {
      flex: 1;
      margin-left: 12px;
    }

    .skeleton-actions {
      margin-left: 12px;
    }

    /* Plan card skeleton */
    .skeleton-plan-card {
      border: 1px solid #e0e0e0;
      border-radius: 12px;
      padding: 24px;
      margin-bottom: 16px;
      text-align: center;
    }

    .skeleton-plan-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
    }

    .skeleton-plan-price {
      margin-bottom: 24px;
    }

    .skeleton-plan-features {
      margin-bottom: 24px;
    }

    .skeleton-feature {
      display: flex;
      align-items: center;
      margin-bottom: 8px;
    }

    .skeleton-feature .skeleton-icon {
      margin-right: 8px;
    }

    /* Category card skeleton */
    .skeleton-category-card {
      border: 1px solid #e0e0e0;
      border-radius: 12px;
      padding: 20px;
      text-align: center;
      margin-bottom: 16px;
    }

    .skeleton-category-icon {
      margin-bottom: 16px;
    }

    .skeleton-category-content {
      .skeleton-text--title {
        margin-bottom: 8px;
      }

      .skeleton-text--description {
        margin-bottom: 12px;
      }
    }

    /* Generic skeleton */
    .skeleton-generic {
      .skeleton-text {
        margin-bottom: 12px;
      }
    }

    /* Responsive adjustments */
    @media (max-width: 768px) {
      .skeleton-card,
      .skeleton-plan-card,
      .skeleton-category-card {
        padding: 12px;
      }

      .skeleton-plan-card {
        text-align: left;
      }

      .skeleton-plan-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 8px;
      }
    }

    /* Dark theme support */
    @media (prefers-color-scheme: dark) {
      .skeleton-text,
      .skeleton-button,
      .skeleton-avatar,
      .skeleton-icon,
      .skeleton-badge,
      .skeleton-header-cell,
      .skeleton-cell {
        background: linear-gradient(90deg, #2a2a2a 25%, #3a3a3a 50%, #2a2a2a 75%);
        background-size: 200% 100%;
      }

      .skeleton-card,
      .skeleton-plan-card,
      .skeleton-category-card {
        border-color: #404040;
      }

      .skeleton-list-item {
        border-bottom-color: #404040;
      }
    }
  `]
})
export class SkeletonLoadingComponent {
  @Input() type: 'table' | 'card' | 'list' | 'plan-card' | 'category-card' | 'generic' = 'generic';
  @Input() columns: number = 4;
  @Input() rows: number = 5;
  @Input() listItems: number = 5;
  @Input() planFeatures: number = 4;
  @Input() genericLines: Array<{type: string}> = [
    { type: 'title' },
    { type: 'line' },
    { type: 'line' },
    { type: 'short' }
  ];
  @Input() containerClass: string = '';

  constructor() {}
}
