import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { 
  ProductModifier, 
  ProductModifierCreateRequest,
  ProductModifierOption,
  ProductModifierOptionCreateRequest
} from '../../../models/ProductModifier';
import { ProductModifierService } from '../../../services/product-modifier.service';

@Component({
  selector: 'app-modifier-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './modifier-management.component.html',
  styleUrls: ['./modifier-management.component.scss']
})
export class ModifierManagementComponent implements OnInit {
  modifiers: ProductModifier[] = [];
  modifierForm: FormGroup;
  optionForm: FormGroup;
  isEditing = false;
  isEditingOption = false;
  editingModifierId?: number;
  editingOptionId?: number;
  selectedModifier?: ProductModifier;
  loading = false;
  showAddModifierForm = false;
  showAddOptionForm = false;

  constructor(
    private fb: FormBuilder,
    private modifierService: ProductModifierService
  ) {
    this.modifierForm = this.fb.group({
      name: ['', Validators.required],
      code: [''],
      description: [''],
      priceAdjustment: [0],
      costAdjustment: [0],
      isRequired: [false],
      isMultiple: [false],
      maxSelections: [1, [Validators.required, Validators.min(1)]],
      isActive: [true],
      displayOrder: [0]
    });

    this.optionForm = this.fb.group({
      name: ['', Validators.required],
      code: [''],
      description: [''],
      priceAdjustment: [0],
      costAdjustment: [0],
      isActive: [true],
      isDefault: [false],
      displayOrder: [0]
    });
  }

  ngOnInit(): void {
    this.loadModifiers();
  }

  loadModifiers(): void {
    this.loading = true;
    this.modifierService.getAllModifiers().subscribe({
      next: (modifiers) => {
        this.modifiers = modifiers;
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
      }
    });
  }

  loadModifierOptions(modifierId: number): void {
    this.modifierService.getModifierOptions(modifierId).subscribe({
      next: (options) => {
        const modifier = this.modifiers.find(m => m.id === modifierId);
        if (modifier) {
          modifier.options = options;
        }
      },
      error: (error) => {
      }
    });
  }

  onModifierSubmit(): void {
    if (this.modifierForm.valid) {
      const formValue = this.modifierForm.value;
      const modifierData: ProductModifierCreateRequest = {
        name: formValue.name,
        code: formValue.code,
        description: formValue.description,
        priceAdjustment: formValue.priceAdjustment,
        costAdjustment: formValue.costAdjustment,
        isRequired: formValue.isRequired,
        isMultiple: formValue.isMultiple,
        maxSelections: formValue.maxSelections,
        isActive: formValue.isActive,
        displayOrder: formValue.displayOrder
      };

      if (this.isEditing && this.editingModifierId) {
        this.updateModifier(modifierData);
      } else {
        this.createModifier(modifierData);
      }
    }
  }

  createModifier(modifierData: ProductModifierCreateRequest): void {
    this.modifierService.createModifier(modifierData).subscribe({
      next: (modifier) => {
        this.modifiers.push(modifier);
        this.showAddModifierForm = false;
        this.resetModifierForm();
      },
      error: (error) => {
      }
    });
  }

  updateModifier(modifierData: ProductModifierCreateRequest): void {
    if (!this.editingModifierId) return;

    this.modifierService.updateModifier(this.editingModifierId, { id: this.editingModifierId, ...modifierData }).subscribe({
      next: (modifier) => {
        const index = this.modifiers.findIndex(m => m.id === this.editingModifierId);
        if (index !== -1) {
          this.modifiers[index] = modifier;
        }
        this.showAddModifierForm = false;
        this.resetModifierForm();
      },
      error: (error) => {
      }
    });
  }

  onOptionSubmit(): void {
    if (this.optionForm.valid && this.selectedModifier) {
      const formValue = this.optionForm.value;
      const optionData: ProductModifierOptionCreateRequest = {
        modifierId: this.selectedModifier.id!,
        name: formValue.name,
        code: formValue.code,
        description: formValue.description,
        priceAdjustment: formValue.priceAdjustment,
        costAdjustment: formValue.costAdjustment,
        isActive: formValue.isActive,
        isDefault: formValue.isDefault,
        displayOrder: formValue.displayOrder
      };

      if (this.isEditingOption && this.editingOptionId) {
        this.updateModifierOption(optionData);
      } else {
        this.createModifierOption(optionData);
      }
    }
  }

  createModifierOption(optionData: ProductModifierOptionCreateRequest): void {
    this.modifierService.createModifierOption(this.selectedModifier!.id!, optionData).subscribe({
      next: (option) => {
        if (!this.selectedModifier!.options) {
          this.selectedModifier!.options = [];
        }
        this.selectedModifier!.options.push(option);
        this.showAddOptionForm = false;
        this.resetOptionForm();
      },
      error: (error) => {
      }
    });
  }

  updateModifierOption(optionData: ProductModifierOptionCreateRequest): void {
    if (!this.editingOptionId) return;

    this.modifierService.updateModifierOption(this.editingOptionId, { id: this.editingOptionId, ...optionData }).subscribe({
      next: (option) => {
        if (this.selectedModifier?.options) {
          const index = this.selectedModifier.options.findIndex(o => o.id === this.editingOptionId);
          if (index !== -1) {
            this.selectedModifier.options[index] = option;
          }
        }
        this.showAddOptionForm = false;
        this.resetOptionForm();
      },
      error: (error) => {
      }
    });
  }

  selectModifier(modifier: ProductModifier): void {
    this.selectedModifier = modifier;
    this.loadModifierOptions(modifier.id!);
  }

  editModifier(modifier: ProductModifier): void {
    this.isEditing = true;
    this.showAddModifierForm = false;
    this.editingModifierId = modifier.id;
    this.modifierForm.patchValue({
      name: modifier.name,
      code: modifier.code,
      description: modifier.description,
      priceAdjustment: modifier.priceAdjustment,
      costAdjustment: modifier.costAdjustment,
      isRequired: modifier.isRequired,
      isMultiple: modifier.isMultiple,
      maxSelections: modifier.maxSelections,
      isActive: modifier.isActive,
      displayOrder: modifier.displayOrder
    });
  }

  editModifierOption(option: ProductModifierOption): void {
    this.isEditingOption = true;
    this.showAddOptionForm = false;
    this.editingOptionId = option.id;
    this.optionForm.patchValue({
      name: option.name,
      code: option.code,
      description: option.description,
      priceAdjustment: option.priceAdjustment,
      costAdjustment: option.costAdjustment,
      isActive: option.isActive,
      isDefault: option.isDefault,
      displayOrder: option.displayOrder
    });
  }

  deleteModifier(modifier: ProductModifier): void {
    if (!modifier.id) return;

    if (confirm('Are you sure you want to delete this modifier?')) {
      this.modifierService.deleteModifier(modifier.id).subscribe({
        next: () => {
          this.modifiers = this.modifiers.filter(m => m.id !== modifier.id);
        },
        error: (error) => {
        }
      });
    }
  }

  deleteModifierOption(option: ProductModifierOption): void {
    if (!option.id) return;

    if (confirm('Are you sure you want to delete this option?')) {
      this.modifierService.deleteModifierOption(option.id).subscribe({
        next: () => {
          if (this.selectedModifier?.options) {
            this.selectedModifier.options = this.selectedModifier.options.filter(o => o.id !== option.id);
          }
        },
        error: (error) => {
        }
      });
    }
  }

  resetModifierForm(): void {
    this.modifierForm.reset({
      name: '',
      code: '',
      description: '',
      priceAdjustment: 0,
      costAdjustment: 0,
      isRequired: false,
      isMultiple: false,
      maxSelections: 1,
      isActive: true,
      displayOrder: 0
    });
    this.isEditing = false;
    this.editingModifierId = undefined;
    this.showAddModifierForm = true;
  }

  resetOptionForm(): void {
    this.optionForm.reset({
      name: '',
      code: '',
      description: '',
      priceAdjustment: 0,
      costAdjustment: 0,
      isActive: true,
      isDefault: false,
      displayOrder: 0
    });
    this.isEditingOption = false;
    this.editingOptionId = undefined;
    this.showAddOptionForm = true;
  }

  cancelEdit(): void {
    this.showAddModifierForm = false;
    this.showAddOptionForm = false;
    this.resetModifierForm();
    this.resetOptionForm();
  }
}
