import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Question } from '../../plan-category-list.component';


@Component({
  selector: "app-questionare-popup",
  standalone: true,
  imports: [CommonModule, FormsModule],

  template: `
    <div class="popup-overlay" (click)="onOverlayClick($event)">
      <div class="popup-content" (click)="$event.stopPropagation()">
        <div class="popup-header">
          <h2>{{ categoryName }} - Assessment Questions</h2>
          <button class="close-btn" (click)="onClose()">&times;</button>
        </div>

        <div class="popup-body">
          <div class="progress-bar">
            <div
              class="progress-fill"
              [style.width.%]="progressPercentage"
            ></div>
          </div>
          <p class="progress-text">
            Question {{ currentQuestionIndex + 1 }} of {{ questions.length }}
          </p>

          <div class="question-container" *ngIf="currentQuestion">
            <h3>{{ currentQuestion.text }}</h3>
            <span class="required-indicator" *ngIf="currentQuestion.required"
              >*</span
            >

            <!-- Text Input -->
            <input
              *ngIf="currentQuestion.type === 'text'"
              type="text"
              [(ngModel)]="answers[currentQuestion.id]"
              [placeholder]="currentQuestion.placeholder"
              class="form-input"
            />

            <!-- Number Input -->
            <input
              *ngIf="currentQuestion.type === 'number'"
              type="number"
              [(ngModel)]="answers[currentQuestion.id]"
              [placeholder]="currentQuestion.placeholder"
              class="form-input"
            />

            <!-- Textarea -->
            <textarea
              *ngIf="currentQuestion.type === 'textarea'"
              [(ngModel)]="answers[currentQuestion.id]"
              [placeholder]="currentQuestion.placeholder"
              class="form-textarea"
              rows="4"
            ></textarea>

            <!-- Select Dropdown -->
            <select
              *ngIf="currentQuestion.type === 'select'"
              [(ngModel)]="answers[currentQuestion.id]"
              class="form-select"
            >
              <option value="">Please select...</option>
              <option
                *ngFor="let option of currentQuestion.options"
                [value]="option"
              >
                {{ option }}
              </option>
            </select>

            <!-- Radio Buttons -->
            <div *ngIf="currentQuestion.type === 'radio'" class="radio-group">
              <label
                *ngFor="let option of currentQuestion.options"
                class="radio-label"
              >
                <input
                  type="radio"
                  [name]="currentQuestion.id"
                  [value]="option"
                  [(ngModel)]="answers[currentQuestion.id]"
                />
                <span class="radio-custom"></span>
                {{ option }}
              </label>
            </div>

            <!-- Checkboxes -->
            <div
              *ngIf="currentQuestion.type === 'checkbox'"
              class="checkbox-group"
            >
              <label
                *ngFor="let option of currentQuestion.options"
                class="checkbox-label"
              >
                <input
                  type="checkbox"
                  [value]="option"
                  (change)="
                    onCheckboxChange(currentQuestion.id, option, $event)
                  "
                />
                <span class="checkbox-custom"></span>
                {{ option }}
              </label>
            </div>
          </div>
        </div>

        <div class="popup-footer">
          <button
            class="btn btn-secondary"
            (click)="previousQuestion()"
            [disabled]="currentQuestionIndex === 0"
          >
            Previous
          </button>

          <button
            class="btn btn-primary"
            (click)="nextQuestion()"
            [disabled]="!isCurrentQuestionValid()"
          >
            {{ isLastQuestion() ? "Save & Next" : "Next" }}
          </button>
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
        content: "✓";
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