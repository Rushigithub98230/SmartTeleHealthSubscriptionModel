import { TestBed } from '@angular/core/testing';

import { PlanCategoryService } from './plan-category.service';

describe('PlanCategoryService', () => {
  let service: PlanCategoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PlanCategoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
