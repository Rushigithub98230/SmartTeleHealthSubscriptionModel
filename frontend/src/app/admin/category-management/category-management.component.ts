import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { environment } from '../../../environments/environment';
import { Category as BackendCategory } from '../../services/subscription.service';

// Using backend DTOs for consistency
export interface CreateCategoryRequest {
  name: string;
  description: string;
  icon?: string;
  color?: string;
  isActive: boolean;
  displayOrder: number;
  isMostPopular?: boolean;
  isTrending?: boolean;
}

export interface UpdateCategoryRequest {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  isMostPopular?: boolean;
  isTrending?: boolean;
}

@Component({
  selector: 'app-category-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './category-management.component.html',
  styleUrls: ['./category-management.component.css']
})
export class CategoryManagementComponent implements OnInit, OnDestroy {
  categories: BackendCategory[] = [];
  filteredCategories: BackendCategory[] = [];
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  
  // Make Math available in template
  Math = Math;
  
  // Form state
  showCreateForm = false;
  showEditForm = false;
  editingCategory: BackendCategory | null = null;
  
  // Form data
  formData: CreateCategoryRequest = {
    name: '',
    description: '',
    icon: '',
    color: '',
    isActive: true,
    displayOrder: 0,
    isMostPopular: false,
    isTrending: false
  };
  
  // Search and filter
  searchTerm = '';
  showActiveOnly = false;
  
  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  
  private subscriptions: Subscription[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadCategories();
  }

  ngOnDestroy() {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadCategories() {
    this.isLoading = true;
    this.errorMessage = '';
    
    const params: Record<string, string> = {
      page: this.currentPage.toString(),
      pageSize: this.pageSize.toString()
    };

    if (this.searchTerm) {
      params['searchTerm'] = this.searchTerm;
    }

    if (this.showActiveOnly) {
      params['isActive'] = 'true';
    }

    const queryString = new URLSearchParams(params).toString();
    const url = `${environment.apiUrl}/subscription-management/categories?${queryString}`;

    this.subscriptions.push(
      this.http.get<any>(url).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.categories = response.data.items || [];
            this.totalItems = response.data.totalCount || 0;
            this.applyFilters();
            console.log('Loaded categories:', this.categories);
          } else {
            this.errorMessage = response.message || 'Failed to load categories';
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading categories:', error);
          this.errorMessage = 'Error loading categories';
          this.isLoading = false;
        }
      })
    );
  }

  applyFilters() {
    let filtered = [...this.categories];
    
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(cat => 
        cat.name.toLowerCase().includes(term) || 
        (cat.description || '').toLowerCase().includes(term)
      );
    }
    
    if (this.showActiveOnly) {
      filtered = filtered.filter(cat => cat.isActive);
    }
    
    this.filteredCategories = filtered;
  }

  onSearch() {
    this.currentPage = 1;
    this.loadCategories();
  }

  onFilterChange() {
    this.applyFilters();
  }

  onCreateCategory() {
    this.showCreateForm = true;
    this.showEditForm = false;
    this.editingCategory = null;
    this.resetForm();
  }

  onEditCategory(category: BackendCategory) {
    this.showEditForm = true;
    this.showCreateForm = false;
    this.editingCategory = category;
    this.formData = {
      name: category.name,
      description: category.description || '',
      icon: category.icon,
      color: category.color || '',
      isActive: category.isActive,
      displayOrder: category.displayOrder,
      isMostPopular: category.isMostPopular || false,
      isTrending: category.isTrending || false
    };
  }

  onCancelForm() {
    this.showCreateForm = false;
    this.showEditForm = false;
    this.editingCategory = null;
    this.resetForm();
  }

  onSubmitForm() {
    if (this.showCreateForm) {
      this.createCategory();
    } else if (this.showEditForm && this.editingCategory) {
      this.updateCategory();
    }
  }

  createCategory() {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.subscriptions.push(
      this.http.post<any>(`${environment.apiUrl}/subscription-management/categories`, this.formData).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.successMessage = 'Category created successfully';
            this.loadCategories();
            this.onCancelForm();
          } else {
            this.errorMessage = response.message || 'Failed to create category';
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error creating category:', error);
          this.errorMessage = 'Error creating category';
          this.isLoading = false;
        }
      })
    );
  }

  updateCategory() {
    if (!this.editingCategory) return;
    
    this.isLoading = true;
    this.errorMessage = '';
    
    const updateData: UpdateCategoryRequest = {
      id: this.editingCategory.id,
      name: this.formData.name,
      description: this.formData.description,
      isActive: this.formData.isActive,
      isMostPopular: this.formData.isMostPopular,
      isTrending: this.formData.isTrending
    };
    
    this.subscriptions.push(
      this.http.put<any>(`${environment.apiUrl}/subscription-management/categories/${this.editingCategory.id}`, updateData).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.successMessage = 'Category updated successfully';
            this.loadCategories();
            this.onCancelForm();
          } else {
            this.errorMessage = response.message || 'Failed to update category';
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error updating category:', error);
          this.errorMessage = 'Error updating category';
          this.isLoading = false;
        }
      })
    );
  }

  onDeleteCategory(category: BackendCategory) {
    if (confirm(`Are you sure you want to delete "${category.name}"?`)) {
      this.isLoading = true;
      this.errorMessage = '';
      
      this.subscriptions.push(
        this.http.delete<any>(`${environment.apiUrl}/subscription-management/categories/${category.id}`).subscribe({
          next: (response) => {
            if (response.statusCode === 200) {
              this.successMessage = 'Category deleted successfully';
              this.loadCategories();
            } else {
              this.errorMessage = response.message || 'Failed to delete category';
            }
            this.isLoading = false;
          },
          error: (error) => {
            console.error('Error deleting category:', error);
            this.errorMessage = 'Error deleting category';
            this.isLoading = false;
          }
        })
      );
    }
  }

  onToggleActive(category: BackendCategory) {
    const updateData: UpdateCategoryRequest = {
      id: category.id,
      name: category.name,
      description: category.description,
      isActive: !category.isActive,
      isMostPopular: category.isMostPopular,
      isTrending: category.isTrending
    };
    
    this.subscriptions.push(
      this.http.put<any>(`${environment.apiUrl}/subscription-management/categories/${category.id}`, updateData).subscribe({
        next: (response) => {
          if (response.statusCode === 200) {
            this.successMessage = `Category ${updateData.isActive ? 'activated' : 'deactivated'} successfully`;
            this.loadCategories();
          } else {
            this.errorMessage = response.message || 'Failed to update category';
          }
        },
        error: (error) => {
          console.error('Error updating category:', error);
          this.errorMessage = 'Error updating category';
        }
      })
    );
  }

  resetForm() {
    this.formData = {
      name: '',
      description: '',
      icon: '',
      color: '',
      isActive: true,
      displayOrder: 0,
      isMostPopular: false,
      isTrending: false
    };
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadCategories();
  }

  clearMessages() {
    this.errorMessage = '';
    this.successMessage = '';
  }
}
