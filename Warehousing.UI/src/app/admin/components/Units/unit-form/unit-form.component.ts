import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UnitsService } from '../../../services/units.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Unit } from '../../../models/unit';

@Component({
  selector: 'app-unit-form',
  standalone: false,
  templateUrl: './unit-form.component.html',
  styleUrl: './unit-form.component.scss'
})
export class UnitFormComponent implements OnInit {

  unitForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private unitsService: UnitsService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService
  ) {
  }

  ngOnInit(): void {
    this.initializingForm(null);

    this.route.queryParams.subscribe(params => {
      const unitId = +params['unitId'];
      if (unitId) {
        this.unitsService.GetUnitById(unitId).subscribe({
          next: (res) => {
            this.initializingForm(res);
          },
          error: (err) => {
            console.error(err.error);
          }
        });
      }
    });
  }

  initializingForm(unit: Unit | null) {
    this.unitForm = this.fb.group({
      id: [unit ? unit.id : 0],
      code: [unit ? unit.code : '', Validators.required],
      nameEn: [unit ? unit.nameAr : '', Validators.required],
      nameAr: [unit ? unit.nameEn : '', Validators.required],
      description: [unit ? unit.description : ''],
      isActive: [unit ? unit.isActive : true]
    });
  }

  onSubmit() {
    if (this.unitForm.valid) {
      this.unitsService.SaveUnit(this.unitForm.value).subscribe({
        next: (res) => {
          if(res) {
            this.toastr.success('تم اضافة وحدة قياس بنجاح', 'unit');
            this.router.navigate(['../unit'], { relativeTo: this.route });
          }
          else {
            this.toastr.error(res, 'unit')
          }
        },
        error: (err) => {
          this.toastr.error(err.error, 'unit')
        }
      });
    }
  }

  cancel() {
    this.router.navigate(['../unit'], { relativeTo: this.route });
  }
}
