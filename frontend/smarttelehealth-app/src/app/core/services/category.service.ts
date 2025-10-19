import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CommonService, ApiResponse } from './common.service';
import { CategoryDto, CreateCategoryDto, UpdateCategoryDto } from '../models';

/**
 * Category Service
 * Uses CommonService for all HTTP calls - NO direct HttpClient usage
 * 
 * API Endpoints Used:
 * - GET /api/Categories
 * - GET /api/Categories/{id}
 * - POST /api/Categories (Admin)
 * - PUT /api/Categories/{id} (Admin)
 * - DELETE /api/Categories/{id} (Admin)
 */
@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  constructor(private commonService: CommonService) {}

  /**
   * Get all active categories
   * API: GET /api/Categories
   * Used in: Marketing Browse, Plan Filters, Admin Category Management
   */
  getAllCategories(): Observable<ApiResponse<CategoryDto[]>> {
    return this.commonService.get<CategoryDto[]>('Categories');
  }

  /**
   * Get category by ID
   * API: GET /api/Categories/{id}
   * Used in: Category Detail Page
   */
  getCategoryById(id: string): Observable<ApiResponse<CategoryDto>> {
    return this.commonService.get<CategoryDto>(`Categories/${id}`);
  }

  /**
   * Create category (Admin Only)
   * API: POST /api/Categories
   * Used in: Admin Create Category Form
   */
  createCategory(dto: CreateCategoryDto): Observable<ApiResponse<CategoryDto>> {
    return this.commonService.post<CategoryDto>('Categories', dto);
  }

  /**
   * Update category (Admin Only)
   * API: PUT /api/Categories/{id}
   * Used in: Admin Edit Category Form
   */
  updateCategory(id: string, dto: UpdateCategoryDto): Observable<ApiResponse<CategoryDto>> {
    return this.commonService.put<CategoryDto>(`Categories/${id}`, dto);
  }

  /**
   * Delete category (Admin Only)
   * API: DELETE /api/Categories/{id}
   * Used in: Admin Category Management
   */
  deleteCategory(id: string): Observable<ApiResponse<any>> {
    return this.commonService.delete(`Categories/${id}`);
  }
}


