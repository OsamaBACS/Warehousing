import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators, FormArray, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-split-general-quantity-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './split-general-quantity-dialog.component.html',
  styleUrls: ['./split-general-quantity-dialog.component.scss']
})
export class SplitGeneralQuantityDialogComponent {
  allocationForm: FormGroup;
  totalAssigned = 0;
  generalQuantity: number;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<SplitGeneralQuantityDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.generalQuantity = data.generalInventory.quantity;
    this.allocationForm = this.fb.group({
      allocations: this.fb.array(
        data.variants.map((v: any) => this.fb.group({
          variantId: [v.id],
          name: [v.name],
          quantity: [0, [Validators.required, Validators.min(0)]]
        }))
      )
    });
    this.onChanges();
  }

  allocations(): FormArray {
    return this.allocationForm.get('allocations') as FormArray;
  }

  onChanges(): void {
    this.allocationForm.valueChanges.subscribe(val => {
      this.totalAssigned = val.allocations
        .map((a: any) => +a.quantity)
        .reduce((a: number, b: number) => a + b, 0);
    });
  }

  isAllocationValid(): boolean {
    return this.totalAssigned === this.generalQuantity && this.allocationForm.valid;
  }

  submit(): void {
    if (!this.isAllocationValid()) return;
    // Output { productId, storeId, generalInventoryId, allocations } structure
    const result = {
      productId: this.data.product.id,
      storeId: this.data.generalInventory.storeId,
      generalInventoryId: this.data.generalInventory.id,
      generalQuantity: this.generalQuantity,
      allocations: this.allocationForm.value.allocations
        .filter((a: any) => +a.quantity > 0)
        .map((a: any) => ({ variantId: a.variantId, quantity: +a.quantity }))
    };
    this.dialogRef.close(result);
  }

  close(): void {
    this.dialogRef.close();
  }
}
