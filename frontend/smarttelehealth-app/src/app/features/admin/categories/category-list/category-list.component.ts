import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CategoryService } from '../../../../core/services';
import { CategoryDto, CreateCategoryDto, UpdateCategoryDto } from '../../../../core/models';

/**
 * Category List Component (Admin)
 * Manage all categories with CRUD operations
 * 
 * APIs Used:
 * - GET /api/Categories
 * - POST /api/Categories
 * - PUT /api/Categories/{id}
 * - DELETE /api/Categories/{id}
 * 
 * Route: /webadmin/categories
 * Access: Admin only
 */
@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './category-list.component.html',
  styleUrls: ['./category-list.component.scss']
})
export class CategoryListComponent implements OnInit {
  categories: CategoryDto[] = [];
  loading = false;
  actionLoading = false;
  error: string | null = null;

  // Modal forms
  categoryForm!: FormGroup;
  isEditMode = false;
  editingCategoryId: string | null = null;

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadCategories();
  }

  /**
   * Initialize category form
   */
  initForm(): void {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)],
      icon: [''],
      color: [''],
      displayOrder: [0],
      isActive: [true],
      isMostPopular: [false],
      isTrending: [false]
    });
  }

  /**
   * Load all categories
   * API: GET /api/Categories
   */
  loadCategories(): void {
    this.loading = true;
    this.error = null;

    this.categoryService.getAllCategories().subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.categories = response.data;
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to load categories';
        this.loading = false;
      }
    });
  }

  /**
   * Open create modal
   */
  openCreateModal(): void {
    this.isEditMode = false;
    this.editingCategoryId = null;
    this.categoryForm.reset({
      displayOrder: 0,
      isActive: true,
      isMostPopular: false,
      isTrending: false
    });
  }

  /**
   * Open edit modal
   */
  openEditModal(category: CategoryDto): void {
    this.isEditMode = true;
    this.editingCategoryId = category.id;
    this.categoryForm.patchValue({
      name: category.name,
      description: category.description,
      icon: category.icon,
      color: category.color,
      displayOrder: category.displayOrder,
      isActive: category.isActive,
      isMostPopular: category.isMostPopular,
      isTrending: category.isTrending
    });
  }

  /**
   * Save category (create or update)
   */
  saveCategory(): void {
    if (this.categoryForm.invalid) {
      this.markFormGroupTouched(this.categoryForm);
      return;
    }

    this.actionLoading = true;

    if (this.isEditMode && this.editingCategoryId) {
      this.updateCategory();
    } else {
      this.createCategory();
    }
  }

  /**
   * Create new category
   * API: POST /api/Categories
   */
  createCategory(): void {
    const dto: CreateCategoryDto = this.categoryForm.value;

    this.categoryService.createCategory(dto).subscribe({
      next: (response) => {
        if (response.statusCode === 201 || response.statusCode === 200) {
          this.loadCategories();
          this.closeModal();
        }
        this.actionLoading = false;
      },
      error: (error) => {
        alert(error.message || 'Failed to create category');
        this.actionLoading = false;
      }
    });
  }

  /**
   * Update existing category
   * API: PUT /api/Categories/{id}
   */
  updateCategory(): void {
    if (!this.editingCategoryId) return;

    const dto: UpdateCategoryDto = {
      id: this.editingCategoryId,
      ...this.categoryForm.value
    };

    this.categoryService.updateCategory(this.editingCategoryId, dto).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.loadCategories();
          this.closeModal();
        }
        this.actionLoading = false;
      },
      error: (error) => {
        alert(error.message || 'Failed to update category');
        this.actionLoading = false;
      }
    });
  }

  /**
   * Delete category
   * API: DELETE /api/Categories/{id}
   */
  deleteCategory(categoryId: string): void {
    if (!confirm('Are you sure you want to delete this category? This cannot be undone.')) {
      return;
    }

    this.actionLoading = true;

    this.categoryService.deleteCategory(categoryId).subscribe({
      next: (response) => {
        if (response.statusCode === 200) {
          this.loadCategories();
        }
        this.actionLoading = false;
      },
      error: (error) => {
        alert(error.message || 'Failed to delete category');
        this.actionLoading = false;
      }
    });
  }

  /**
   * Close modal
   */
  closeModal(): void {
    this.categoryForm.reset();
    this.isEditMode = false;
    this.editingCategoryId = null;
    
    // Close Bootstrap modal programmatically
    const modalElement = document.getElementById('categoryModal');
    if (modalElement) {
      const modal = (window as any).bootstrap.Modal.getInstance(modalElement);
      if (modal) modal.hide();
    }
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      formGroup.get(key)?.markAsTouched();
    });
  }

  hasError(field: string, error: string): boolean {
    const control = this.categoryForm.get(field);
    return !!control && control.hasError(error) && control.touched;
  }
}


