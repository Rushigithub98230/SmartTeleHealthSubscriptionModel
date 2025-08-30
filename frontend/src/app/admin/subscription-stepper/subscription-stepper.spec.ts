import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SubscriptionStepper } from './subscription-stepper';

describe('SubscriptionStepper', () => {
  let component: SubscriptionStepper;
  let fixture: ComponentFixture<SubscriptionStepper>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SubscriptionStepper]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SubscriptionStepper);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
