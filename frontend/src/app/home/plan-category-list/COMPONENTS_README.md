# Plan Category List Components

This README provides the code and overview for all components in the `plan-category-list` feature, including subcomponents in `src/app/front/plan-category-list/Component`.


## Main Component: PlanCategoryListComponent

```typescript
import { Component } from '@angular/core';
import { PlanCategoryService } from './plan-category.service';
import { PlanCategoryAuthService } from './plan-category-auth.service';

interface Plan {
	id: string;
	name: string;
	price: string;
	features: string[];
	description: string;
	popular?: boolean;
	trending?: boolean;
	categoryId: string;
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
export class PlanCategoryListComponent {
	selectedCategory: Category | null = null;
	activeTab: "categories" | "trending" = "categories";
	currentStep: "overview" | "questions" | "plans" | "login" | "payment" = "overview";
	showQuestionPopup = false;
	showLoginPopup = false;
	formData: FormData = {
		categoryId: "",
		planId: "",
		answers: {},
		fromTrending: false,
	};

	categories: Category[] = [
		{
			id: "hair-loss",
			name: "Hair Loss Treatment",
			icon: "../../../../assets/img/vitromd/hair.png",
			description:
				"Comprehensive hair restoration programs with personalized treatment approaches.",
			specialties: ["Dermatology", "Trichology"],
			plans: [
				{
					id: "basic-hair",
					name: "Basic Hair Care",
					price: "$199/month",
					description:
						"Essential hair loss treatment with monthly consultations",
					categoryId: "hair-loss",
					features: [
						"Monthly consultation",
						"Basic scalp analysis",
						"Prescription medications",
						"Email support",
					],
				},
				{
					id: "advanced-hair",
					name: "Advanced Hair Restoration",
					price: "$299/month",
					description:
						"Comprehensive hair restoration with advanced treatments",
					popular: true,
					trending: true,
					categoryId: "hair-loss",
					features: [
						"Bi-weekly consultations",
						"Advanced scalp analysis",
						"Prescription medications",
						"PRP therapy sessions",
						"24/7 medical support",
						"Progress tracking",
					],
				},
				{
					id: "premium-hair",
					name: "Premium Hair Solutions",
					price: "$449/month",
					description: "Complete hair restoration with cutting-edge treatments",
					categoryId: "hair-loss",
					features: [
						"Weekly consultations",
						"Comprehensive scalp analysis",
						"All medications included",
						"PRP + microneedling",
						"Hair transplant consultation",
						"Dedicated care coordinator",
						"Priority support",
					],
				},
			],
		},
		{
			id: "skincare",
			name: "Advanced Skincare",
			icon: "../../../../assets/img/vitromd/skin.png",
			description:
				"Professional skincare treatments for acne, aging, and overall skin health.",
			specialties: ["Dermatology", "Cosmetic"],
			plans: [
				{
					id: "basic-skin",
					name: "Essential Skincare",
					price: "$149/month",
					description: "Basic skincare routine with professional guidance",
					categoryId: "skincare",
					features: [
						"Skin analysis",
						"Custom skincare routine",
						"Monthly check-ins",
						"Basic products included",
					],
				},
				{
					id: "advanced-skin",
					name: "Advanced Skincare",
					price: "$199/month",
					description: "Comprehensive skincare with advanced treatments",
					popular: true,
					trending: true,
					categoryId: "skincare",
					features: [
						"Detailed skin analysis",
						"Custom treatment plan",
						"Bi-weekly consultations",
						"Professional-grade products",
						"Chemical peels (quarterly)",
						"Progress tracking",
					],
				},
				{
					id: "premium-skin",
					name: "Premium Skincare",
					price: "$299/month",
					description: "Luxury skincare with premium treatments",
					categoryId: "skincare",
					trending: true,
					features: [
						"Weekly consultations",
						"Advanced treatments",
						"Premium product line",
						"Monthly facials",
						"Laser treatments",
						"Concierge service",
					],
				},
			],
		},
		{
			id: "weight-management",
			name: "Weight Management",
			icon: "../../../../assets/img/vitromd/weight.png",
			description:
				"Medically supervised weight loss with nutrition and lifestyle coaching.",
			specialties: ["Endocrinology", "Nutrition"],
			plans: [
				{
					id: "basic-weight",
					name: "Weight Loss Basics",
					price: "$179/month",
					description: "Fundamental weight management program",
					categoryId: "weight-management",
					features: [
						"Initial assessment",
						"Meal planning guidance",
						"Monthly weigh-ins",
						"Basic supplements",
					],
				},
				{
					id: "comprehensive-weight",
					name: "Comprehensive Weight Management",
					price: "$249/month",
					description: "Complete weight loss program with medical supervision",
					popular: true,
					categoryId: "weight-management",
					features: [
						"Medical evaluation",
						"Personalized meal plans",
						"Weekly coaching sessions",
						"Prescription support",
						"Lab work monitoring",
						"Exercise planning",
					],
				},
			],
		},
		{
			id: "mental-wellness",
			name: "Mental Wellness",
			icon: "../../../../assets/img/vitromd/mental-well.png",
			description:
				"Comprehensive mental health support with therapy and psychiatric care.",
			specialties: ["Psychology", "Psychiatry"],
			plans: [
				{
					id: "therapy-sessions",
					name: "Therapy Sessions",
					price: "$150/session",
					description: "Individual therapy sessions with licensed therapists",
					categoryId: "mental-wellness",
					trending: true,
					features: [
						"Licensed therapist",
						"50-minute sessions",
						"Flexible scheduling",
						"Crisis support",
					],
				},
				{
					id: "comprehensive-mental",
					name: "Comprehensive Mental Health",
					price: "$299/month",
					description:
						"Complete mental health care with therapy and medication management",
					popular: true,
					categoryId: "mental-wellness",
					features: [
						"Weekly therapy sessions",
						"Psychiatric evaluation",
						"Medication management",
						"Crisis intervention",
						"Family counseling options",
						"24/7 support line",
					],
				},
			],
		},
		{
			id: "hormone-therapy",
			name: "Hormone Optimization",
			icon: "../../../assets/img/vitromd/hormon.png",
			description:
				"Hormone replacement therapy and optimization for improved wellness.",
			specialties: ["Endocrinology", "Anti-aging"],
			plans: [
				{
					id: "hormone-assessment",
					name: "Hormone Assessment",
					price: "$199/month",
					description: "Comprehensive hormone testing and basic optimization",
					categoryId: "hormone-therapy",
					features: [
						"Comprehensive testing",
						"Results consultation",
						"Basic supplementation",
						"Quarterly follow-ups",
					],
				},
				{
					id: "full-hormone",
					name: "Complete Hormone Optimization",
					price: "$349/month",
					description:
						"Full hormone replacement therapy with ongoing monitoring",
					popular: true,
					categoryId: "hormone-therapy",
					features: [
						"Complete hormone panel",
						"Bioidentical hormones",
						"Monthly monitoring",
						"Lifestyle optimization",
						"Supplement protocols",
						"Regular adjustments",
					],
				},
			],
		},
		{
			id: "preventive-care",
			name: "Preventive Care",
			icon: "../../../assets/img/vitromd/prevention.png",
			description:
				"Comprehensive preventive healthcare with regular screenings.",
			specialties: ["Internal Medicine", "Preventive"],
			plans: [
				{
					id: "basic-preventive",
					name: "Basic Preventive Care",
					price: "$99/month",
					description: "Essential preventive health screenings",
					categoryId: "preventive-care",
					features: [
						"Annual physical exam",
						"Basic lab work",
						"Vaccination tracking",
						"Health risk assessment",
					],
				},
				{
					id: "comprehensive-preventive",
					name: "Comprehensive Preventive Care",
					price: "$149/month",
					description: "Complete preventive health program",
					popular: true,
					categoryId: "preventive-care",
					features: [
						"Comprehensive physical",
						"Advanced lab panels",
						"Specialist referrals",
						"Wellness coaching",
						"Health optimization plan",
						"Priority scheduling",
					],
				},
			],
		},
	];

	constructor(
		private questionsService: PlanCategoryService,
		private authService: PlanCategoryAuthService
	) {}

	get plansForSelectedCategory(): Plan[] {
		if (!this.formData.categoryId) return [];
		const category = this.categories.find(
			(cat) => cat.id === this.formData.categoryId
		);
		return category?.plans || [];
	}

	getQuestionsForCurrentCategory() {
		return this.questionsService.getQuestionsForCategory(
			this.formData.categoryId
		);
	}

	get trendingPlans(): Plan[] {
		return this.categories
			.flatMap((category) => category.plans)
			.filter((plan) => plan.trending);
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
		// Simulate payment processing
		alert(
			"Payment functionality will be integrated with Stripe. For now, this is a demo."
		);
		console.log("Processing payment for:", this.formData);

		// Reset after payment
		this.resetFormData();
		this.currentStep = "overview";
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

	// Legacy methods for compatibility
	onLearnMore(plan: Plan): void {
		console.log("Learn more about plan:", plan);
	}
}
```
							</div>
							<div class="error-message" *ngIf="errorMessage">
								{{ errorMessage }}
							</div>
							<button
								type="submit"
								class="btn btn-primary btn-full"
								[disabled]="isLoading || !loginForm.valid"
							>
								<span *ngIf="isLoading">Logging in...</span>
								<span *ngIf="!isLoading">Login</span>
							</button>
						</form>
						<div class="demo-credentials">
							<p><strong>Demo Credentials:</strong></p>
							<p>Email: demo@example.com</p>
							<p>Password: any password</p>
						</div>
					</div>
				</div>
			</div>
		`,
		styles: [
			`
				.popup-overlay {
					position: fixed;
					top: 0;
					left: 0;
					width: 100%;
					height: 100%;
					background: rgba(0, 0, 0, 0.5);
					display: flex;
					align-items: center;
					justify-content: center;
					z-index: 1000;
				}
				.popup-content {
					background: white;
					border-radius: 12px;
					width: 90%;
					max-width: 400px;
					box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
				}
				.popup-header {
					display: flex;
					justify-content: space-between;
					align-items: center;
					padding: 24px;
					border-bottom: 1px solid #e5e7eb;
				}
				.popup-header h2 {
					margin: 0;
					color: #1f2937;
					font-size: 20px;
					font-weight: 600;
				}
				.close-btn {
					background: none;
					border: none;
					font-size: 24px;
					cursor: pointer;
					color: #6b7280;
					padding: 0;
					width: 32px;
					height: 32px;
					display: flex;
					align-items: center;
					justify-content: center;
					border-radius: 50%;
					transition: all 0.2s;
				}
				.close-btn:hover {
					background: #f3f4f6;
					color: #374151;
				}
				.popup-body {
					padding: 24px;
				}
				.login-message {
					margin-bottom: 24px;
					text-align: center;
				}
				.form-group {
					margin-bottom: 20px;
				}
				.form-group label {
					display: block;
					margin-bottom: 8px;
					color: #374151;
					font-weight: 500;
				}
				.form-input {
					width: 100%;
					padding: 12px;
					border: 2px solid #e5e7eb;
					border-radius: 8px;
					font-size: 16px;
					transition: border-color 0.2s;
					font-family: inherit;
				}
				.form-input:focus {
					outline: none;
					border-color: #3b82f6;
				}
				.error-message {
					color: #ef4444;
					font-size: 14px;
					margin-bottom: 16px;
					text-align: center;
				}
				.btn {
					padding: 12px 24px;
					border: none;
					border-radius: 8px;
					font-size: 16px;
					font-weight: 500;
					cursor: pointer;
					transition: all 0.2s;
					font-family: inherit;
				}
				.btn-full {
					width: 100%;
				}
				.btn:disabled {
					opacity: 0.5;
					cursor: not-allowed;
				}
				.btn-primary {
					background: #322e9f;
					color: white;
				}
				.btn-primary:hover:not(:disabled) {
					background: #322e9f;
				}
				.demo-credentials {
					margin-top: 24px;
					padding: 16px;
					background: #f3f4f6;
					border-radius: 8px;
					font-size: 14px;
				}
				.demo-credentials p {
					margin: 4px 0;
					color: #6b7280;
				}
				.demo-credentials strong {
					color: #374151;
				}
				@media (max-width: 768px) {
					.popup-content {
						width: 95%;
						margin: 20px;
					}
					.popup-header,
					.popup-body {
						padding: 16px;
					}
				}
			`,
		],
	})
	export class LoginPopupOurPlanComponent {
		@Output() close = new EventEmitter<void>();
		@Output() loginSuccess = new EventEmitter<void>();
		email = "";
		password = "";
		isLoading = false;
		errorMessage = "";
		constructor(private authService: PlanCategoryAuthService) {}
		async onLogin(): Promise<void> {
			this.isLoading = true;
			this.errorMessage = "";
			try {
				const success = await this.authService.login(this.email, this.password);

				### 2. QuestionarePopupComponent
				```typescript
				import { Component, EventEmitter, Input, Output } from '@angular/core';
				import { CommonModule } from '@angular/common';
				import { FormsModule } from '@angular/forms';
				import { Question } from '../../plan-category-list.component';

				@Component({
					selector: "app-questionare-popup",
					template: `
						<div class="popup-overlay" (click)="onOverlayClick($event)">
							<div class="popup-content" (click)="$event.stopPropagation()">
								<div class="popup-header">
									<h2>{{ categoryName }} - Assessment Questions</h2>
									<button class="close-btn" (click)="onClose()">&times;</button>
								</div>
								<div class="popup-body">
									<div class="progress-bar">
										<div class="progress-fill" [style.width.%]="progressPercentage"></div>
									</div>
									<p class="progress-text">
										Question {{ currentQuestionIndex + 1 }} of {{ questions.length }}
									</p>
									<div class="question-container" *ngIf="currentQuestion">
										<h3>{{ currentQuestion.text }}</h3>
										<span class="required-indicator" *ngIf="currentQuestion.required">*</span>
										<!-- Text Input -->
										<input *ngIf="currentQuestion.type === 'text'" type="text" [(ngModel)]="answers[currentQuestion.id]" [placeholder]="currentQuestion.placeholder" class="form-input" />
										<!-- Number Input -->
										<input *ngIf="currentQuestion.type === 'number'" type="number" [(ngModel)]="answers[currentQuestion.id]" [placeholder]="currentQuestion.placeholder" class="form-input" />
										<!-- Textarea -->
										<textarea *ngIf="currentQuestion.type === 'textarea'" [(ngModel)]="answers[currentQuestion.id]" [placeholder]="currentQuestion.placeholder" class="form-textarea" rows="4"></textarea>
										<!-- Select Dropdown -->
										<select *ngIf="currentQuestion.type === 'select'" [(ngModel)]="answers[currentQuestion.id]" class="form-select">
											<option value="">Please select...</option>
											<option *ngFor="let option of currentQuestion.options" [value]="option">{{ option }}</option>
										</select>
										<!-- Radio Buttons -->
										<div *ngIf="currentQuestion.type === 'radio'" class="radio-group">
											<label *ngFor="let option of currentQuestion.options" class="radio-label">
												<input type="radio" [name]="currentQuestion.id" [value]="option" [(ngModel)]="answers[currentQuestion.id]" />
												<span class="radio-custom"></span>
												{{ option }}
											</label>
										</div>
										<!-- Checkboxes -->
										<div *ngIf="currentQuestion.type === 'checkbox'" class="checkbox-group">
											<label *ngFor="let option of currentQuestion.options" class="checkbox-label">
												<input type="checkbox" [value]="option" (change)="onCheckboxChange(currentQuestion.id, option, $event)" />
												<span class="checkbox-custom"></span>
												{{ option }}
											</label>
										</div>
									</div>
								</div>
								<div class="popup-footer">
									<button class="btn btn-secondary" (click)="previousQuestion()" [disabled]="currentQuestionIndex === 0">Previous</button>
									<button class="btn btn-primary" (click)="nextQuestion()" [disabled]="!isCurrentQuestionValid()">{{ isLastQuestion() ? "Save & Next" : "Next" }}</button>
								</div>
							</div>
						</div>
					`,
					styles: [
						`
							.popup-overlay {
								position: fixed;
								top: 0;
								left: 0;
								width: 100%;
								height: 100%;
								background: rgba(0, 0, 0, 0.5);
								display: flex;
								align-items: center;
								justify-content: center;
								z-index: 1000;
							}
							.popup-content {
								background: white;
								border-radius: 12px;
								width: 90%;
								max-width: 600px;
								max-height: 80vh;
								overflow-y: auto;
								box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
							}
							.popup-header {
								display: flex;
								justify-content: space-between;
								align-items: center;
								padding: 24px;
								border-bottom: 1px solid #e5e7eb;
							}
							.popup-header h2 {
								margin: 0;
								color: #1f2937;
								font-size: 20px;
								font-weight: 600;
							}
							.close-btn {
								background: none;
								border: none;
								font-size: 24px;
								cursor: pointer;
								color: #6b7280;
								padding: 0;
								width: 32px;
								height: 32px;
								display: flex;
								align-items: center;
								justify-content: center;
								border-radius: 50%;
								transition: all 0.2s;
							}
							.close-btn:hover {
								background: #f3f4f6;
								color: #374151;
							}
							.popup-body {
								padding: 24px;
							}
							.progress-bar {
								width: 100%;
								height: 8px;
								background: #e5e7eb;
								border-radius: 4px;
								overflow: hidden;
								margin-bottom: 8px;
							}
							.progress-fill {
								height: 100%;
								background: linear-gradient(90deg, #c0347e 4.28%, #5551c5 100.15%);
								transition: width 0.3s ease;
							}
							.progress-text {
								font-size: 14px;
								color: #6b7280;
								margin-bottom: 24px;
							}
							.question-container h3 {
								color: #1f2937;
								margin-bottom: 16px;
								font-size: 18px;
								font-weight: 500;
							}
							.required-indicator {
								color: #ef4444;
								margin-left: 4px;
							}
							.form-input,
							.form-textarea,
							.form-select {
								width: 100%;
								padding: 12px;
								border: 2px solid #e5e7eb;
								border-radius: 8px;
								font-size: 16px;
								transition: border-color 0.2s;
								font-family: inherit;
							}
							.form-input:focus,
							.form-textarea:focus,
							.form-select:focus {
								outline: none;
								border-color: #322e9f;
							}
							.form-textarea {
								resize: vertical;
								min-height: 100px;
							}
							.radio-group,
							.checkbox-group {
								display: flex;
								flex-direction: column;
								gap: 12px;
							}
							.radio-label,
							.checkbox-label {
								display: flex;
								align-items: center;
								cursor: pointer;
								font-size: 16px;
								color: #374151;
							}
							.radio-label input,
							.checkbox-label input {
								display: none;
							}
							.radio-custom,
							.checkbox-custom {
								width: 20px;
								height: 20px;
								border: 2px solid #d1d5db;
								margin-right: 12px;
								transition: all 0.2s;
								flex-shrink: 0;
							}
							.radio-custom {
								border-radius: 50%;
							}
							.checkbox-custom {
								border-radius: 4px;
							}
							.radio-label input:checked + .radio-custom {
								border-color: #322e9f;
								background: #322e9f;
								position: relative;
							}
							.radio-label input:checked + .radio-custom::after {
								content: "";
								position: absolute;
								top: 50%;
								left: 50%;
								transform: translate(-50%, -50%);
								width: 8px;
								height: 8px;
								background: white;
								border-radius: 50%;
							}
							.checkbox-label input:checked + .checkbox-custom {
								border-color: #322e9f;
								background: #322e9f;
								position: relative;
							}
							.checkbox-label input:checked + .checkbox-custom::after {
								content: "\2713";
								position: absolute;
								top: 50%;
								left: 50%;
								transform: translate(-50%, -50%);
								color: white;
								font-size: 12px;
								font-weight: bold;
							}
							.popup-footer {
								display: flex;
								justify-content: space-between;
								padding: 24px;
								border-top: 1px solid #e5e7eb;
								gap: 12px;
							}
							.btn {
								padding: 12px 24px;
								border: none;
								border-radius: 50px;
								font-size: 16px;
								font-weight: 500;
								cursor: pointer;
								transition: all 0.2s;
								font-family: inherit;
							}
							.btn:disabled {
								opacity: 0.5;
								cursor: not-allowed;
							}
							.btn-primary {
								background: #322e9f;
								color: white;
							}
							.btn-primary:hover:not(:disabled) {
								background: #1d4ed8;
							}
							.btn-secondary {
								background: transparent;
								color: #6b7280;
								border: 2px solid #e5e7eb;
							}
							.btn-secondary:hover:not(:disabled) {
								border-color: #d1d5db;
								color: #374151;
							}
							@media (max-width: 768px) {
								.popup-content {
									width: 95%;
									margin: 20px;
								}
								.popup-header,
								.popup-body,
								.popup-footer {
									padding: 16px;
								}
							}
						`,
					],
				})
				export class QuestionarePopupComponent {
					@Input() questions: Question[] = [];
					@Input() categoryName: string = "";
					@Output() close = new EventEmitter<void>();
					@Output() complete = new EventEmitter<{ [key: string]: any }>();
					currentQuestionIndex = 0;
					answers: { [key: string]: any } = {};
					constructor() {
						console.log("hureeeeeeee");
					}
					get currentQuestion(): Question | null {
						return this.questions[this.currentQuestionIndex] || null;
					}
					get progressPercentage(): number {
						return ((this.currentQuestionIndex + 1) / this.questions.length) * 100;
					}
					isLastQuestion(): boolean {
						return this.currentQuestionIndex === this.questions.length - 1;
					}
					isCurrentQuestionValid(): boolean {
						const question = this.currentQuestion;
						if (!question) return false;
						if (!question.required) return true;
						const answer = this.answers[question.id];
						if (question.type === "checkbox") {
							return Array.isArray(answer) && answer.length > 0;
						}
						return answer !== undefined && answer !== null && answer !== "";
					}
					onCheckboxChange(questionId: string, option: string, event: any): void {
						if (!this.answers[questionId]) {
							this.answers[questionId] = [];
						}
						if (event.target.checked) {
							this.answers[questionId].push(option);
						} else {
							const index = this.answers[questionId].indexOf(option);
							if (index > -1) {
								this.answers[questionId].splice(index, 1);
							}
						}
					}
					nextQuestion(): void {
						if (this.isLastQuestion()) {
							this.complete.emit(this.answers);
						} else {
							this.currentQuestionIndex++;
						}
					}
					previousQuestion(): void {
						if (this.currentQuestionIndex > 0) {
							this.currentQuestionIndex--;
						}
					}
					onClose(): void {
						this.close.emit();
					}
					onOverlayClick(event: Event): void {
						if (event.target === event.currentTarget) {
							this.onClose();
						}
					}
				}
				```
