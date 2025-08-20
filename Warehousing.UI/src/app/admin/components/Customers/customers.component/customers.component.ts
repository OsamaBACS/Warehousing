import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import { Customer } from '../../../models/customer';
import { CustomersService } from '../../../services/customers.service';

@Component({
  selector: 'app-customers.component',
  standalone: false,
  templateUrl: './customers.component.html',
  styleUrl: './customers.component.scss'
})
export class CustomersComponent implements OnInit {

  customers: Customer[] = [];

  constructor(
    private customerService: CustomersService,
    public dialog: MatDialog,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers() {
    this.customerService.GetCustomers().subscribe(res => {
      this.customers = res;
    });
  }

  openForm(customerId: number | null) {
    if (customerId) {
      this.router.navigate(['../customers-form'], { relativeTo: this.route, queryParams: { customerId: customerId } });
    }
    else {
      this.router.navigate(['../customers-form'], { relativeTo: this.route });
    }
  }
}
