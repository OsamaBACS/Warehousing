import { Component, OnInit } from '@angular/core';
import { UnitsService } from '../../../services/units.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';
import { Observable } from 'rxjs';
import { Unit } from '../../../models/unit';

@Component({
  selector: 'app-unit',
  standalone: false,
  templateUrl: './unit.component.html',
  styleUrl: './unit.component.scss'
})
export class UnitComponent implements OnInit {

  constructor(
    private unitsService: UnitsService,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService
  ){}

  ngOnInit(): void {
    this.loadunits();
  }

  units$!: Observable<Unit[]>;

  loadunits() {
    this.units$ = this.unitsService.GetUnits();
  }

  onEdit(unitId: number | null): void {
    if (unitId) {
      this.router.navigate(['../unit-form'], { relativeTo: this.route, queryParams: { unitId } });
    } else {
      this.router.navigate(['../unit-form'], { relativeTo: this.route });
    }
  }
}
