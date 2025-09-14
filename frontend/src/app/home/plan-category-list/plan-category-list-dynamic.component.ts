import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { PlanCategoryService } from './plan-category.service';
import { PlanCategoryAuthService } from './plan-category-auth.service';
import { SubscriptionService, SubscriptionPlan, Category as BackendCategory, BillingCycle } from '../../services/subscription.service';

interface Plan {
  id: string;
  name: string;
  price: string;
  features: string[];
  description: string;
  popular?: boolean;
  trending?: boolean;
  categoryId: string;
  billingCycleId?: string;
  actualPrice?: number;
}

export interface Question {
  id: string;
  text: string;
  type: 'text' | 'select' | 'radio' | 'checkbox' | 'textarea' | 'number';
  options?: string[];
  required: boolean;
  placeholder?: string;
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

interface Category {
  id: string;
  name: string;
  icon: string;
  description: string;
  specialties: string[];
  plans: Plan[];
}

@Component({
  selector: "app-plan-category-list",
  templateUrl: "./plan-category-list.component.html",
  styleUrl: "./plan-category-list.component.css",
})
export class PlanCategoryListComponent implements OnInit, OnDestroy {
  selectedCategory: Category | null = null;
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

  constructor(
    private questionsService: PlanCategoryService,
    private authService: PlanCategoryAuthService,
    private subscriptionService: SubscriptionService
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
    
    this.subscriptionService.getActivePlans().subscribe({
      next: (plans) => {
        this.backendPlans = plans || [];
        console.log('Loaded plans:', this.backendPlans);
        this.isLoadingPlans = false;
      },
      error: (error) => {
        console.error('Error loading plans:', error);
        this.errorMessage = 'Error loading subscription plans';
        this.isLoadingPlans = false;
      }
    });
  }

  loadCategories() {
    this.isLoadingCategories = true;
    this.errorMessage = '';
    
    this.subscriptionService.getCategories().subscribe({
      next: (categories) => {
        this.backendCategories = categories || [];
        console.log('Loaded categories:', this.backendCategories);
        this.isLoadingCategories = false;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.errorMessage = 'Error loading categories';
        this.isLoadingCategories = false;
      }
    });
  }

  loadBillingCycles() {
    // For now, use hardcoded billing cycles
    // TODO: Implement getBillingCycles in SubscriptionService
    this.billingCycles = [
      { id: 'monthly', name: 'Monthly', durationInMonths: 1, isActive: true, displayOrder: 1 },
      { id: 'quarterly', name: 'Quarterly', durationInMonths: 3, isActive: true, displayOrder: 2 },
      { id: 'annual', name: 'Annual', durationInMonths: 12, isActive: true, displayOrder: 3 }
    ];
  }

  private convertBackendPlansToUI(plans: SubscriptionPlan[]): Plan[] {
    return plans.map(plan => ({
      id: plan.id,
      name: plan.name,
      price: this.subscriptionService.formatPrice(plan.price),
      description: plan.description || '',
      features: this.parseFeatures(plan.features),
      popular: plan.isMostPopular,
      trending: plan.isTrending,
      categoryId: plan.categoryId,
      billingCycleId: plan.billingCycleId,
      actualPrice: plan.price
    }));
  }

  private parseFeatures(featuresString?: string): string[] {
    if (!featuresString) return [];
    try {
      return JSON.parse(featuresString);
    } catch {
      // If not JSON, split by newlines or commas
      return featuresString.split(/[\n,]/).map(f => f.trim()).filter(f => f.length > 0);
    }
  }

  get categories(): Category[] {
    // Use only backend data - no fallback to static data
    if (this.backendCategories.length > 0) {
      return this.backendCategories.map(category => ({
        id: category.id,
        name: category.name,
        description: category.description || '',
        icon: category.icon || '',
        specialties: [], // Add default specialties
        plans: this.backendPlans.filter(plan => plan.categoryId === category.id)
          .map(plan => this.convertBackendPlansToUI([plan])[0])
      }));
    }
    
    // Return empty array if no backend data
    return [];
  }

  get plansForSelectedCategory(): Plan[] {
    if (!this.formData.categoryId) return [];
    
    // Use only backend data
    if (this.backendPlans.length > 0) {
      const categoryPlans = this.backendPlans.filter(plan => plan.categoryId === this.formData.categoryId);
      return this.convertBackendPlansToUI(categoryPlans);
    }
    
    return [];
  }

  getQuestionsForCurrentCategory() {
    return this.questionsService.getQuestionsForCategory(
      this.formData.categoryId
    );
  }

  get trendingPlans(): Plan[] {
    // Use only backend data
    if (this.backendPlans.length > 0) {
      const trendingPlans = this.backendPlans.filter(plan => plan.isTrending);
      return this.convertBackendPlansToUI(trendingPlans);
    }
    
    return [];
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

  get selectedPlanForDisplay(): Plan | null {
    if (this.formData.selectedPlan) {
      return this.formData.selectedPlan;
    }

    const category = this.categories.find(
      (cat) => cat.id === this.formData.categoryId
    );
    if (category) {
      return (
        category.plans.find((plan) => plan.id === this.formData.planId) || null
      );
    }

    return null;
  }

  // Navigation methods
  handleCategoryClick(category: Category): void {
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

  getCategoryForPlan(planId: string): Category | undefined {
    return this.categories.find((category) =>
      category.plans.some((plan) => plan.id === planId)
    );
  }

  // Stepper form methods
  onSelectPlan(plan: Plan): void {
    this.formData.categoryId = plan.categoryId;
    this.formData.planId = plan.id;
    this.formData.fromTrending = false;

    this.showQuestionPopup = true;
    this.currentStep = "questions";
  }

  handlePlanSelect(plan: Plan): void {
    this.formData.categoryId = plan.categoryId;
    this.formData.planId = plan.id;
    this.formData.selectedPlan = plan;
    this.formData.fromTrending = true;

    this.showQuestionPopup = true;
    this.currentStep = "questions";
  }

  onQuestionsComplete(answers: { [key: string]: any }): void {
    this.formData.answers = answers;
    this.showQuestionPopup = false;
    this.currentStep = "plans";
  }

  onQuestionsClose(): void {
    this.showQuestionPopup = false;
    this.currentStep = "overview";
    this.resetFormData();
  }

  onPlanSelection(plan: Plan): void {
    this.formData.planId = plan.id;
    this.formData.selectedPlan = plan;
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
      alert('No plan selected');
      return;
    }

    // Get the selected plan details
    const selectedPlan = this.formData.selectedPlan;
    const billingCycleId = selectedPlan.billingCycleId || 'monthly'; // Default to monthly

    // Create checkout session request
    const checkoutRequest = {
      planId: selectedPlan.id,
      billingCycleId: billingCycleId,
      successUrl: `${window.location.origin}/subscription/success?session_id={CHECKOUT_SESSION_ID}`,
      cancelUrl: `${window.location.origin}/subscription/cancel`
    };

    console.log('Creating checkout session for:', checkoutRequest);

    // Create Stripe checkout session
    this.subscriptionService.createCheckoutSession(checkoutRequest).subscribe({
      next: (response) => {
        console.log('Checkout session created:', response);
        // Redirect to Stripe checkout
        window.location.href = response.url;
      },
      error: (error) => {
        console.error('Error creating checkout session:', error);
        alert('Failed to create checkout session. Please try again.');
      }
    });
  }

  resetFormData(): void {
    this.formData = {
      categoryId: "",
      planId: "",
      answers: {},
      fromTrending: false,
    };
    this.selectedCategory = null;
  }

  backToOverview(): void {
    this.resetFormData();
    this.currentStep = "overview";
  }

  backToPlans(): void {
    this.currentStep = "plans";
  }

  // Billing cycle methods
  onBillingCycleChange(plan: Plan, event: any): void {
    const selectedCycleId = event.target.value;
    plan.billingCycleId = selectedCycleId;
    
    // Update the price display
    const selectedCycle = this.billingCycles.find(c => c.id === selectedCycleId);
    if (selectedCycle && plan.actualPrice) {
      const newPrice = plan.actualPrice * selectedCycle.durationInMonths;
      plan.price = this.subscriptionService.formatPrice(newPrice);
    }
  }

  calculatePriceForCycle(plan: Plan, cycleId: string): string {
    const cycle = this.billingCycles.find(c => c.id === cycleId);
    if (!cycle || !plan.actualPrice) return plan.price;
    
    const calculatedPrice = plan.actualPrice * cycle.durationInMonths;
    return this.subscriptionService.formatPrice(calculatedPrice);
  }

  // Legacy methods for compatibility
  onLearnMore(plan: Plan): void {
    console.log("Learn more about plan:", plan);
  }
}
