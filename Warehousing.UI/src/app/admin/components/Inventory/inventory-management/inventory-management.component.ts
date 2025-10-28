import { Component, OnInit } from '@angular/core';
import { InventoryService } from '../../../services/inventory.service';
import { StoreService } from '../../../services/store.service';
import { ProductsService } from '../../../services/products.service';
import { AuthService } from '../../../../core/services/auth.service';
import { Inventory } from '../../../models/Inventory';
import { Store } from '../../../models/store';
import { Product } from '../../../models/product';

@Component({
  selector: 'app-inventory-management',
  standalone: false,
  templateUrl: './inventory-management.component.html',
  styleUrl: './inventory-management.component.scss'
})
export class InventoryManagementComponent implements OnInit {

  inventories: Inventory[] = [];
  stores: Store[] = [];
  products: Product[] = [];
  selectedStoreId: number | null = null;
  selectedProductId: number | null = null;
  loading = false;
  searchKeyword = '';

  constructor(
    private inventoryService: InventoryService,
    private storeService: StoreService,
    private productService: ProductsService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadStores();
    this.loadProducts();
    this.loadInventories();
  }

  loadStores(): void {
    this.storeService.GetStores().subscribe({
      next: (stores) => this.stores = stores,
      error: (err) => console.error('Error loading stores:', err)
    });
  }

  loadProducts(): void {
    this.productService.GetProducts().subscribe({
      next: (products) => {
        // Filter products based on user permissions
        this.products = products.filter(product => this.authService.hasProduct(product.id!));
      },
      error: (err) => console.error('Error loading products:', err)
    });
  }

  loadInventories(): void {
    this.loading = true;
    
    if (this.selectedStoreId) {
      this.inventoryService.GetInventoryByStore(this.selectedStoreId).subscribe({
        next: (inventories) => {
          this.inventories = inventories;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading store inventory:', err);
          this.loading = false;
        }
      });
    } else if (this.selectedProductId) {
      this.inventoryService.GetInventoryByProduct(this.selectedProductId).subscribe({
        next: (inventories) => {
          this.inventories = inventories;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading product inventory:', err);
          this.loading = false;
        }
      });
    } else {
      this.inventoryService.GetAllInventory().subscribe({
        next: (inventories) => {
          this.inventories = inventories;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading all inventory:', err);
          this.loading = false;
        }
      });
    }
  }

  onStoreChange(): void {
    this.selectedProductId = null;
    this.loadInventories();
  }

  onProductChange(): void {
    this.selectedStoreId = null;
    this.loadInventories();
  }

  onSearchChange(): void {
    // Implement search functionality
    this.loadInventories();
  }

  clearFilters(): void {
    this.selectedStoreId = null;
    this.selectedProductId = null;
    this.searchKeyword = '';
    this.loadInventories();
  }

  adjustInventory(inventory: Inventory, newQuantity: number): void {
    if (newQuantity !== inventory.quantity) {
      this.inventoryService.AdjustInventory(inventory.id, newQuantity, 'Manual adjustment').subscribe({
        next: (response) => {
          console.log('Inventory adjusted successfully:', response);
          this.loadInventories(); // Reload data
        },
        error: (err) => {
          console.error('Error adjusting inventory:', err);
        }
      });
    }
  }

  getProductName(productId: number): string {
    const product = this.products.find(p => p.id === productId);
    return product ? product.nameAr : 'Unknown Product';
  }

  getStoreName(storeId: number): string {
    const store = this.stores.find(s => s.id === storeId);
    return store ? store.nameAr : 'Unknown Store';
  }

  getLowStockItems(): void {
    this.loading = true;
    this.inventoryService.GetLowStockItems().subscribe({
      next: (inventories) => {
        this.inventories = inventories;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading low stock items:', err);
        this.loading = false;
      }
    });
  }

  viewHistory(inventory: Inventory): void {
    // TODO: Implement inventory transaction history view
    console.log('View history for inventory:', inventory);
  }
}
