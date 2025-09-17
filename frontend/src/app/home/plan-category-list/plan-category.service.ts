import { Injectable } from '@angular/core';
import { CategoryQuestions, Question } from './plan-category-list.component';
import { QuestionnaireService, CreateUserResponseDto } from '../../services/questionnaire.service';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PlanCategoryService {
  constructor(private questionnaireService: QuestionnaireService) {}

  /**
   * Get questions for a specific category from backend
   */
  getQuestionsForCategory(categoryId: string): Observable<Question[]> {
    return this.questionnaireService.getTemplatesByCategory(categoryId).pipe(
      map((response: any) => {
        if (response.statusCode === 200 && response.data && response.data.length > 0) {
          // Get the first active template for the category
          const template = response.data.find((t: any) => t.isActive);
          if (template && template.questions) {
            return template.questions.map((q: any) => ({
              id: q.id,
              text: q.text,
              type: q.type,
              options: q.options || [],
              required: q.required || false,
              placeholder: q.placeholder
            }));
          }
        }
        // Fallback to hardcoded questions if backend fails
        return this.getHardcodedQuestions(categoryId);
      }),
      catchError((error) => {
        console.error('Error fetching questions from backend:', error);
        // Fallback to hardcoded questions
        return of(this.getHardcodedQuestions(categoryId));
      })
    );
  }

  /**
   * Submit questionnaire response to backend
   */
  submitQuestionnaireResponse(templateId: string, answers: { [key: string]: any }, planId?: string, categoryId?: string): Observable<any> {
    const response: CreateUserResponseDto = {
      templateId: templateId,
      answers: answers,
      planId: planId,
      categoryId: categoryId
    };

    return this.questionnaireService.submitResponse(response);
  }

  /**
   * Get hardcoded questions as fallback
   */
  private getHardcodedQuestions(categoryId: string): Question[] {
    const categoryKey = this.getCategoryKey(categoryId);
    return this.categoryQuestions[categoryKey] || [];
  }

  /**
   * Map category ID to category key
   */
  private getCategoryKey(categoryId: string): string {
    // This is a simple mapping - you might want to make this more sophisticated
    const categoryMap: { [key: string]: string } = {
      'hair-loss': 'hair-loss',
      'skincare': 'skincare',
      'weight-loss': 'weight-loss',
      'mental-health': 'mental-health',
      'sexual-health': 'sexual-health',
      'general-health': 'general-health'
    };
    return categoryMap[categoryId] || 'general-health';
  }
 private categoryQuestions: CategoryQuestions = {
    'hair-loss': [
      {
        id: 'hair-loss-duration',
        text: 'How long have you been experiencing hair loss?',
        type: 'select',
        options: ['Less than 6 months', '6 months - 1 year', '1-2 years', '2-5 years', 'More than 5 years'],
        required: true
      },
      {
        id: 'hair-loss-pattern',
        text: 'What type of hair loss pattern are you experiencing?',
        type: 'radio',
        options: ['Receding hairline', 'Crown thinning', 'Overall thinning', 'Patchy hair loss', 'Not sure'],
        required: true
      },
      {
        id: 'family-history',
        text: 'Do you have a family history of hair loss?',
        type: 'radio',
        options: ['Yes, on mother\'s side', 'Yes, on father\'s side', 'Yes, on both sides', 'No', 'Not sure'],
        required: true
      },
      {
        id: 'previous-treatments',
        text: 'Have you tried any hair loss treatments before?',
        type: 'checkbox',
        options: ['Minoxidil', 'Finasteride', 'Hair transplant', 'Laser therapy', 'Natural remedies', 'None'],
        required: false
      },
      {
        id: 'current-medications',
        text: 'Are you currently taking any medications?',
        type: 'textarea',
        placeholder: 'Please list any current medications or write "None"',
        required: true
      }
    ],
    'skincare': [
      {
        id: 'skin-concerns',
        text: 'What are your primary skin concerns?',
        type: 'checkbox',
        options: ['Acne', 'Wrinkles/Fine lines', 'Dark spots', 'Uneven skin tone', 'Dryness', 'Oily skin', 'Sensitivity'],
        required: true
      },
      {
        id: 'skin-type',
        text: 'What is your skin type?',
        type: 'radio',
        options: ['Oily', 'Dry', 'Combination', 'Normal', 'Sensitive', 'Not sure'],
        required: true
      },
      {
        id: 'current-routine',
        text: 'Describe your current skincare routine',
        type: 'textarea',
        placeholder: 'Please describe your morning and evening skincare routine',
        required: true
      },
      {
        id: 'allergies',
        text: 'Do you have any known allergies to skincare ingredients?',
        type: 'text',
        placeholder: 'List any known allergies or write "None"',
        required: true
      },
      {
        id: 'sun-exposure',
        text: 'How often are you exposed to sun?',
        type: 'select',
        options: ['Daily', 'Few times a week', 'Occasionally', 'Rarely', 'Never'],
        required: true
      }
    ],
    'weight-management': [
      {
        id: 'weight-goal',
        text: 'What is your weight loss goal?',
        type: 'select',
        options: ['5-10 lbs', '10-20 lbs', '20-30 lbs', '30-50 lbs', 'More than 50 lbs'],
        required: true
      },
      {
        id: 'current-weight',
        text: 'What is your current weight? (lbs)',
        type: 'number',
        placeholder: 'Enter your weight in pounds',
        required: true
      },
      {
        id: 'height',
        text: 'What is your height? (feet and inches)',
        type: 'text',
        placeholder: 'e.g., 5\'8"',
        required: true
      },
      {
        id: 'diet-restrictions',
        text: 'Do you have any dietary restrictions?',
        type: 'checkbox',
        options: ['Vegetarian', 'Vegan', 'Gluten-free', 'Dairy-free', 'Keto', 'Low-carb', 'None'],
        required: false
      },
      {
        id: 'exercise-level',
        text: 'What is your current exercise level?',
        type: 'radio',
        options: ['Sedentary', 'Light exercise (1-2 days/week)', 'Moderate exercise (3-4 days/week)', 'Heavy exercise (5-6 days/week)', 'Very heavy exercise (daily)'],
        required: true
      },
      {
        id: 'medical-conditions',
        text: 'Do you have any medical conditions that might affect weight loss?',
        type: 'textarea',
        placeholder: 'Please list any relevant medical conditions or write "None"',
        required: true
      }
    ],
    'mental-wellness': [
      {
        id: 'primary-concerns',
        text: 'What are your primary mental health concerns?',
        type: 'checkbox',
        options: ['Anxiety', 'Depression', 'Stress management', 'Relationship issues', 'Work-life balance', 'Sleep issues', 'Other'],
        required: true
      },
      {
        id: 'therapy-experience',
        text: 'Have you had therapy or counseling before?',
        type: 'radio',
        options: ['Yes, recently (within last year)', 'Yes, but not recently', 'No, this is my first time', 'Prefer not to say'],
        required: true
      },
      {
        id: 'support-system',
        text: 'How would you rate your current support system?',
        type: 'radio',
        options: ['Very strong', 'Adequate', 'Limited', 'Very limited', 'Prefer not to say'],
        required: true
      },
      {
        id: 'therapy-goals',
        text: 'What would you like to achieve through therapy?',
        type: 'textarea',
        placeholder: 'Please describe your goals and what you hope to accomplish',
        required: true
      },
      {
        id: 'crisis-support',
        text: 'Do you currently have thoughts of self-harm?',
        type: 'radio',
        options: ['No', 'Sometimes', 'Yes - I need immediate help'],
        required: true
      }
    ],
    'hormone-therapy': [
      {
        id: 'symptoms',
        text: 'What symptoms are you experiencing?',
        type: 'checkbox',
        options: ['Fatigue', 'Weight gain', 'Mood changes', 'Sleep issues', 'Low libido', 'Hot flashes', 'Brain fog', 'Other'],
        required: true
      },
      {
        id: 'age',
        text: 'What is your age?',
        type: 'number',
        placeholder: 'Enter your age',
        required: true
      },
      {
        id: 'gender',
        text: 'What is your gender?',
        type: 'radio',
        options: ['Male', 'Female', 'Non-binary', 'Prefer not to say'],
        required: true
      },
      {
        id: 'previous-testing',
        text: 'Have you had hormone testing done before?',
        type: 'radio',
        options: ['Yes, within the last year', 'Yes, but more than a year ago', 'No', 'Not sure'],
        required: true
      },
      {
        id: 'current-hormones',
        text: 'Are you currently taking any hormone medications?',
        type: 'textarea',
        placeholder: 'Please list any hormone medications or supplements, or write "None"',
        required: true
      }
    ],
    'preventive-care': [
      {
        id: 'last-checkup',
        text: 'When was your last comprehensive physical exam?',
        type: 'select',
        options: ['Within the last 6 months', '6 months - 1 year ago', '1-2 years ago', '2-3 years ago', 'More than 3 years ago', 'Never'],
        required: true
      },
      {
        id: 'health-concerns',
        text: 'Do you have any current health concerns?',
        type: 'textarea',
        placeholder: 'Please describe any health concerns or symptoms you\'d like to discuss',
        required: false
      },
      {
        id: 'family-medical-history',
        text: 'Do you have a family history of any of the following?',
        type: 'checkbox',
        options: ['Heart disease', 'Diabetes', 'Cancer', 'High blood pressure', 'Stroke', 'Mental health conditions', 'None of the above'],
        required: true
      },
      {
        id: 'lifestyle-habits',
        text: 'Which lifestyle habits apply to you?',
        type: 'checkbox',
        options: ['Regular exercise', 'Healthy diet', 'Adequate sleep', 'Stress management', 'No smoking', 'Limited alcohol', 'None of the above'],
        required: false
      },
      {
        id: 'screening-interest',
        text: 'Which preventive screenings are you interested in?',
        type: 'checkbox',
        options: ['Blood work', 'Cancer screenings', 'Heart health assessment', 'Bone density', 'Vision/hearing tests', 'Mental health screening'],
        required: true
      }
    ]
  };

}