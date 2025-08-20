import { Component, OnInit } from '@angular/core';
import { Supplier } from '../../../models/supplier';
import { SupplierService } from '../../../services/supplier.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-suppliers.component',
  standalone: false,
  templateUrl: './suppliers.component.html',
  styleUrl: './suppliers.component.scss'
})
export class SuppliersComponent implements OnInit {

  suppliers: Supplier[] = [];

  constructor(
    private supplierService: SupplierService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers() {
    this.supplierService.GetSuppliers().subscribe(res => {
      this.suppliers = res;
    });
  }

  openForm(supplierId: number | null) {
    if (supplierId) {
      this.router.navigate(['../suppliers-form'], { relativeTo: this.route, queryParams: { supplierId: supplierId } });
    }
    else {
      this.router.navigate(['../suppliers-form'], { relativeTo: this.route });
    }
  }
}