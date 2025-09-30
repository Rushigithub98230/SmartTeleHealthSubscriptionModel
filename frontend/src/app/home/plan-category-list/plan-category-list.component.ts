import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { PlanCategoryService } from './plan-category.service';
import { PlanCategoryAuthService } from './plan-category-auth.service';
import { SubscriptionService, SubscriptionPlan, Category as BackendCategory, BillingCycle } from '../../services/subscription.service';
import { HeaderComponent } from '../header/header.component';
import { QuestionarePopupComponent } from './Component/questionare-popup/questionare-popup.component';
import { LoginPopupOurPlanComponent } from './Component/login-popup-our-plan/login-popup-our-plan.component';

// Using SubscriptionPlan interface from subscription service

export interface Question {
  id: string;
  text: string;
  type: 'text' | 'select' | 'radio' | 'checkbox' | 'textarea' | 'number';
  options?: string[];
  required: boolean;
  placeholder?: string;
  templateId?: string;
}

export interface CategoryQuestions {
  [categoryId: string]: Question[];
}

export interface FormData {
  categoryId: string;
  planId: string;
  answers: { [questionId: string]: any };
  selectedPlan?: any;
  fromTrending?: boolean;
}

export interface User {
  email: string;
  name: string;
  accessToken: string;
}

// Using Category interface from subscription service

@Component({
  selector: "app-plan-category-list",
  standalone: true,
  imports: [CommonModule, FormsModule, HeaderComponent, QuestionarePopupComponent, LoginPopupOurPlanComponent],
  templateUrl: "./plan-category-list.component.html",
  styleUrl: "./plan-category-list.component.css",
})
export class PlanCategoryListComponent implements OnInit, OnDestroy {
  selectedCategory: BackendCategory | null = null;
  activeTab: "categories" | "trending" = "categories";
  
  // Data from backend
  backendPlans: SubscriptionPlan[] = [];
  backendCategories: BackendCategory[] = [];
  billingCycles: BillingCycle[] = [];
  private subscriptions: Subscription[] = [];
  
  // Loading states
  isLoadingPlans = false;
  isLoadingCategories = false;
  errorMessage = '';

  // Stepper form state
  currentStep: "overview" | "questions" | "plans" | "login" | "payment" =
    "overview";
  showQuestionPopup = false;
  showLoginPopup = false;
  formData: FormData = {
    categoryId: "",
    planId: "",
    answers: {},
    fromTrending: false,
  };
  
  // Questionnaire state
  currentQuestions: Question[] = [];
  questionnaireTemplateId: string = '';

  constructor(
    private questionsService: PlanCategoryService,
    private authService: PlanCategoryAuthService,
    public subscriptionService: SubscriptionService
  ) {}

  ngOnInit() {
    this.loadData();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadData() {
    this.loadPlans();
    this.loadCategories();
    this.loadBillingCycles();
  }

  loadPlans() {
    this.isLoadingPlans = true;
    this.errorMessage = '';
    
    const subscription = this.subscriptionService.getActivePlans().subscribe({
      next: (plans: SubscriptionPlan[]) => {
        this.backendPlans = plans || [];
        console.log('Loaded plans:', this.backendPlans);
        this.isLoadingPlans = false;
      },
      error: (error) => {
        console.error('Failed to load plans:', error);
        this.errorMessage = 'Failed to load subscription plans';
        this.isLoadingPlans = false;
      }
    });
    
    this.subscriptions.push(subscription);
  }

  loadCategories() {
    this.isLoadingCategories = true;
    this.errorMessage = '';
    
    const subscription = this.subscriptionService.getCategories().subscribe({
      next: (categories: BackendCategory[]) => {
        this.backendCategories = categories || [];
        console.log('Loaded categories:', this.backendCategories);
        this.isLoadingCategories = false;
      },
      error: (error) => {
        console.error('Failed to load categories:', error);
        this.errorMessage = 'Failed to load categories';
        this.isLoadingCategories = false;
      }
    });
    
    this.subscriptions.push(subscription);
  }

  loadBillingCycles() {
    const subscription = this.subscriptionService.getBillingCycles().subscribe({
      next: (cycles: BillingCycle[]) => {
        this.billingCycles = cycles || [];
        console.log('Loaded billing cycles:', this.billingCycles);
      },
      error: (error) => {
        console.error('Failed to load billing cycles:', error);
        // Fallback to hardcoded cycles if API fails
        this.billingCycles = [
          { id: 'monthly', name: 'Monthly', durationInMonths: 1, isActive: true, displayOrder: 1 },
          { id: 'quarterly', name: 'Quarterly', durationInMonths: 3, isActive: true, displayOrder: 2 },
          { id: 'annual', name: 'Annual', durationInMonths: 12, isActive: true, displayOrder: 3 }
        ];
      }
    });
    
    this.subscriptions.push(subscription);
  }

  // Removed convertBackendPlansToUI - using backend data directly

  parseFeatures(featuresString?: string): string[] {
    if (!featuresString) return [];
    try {
      return JSON.parse(featuresString);
    } catch {
      // If not JSON, split by newlines or commas
      return featuresString.split(/[\n,]/).map(f => f.trim()).filter(f => f.length > 0);
    }
  }

  get categories(): BackendCategory[] {
    // Use only backend data - no fallback to static data
    return this.backendCategories || [];
  }

  get plansForSelectedCategory(): SubscriptionPlan[] {
    if (!this.formData.categoryId) return [];
    
    // Use only backend data
    return this.backendPlans.filter(plan => plan.categoryId === this.formData.categoryId);
  }

  getQuestionsForCurrentCategory() {
    return this.questionsService.getQuestionsForCategory(
      this.formData.categoryId
    );
  }

  get trendingPlans(): SubscriptionPlan[] {
    // Use only backend data
    return this.backendPlans.filter(plan => plan.isTrending);
  }

  get currentStepNumber(): number {
    const stepMap = {
      overview: 1,
      questions: 2,
      plans: 3,
      login: 4,
      payment: 5,
    };
    return stepMap[this.currentStep];
  }

  get selectedPlanForDisplay(): SubscriptionPlan | null {
    if (this.formData.selectedPlan) {
      return this.formData.selectedPlan;
    }

    const category = this.categories.find(
      (cat) => cat.id === this.formData.categoryId
    );
    if (category) {
      return (
        this.backendPlans.find((plan) => plan.id === this.formData.planId) || null
      );
    }

    return null;
  }

  // Navigation methods
  handleCategoryClick(category: BackendCategory): void {
    if (this.currentStep === "overview") {
      this.selectedCategory = category;
    }
  }

  handleBackToCategories(): void {
    this.selectedCategory = null;
  }

  setActiveTab(tab: "categories" | "trending"): void {
    this.activeTab = tab;
  }

  getCategoryForPlan(planId: string): BackendCategory | undefined {
    const plan = this.backendPlans.find(p => p.id === planId);
    if (plan) {
      return this.categories.find(category => category.id === plan.categoryId);
    }
    return undefined;
  }

  // Stepper form methods
  onSelectPlan(plan: SubscriptionPlan): void {
    console.log('Plan selected from category view:', plan);
    
    // Validate plan data
    if (!plan || !plan.id || !plan.categoryId) {
      console.error('Invalid plan data');
      this.errorMessage = 'Invalid plan selected. Please try again.';
      return;
    }
    
    this.formData.categoryId = plan.categoryId;
    this.formData.planId = plan.id;
    this.formData.selectedPlan = { ...plan, billingCycleId: plan.billingCycleId || 'monthly' };
    this.formData.fromTrending = false;
    this.formData.answers = {}; // Reset answers when selecting new plan

    // Load questions from backend for this category
    this.loadQuestionsForCategory(plan.categoryId);
  }

  handlePlanSelect(plan: SubscriptionPlan): void {
    console.log('Plan selected from trending view:', plan);
    
    // Validate plan data
    if (!plan || !plan.id || !plan.categoryId) {
      console.error('Invalid plan data');
      this.errorMessage = 'Invalid plan selected. Please try again.';
      return;
    }
    
    this.formData.categoryId = plan.categoryId;
    this.formData.planId = plan.id;
    this.formData.selectedPlan = { ...plan, billingCycleId: plan.billingCycleId || 'monthly' };
    this.formData.fromTrending = true;
    this.formData.answers = {}; // Reset answers when selecting new plan

    // Load questions from backend for this category
    this.loadQuestionsForCategory(plan.categoryId);
  }

  loadQuestionsForCategory(categoryId: string): void {
    console.log('Loading questions for category:', categoryId);
    
    const subscription = this.questionsService.getQuestionsForCategory(categoryId).subscribe({
      next: (questions: Question[]) => {
        console.log('Questions loaded:', questions);
        this.currentQuestions = questions;
        
        // Set questionnaire template ID from the first question if available
        if (questions.length > 0 && questions[0].templateId) {
          this.questionnaireTemplateId = questions[0].templateId;
        } else {
          this.questionnaireTemplateId = '';
        }
        
        this.showQuestionPopup = true;
        this.currentStep = "questions";
      },
      error: (error) => {
        console.error('Error loading questions:', error);
        this.errorMessage = 'Failed to load questions. Please try again.';
        this.currentQuestions = [];
        this.questionnaireTemplateId = '';
        this.showQuestionPopup = false; // Don't show empty popup
      }
    });
    
    this.subscriptions.push(subscription);
  }

  onQuestionsComplete(answers: { [key: string]: any }): void {
    this.formData.answers = answers;
    
    // Submit questionnaire response to backend
    if (this.questionnaireTemplateId) {
      const subscription = this.questionsService.submitQuestionnaireResponse(
        this.questionnaireTemplateId,
        answers,
        this.formData.planId,
        this.formData.categoryId
      ).subscribe({
        next: (response) => {
          console.log('Questionnaire response submitted:', response);
          this.showQuestionPopup = false;
          this.currentStep = "plans";
        },
        error: (error) => {
          console.error('Error submitting questionnaire response:', error);
          // Continue with the flow even if submission fails
          this.showQuestionPopup = false;
          this.currentStep = "plans";
        }
      });
      
      this.subscriptions.push(subscription);
    } else {
      // No template ID, just continue
      this.showQuestionPopup = false;
      this.currentStep = "plans";
    }
  }

  onQuestionsClose(): void {
    this.showQuestionPopup = false;
    this.currentStep = "overview";
    this.resetFormData();
  }

  onPlanSelection(plan: SubscriptionPlan): void {
    console.log('Plan selection from plans step:', plan);
    
    this.formData.planId = plan.id;
    this.formData.selectedPlan = { ...plan, billingCycleId: plan.billingCycleId || 'monthly' };
    
    this.proceedToNextStep();
  }

  proceedToNextStep(): void {
    if (this.authService.isAuthenticated()) {
      this.currentStep = "payment";
    } else {
      this.currentStep = "login";
      this.showLoginPopup = true;
    }
  }

  onLoginSuccess(): void {
    this.showLoginPopup = false;
    this.currentStep = "payment";
  }

  onLoginClose(): void {
    this.showLoginPopup = false;
    this.currentStep = "plans";
  }

  onPayment(): void {
    if (!this.formData.selectedPlan) {
      console.error('No plan selected for payment');
      this.errorMessage = 'No plan selected. Please select a plan first.';
      return;
    }

    // Get the selected plan details
    const selectedPlan = this.formData.selectedPlan;
    const billingCycleId = selectedPlan.billingCycleId || 'monthly'; // Default to monthly

    // Validate required fields
    if (!selectedPlan.id) {
      console.error('Plan ID is missing');
      this.errorMessage = 'Plan information is incomplete. Please try again.';
      return;
    }

    // Validate billing cycle
    const validBillingCycle = this.billingCycles.find(cycle => cycle.id === billingCycleId);
    if (!validBillingCycle) {
      console.error('Invalid billing cycle');
      this.errorMessage = 'Invalid billing cycle selected. Please try again.';
      return;
    }

    // Validate plan price
    if (!selectedPlan.price || selectedPlan.price <= 0) {
      console.error('Invalid plan price');
      this.errorMessage = 'Invalid plan price. Please contact support.';
      return;
    }

    // Create checkout session request
    const checkoutRequest = {
      planId: selectedPlan.id,
      billingCycleId: billingCycleId,
      successUrl: `${window.location.origin}/subscription/success?session_id={CHECKOUT_SESSION_ID}`,
      cancelUrl: `${window.location.origin}/subscription/cancel`,
      questionnaireResponses: this.formData.answers,
      categoryId: this.formData.categoryId
    };

    console.log('Creating checkout session for:', checkoutRequest);
    console.log('Selected plan details:', selectedPlan);

    // Create Stripe checkout session
    const subscription = this.subscriptionService.createCheckoutSession(checkoutRequest).subscribe({
      next: (response) => {
        console.log('Checkout session created successfully:', response);
        if (response.url) {
          // Redirect to Stripe checkout
          window.location.href = response.url;
        } else {
          console.error('No checkout URL received');
          this.errorMessage = 'Failed to get checkout URL. Please try again.';
        }
      },
      error: (error) => {
        console.error('Error creating checkout session:', error);
        let errorMessage = 'Failed to create checkout session. Please try again.';
        
        if (error.message) {
          errorMessage = error.message;
        } else if (error.error && error.error.message) {
          errorMessage = error.error.message;
        }
        
        this.errorMessage = errorMessage;
      }
    });
    
    this.subscriptions.push(subscription);
  }

  resetFormData(): void {
    this.formData = {
      categoryId: "",
      planId: "",
      answers: {},
      fromTrending: false,
    };
    this.selectedCategory = null;
    this.clearErrorMessage();
  }

  clearErrorMessage(): void {
    this.errorMessage = '';
  }

  backToOverview(): void {
    this.resetFormData();
    this.currentStep = "overview";
  }

  backToPlans(): void {
    this.currentStep = "plans";
  }

  // Billing cycle methods
  onBillingCycleChange(plan: SubscriptionPlan, event: any): void {
    const selectedCycleId = event.target.value;
    
    // Create a local copy to modify without affecting the original plan data
    const updatedPlan = { ...plan, billingCycleId: selectedCycleId };
    
    // If this is the selected plan, update it in formData
    if (this.formData.planId === plan.id) {
      this.formData.selectedPlan = updatedPlan;
    }
    
    console.log('Billing cycle changed to:', selectedCycleId, 'for plan:', plan.name);
  }

  calculatePriceForCycle(plan: SubscriptionPlan, cycleId: string): string {
    const cycle = this.billingCycles.find(c => c.id === cycleId);
    if (!cycle) return this.subscriptionService.formatPrice(plan.price);
    
    const calculatedPrice = plan.price * cycle.durationInMonths;
    return this.subscriptionService.formatPrice(calculatedPrice);
  }

  // Helper methods
  getPlansForCategory(categoryId: string): SubscriptionPlan[] {
    return this.backendPlans.filter(plan => plan.categoryId === categoryId);
  }

  // Legacy methods for compatibility
  onLearnMore(plan: SubscriptionPlan): void {
    console.log("Learn more about plan:", plan);
  }
}
