import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlanCategoryListComponent } from './plan-category-list.component';

describe('PlanCategoryListComponent', () => {
  let component: PlanCategoryListComponent;
  let fixture: ComponentFixture<PlanCategoryListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlanCategoryListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlanCategoryListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
