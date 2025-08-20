import { Component, OnInit } from '@angular/core';
import { StoreService } from '../../../services/store.service';
import { Observable } from 'rxjs';
import { Store } from '../../../models/store';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';

@Component({
  selector: 'app-store',
  standalone: false,
  templateUrl: './store.component.html',
  styleUrl: './store.component.scss'
})
export class StoreComponent implements OnInit {

  constructor(
    private storeService: StoreService,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService
  ){}

  ngOnInit(): void {
    this.loadStores();
  }

  stores$!: Observable<Store[]>;

  loadStores() {
    this.stores$ = this.storeService.GetStores();
  }

  onEdit(storeId: number | null): void {
    if (storeId) {
      this.router.navigate(['../store-form'], { relativeTo: this.route, queryParams: { storeId } });
    } else {
      this.router.navigate(['../store-form'], { relativeTo: this.route });
    }
  }
}
