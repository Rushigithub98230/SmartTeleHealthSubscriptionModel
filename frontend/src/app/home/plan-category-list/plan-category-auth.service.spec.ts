import { TestBed } from '@angular/core/testing';

import { PlanCategoryAuthService } from './plan-category-auth.service';

describe('PlanCategoryAuthService', () => {
  let service: PlanCategoryAuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PlanCategoryAuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
