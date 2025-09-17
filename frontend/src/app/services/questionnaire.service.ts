import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Question {
  id: string;
  text: string;
  type: 'text' | 'select' | 'radio' | 'checkbox' | 'textarea' | 'number';
  options?: string[];
  required: boolean;
  placeholder?: string;
}

export interface QuestionnaireTemplate {
  id: string;
  title: string;
  description: string;
  categoryId: string;
  questions: Question[];
  isActive: boolean;
  createdDate: string;
  updatedDate: string;
}

export interface UserResponse {
  id: string;
  userId: string;
  templateId: string;
  answers: { [questionId: string]: any };
  submittedDate: string;
  status: string;
}

export interface CreateUserResponseDto {
  templateId: string;
  answers: { [questionId: string]: any };
  planId?: string;
  categoryId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class QuestionnaireService {
  private readonly baseUrl = `${environment.apiUrl}/api/Questionnaire`;

  constructor(private http: HttpClient) {}

  /**
   * Get all questionnaire templates
   */
  getAllTemplates(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/templates`);
  }

  /**
   * Get questionnaire template by ID
   */
  getTemplateById(id: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/templates/${id}`);
  }

  /**
   * Get questionnaire templates by category
   */
  getTemplatesByCategory(categoryId: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/templates/by-category/${categoryId}`);
  }

  /**
   * Submit user response
   */
  submitResponse(response: CreateUserResponseDto): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/responses`, response);
  }

  /**
   * Get user response by ID
   */
  getUserResponseById(id: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/responses/${id}`);
  }

  /**
   * Get user responses by user and template
   */
  getUserResponsesByUserAndTemplate(userId: string, templateId: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/responses/user/${userId}/template/${templateId}`);
  }

  /**
   * Get user responses by user ID
   */
  getUserResponsesByUser(userId: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/responses/user/${userId}`);
  }
}
