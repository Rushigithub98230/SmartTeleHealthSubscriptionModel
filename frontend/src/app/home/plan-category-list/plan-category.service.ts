import { Injectable } from '@angular/core';
import { CategoryQuestions, Question } from './plan-category-list.component';

@Injectable({
  providedIn: 'root'
})
export class PlanCategoryService {
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

  getQuestionsForCategory(categoryId: string): Question[] {
    console.log('Getting questions for category ID:', categoryId);
    
    // Map backend category IDs to question sets
    // This mapping should ideally come from the backend, but for now we'll use a mapping
    const categoryMapping: { [key: string]: string } = {
      // Map actual backend category IDs to question sets
      // You should update these mappings based on the actual category IDs from your backend
      '1': 'hair-loss',
      '2': 'skincare', 
      '3': 'weight-management',
      '4': 'mental-wellness',
      '5': 'hormone-therapy',
      '6': 'preventive-care'
    };
    
    // Try to find a mapping first
    const questionKey = categoryMapping[categoryId];
    
    // If we have a specific question set for this category, use it
    if (questionKey && this.categoryQuestions[questionKey]) {
      console.log('Found specific questions for category:', questionKey);
      return this.categoryQuestions[questionKey];
    }
    
    // Return general questions if no specific mapping found
    console.log('Using general questions for category:', categoryId);
    const generalQuestions: Question[] = [
      {
        id: 'health-goals',
        text: 'What are your primary health goals?',
        type: 'checkbox',
        options: ['Improve overall health', 'Manage specific condition', 'Preventive care', 'Regular monitoring', 'Expert consultation'],
        required: true
      },
      {
        id: 'medical-history',
        text: 'Do you have any significant medical history?',
        type: 'textarea',
        placeholder: 'Please describe any relevant medical conditions, allergies, or medications',
        required: true
      },
      {
        id: 'previous-experience',
        text: 'Have you used telemedicine services before?',
        type: 'radio',
        options: ['Yes, frequently', 'Yes, occasionally', 'Yes, but rarely', 'No, this is my first time'],
        required: true
      },
      {
        id: 'communication-preference',
        text: 'How do you prefer to communicate with healthcare providers?',
        type: 'checkbox',
        options: ['Video calls', 'Phone calls', 'Text messaging', 'Email', 'In-app messaging'],
        required: true
      },
      {
        id: 'emergency-contact',
        text: 'Emergency contact information',
        type: 'text',
        placeholder: 'Name and phone number of emergency contact',
        required: true
      }
    ];
    
    return generalQuestions;
  }
}