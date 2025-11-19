import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { 
  ProductModifier, 
  ProductModifierCreateRequest, 
  ProductModifierOption,
  ProductModifierOptionCreateRequest,
  ProductModifierGroup,
  ProductModifierGroupCreateRequest
} from '../../../models/ProductModifier';
import { ProductModifierService } from '../../../services/product-modifier.service';

@Component({
  selector: 'app-product-modifiers',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-modifiers.html',
  styleUrls: ['./product-modifiers.scss']
})
export class ProductModifiersComponent implements OnInit {
  @Input() productId!: number;
  @Output() modifiersUpdated = new EventEmitter<ProductModifierGroup[]>();

  modifierGroups: ProductModifierGroup[] = [];
  availableModifiers: ProductModifier[] = [];
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
      modifierId: ['', Validators.required],
      isRequired: [false],
      maxSelections: [1, [Validators.required, Validators.min(1)]],
      displayOrder: [0],
      isActive: [true]
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
    this.loadModifierGroups();
    this.loadAvailableModifiers();
  }

  loadModifierGroups(): void {
    this.loading = true;
    this.modifierService.getModifierGroupsByProduct(this.productId).subscribe({
      next: (groups) => {
        this.modifierGroups = groups;
        this.modifiersUpdated.emit(groups);
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
      }
    });
  }

  loadAvailableModifiers(): void {
    this.modifierService.getAllModifiers().subscribe({
      next: (modifiers) => {
        this.availableModifiers = modifiers;
      },
      error: (error) => {
      }
    });
  }

  loadModifierOptions(modifierId: number): void {
    this.modifierService.getModifierOptions(modifierId).subscribe({
      next: (options) => {
        const modifier = this.availableModifiers.find(m => m.id === modifierId);
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
      const groupData: ProductModifierGroupCreateRequest = {
        productId: this.productId,
        modifierId: formValue.modifierId,
        isRequired: formValue.isRequired,
        maxSelections: formValue.maxSelections,
        displayOrder: formValue.displayOrder,
        isActive: formValue.isActive
      };

      this.modifierService.createModifierGroup(groupData).subscribe({
        next: (group) => {
          this.modifierGroups.push(group);
          this.showAddModifierForm = false; // Hide the form after successful creation
          this.resetModifierForm();
          this.modifiersUpdated.emit(this.modifierGroups);
        },
        error: (error) => {
        }
      });
    }
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

      this.modifierService.createModifierOption(this.selectedModifier.id!, optionData).subscribe({
        next: (option) => {
          if (!this.selectedModifier!.options) {
            this.selectedModifier!.options = [];
          }
          this.selectedModifier!.options.push(option);
          this.showAddOptionForm = false; // Hide the form after successful creation
          this.resetOptionForm();
        },
        error: (error) => {
        }
      });
    }
  }

  selectModifier(modifier: ProductModifier): void {
    this.selectedModifier = modifier;
    this.loadModifierOptions(modifier.id!);
  }

  editModifierGroup(group: ProductModifierGroup): void {
    this.isEditing = true;
    this.showAddModifierForm = false; // Hide add form when editing
    this.editingModifierId = group.id;
    this.modifierForm.patchValue({
      modifierId: group.modifierId,
      isRequired: group.isRequired,
      maxSelections: group.maxSelections,
      displayOrder: group.displayOrder,
      isActive: group.isActive
    });
  }

  editModifierOption(option: ProductModifierOption): void {
    this.isEditingOption = true;
    this.showAddOptionForm = false; // Hide add form when editing
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

  deleteModifierGroup(group: ProductModifierGroup): void {
    if (!group.id) return;

    if (confirm('Are you sure you want to remove this modifier from the product?')) {
      this.modifierService.deleteModifierGroup(group.id).subscribe({
        next: () => {
          this.modifierGroups = this.modifierGroups.filter(g => g.id !== group.id);
          this.modifiersUpdated.emit(this.modifierGroups);
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
      modifierId: '',
      isRequired: false,
      maxSelections: 1,
      displayOrder: 0,
      isActive: true
    });
    this.isEditing = false;
    this.editingModifierId = undefined;
    this.showAddModifierForm = true; // Show the form when adding a new modifier
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
    this.showAddOptionForm = true; // Show the form when adding a new option
  }

  cancelEdit(): void {
    this.showAddModifierForm = false; // Hide the form when canceling
    this.showAddOptionForm = false; // Hide the option form when canceling
    this.resetModifierForm();
    this.resetOptionForm();
  }
}