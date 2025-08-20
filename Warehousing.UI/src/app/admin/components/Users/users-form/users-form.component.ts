import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Role } from '../../../models/role';
import { Store } from '../../../models/store';
import { UsersService } from '../../../services/users.service';
import { RoleService } from '../../../services/role.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LanguageService } from '../../../../core/services/language.service';
import { ToastrService } from 'ngx-toastr';
import { User } from '../../../models/users';

@Component({
  selector: 'app-users-form',
  standalone: false,
  templateUrl: './users-form.component.html',
  styleUrl: './users-form.component.scss'
})
export class UsersFormComponent implements OnInit {

  userForm!: FormGroup;
  roles: Role[] = [];
  stores: Store[] = [];

  constructor(
    private usersService: UsersService,
    private roleService: RoleService,
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    public lang: LanguageService,
    private toastr: ToastrService
  ) {
    this.stores = this.route.snapshot.data['StoresResolver'];
  }

  ngOnInit(): void {
    this.initializingForm(null);
    this.route.queryParams.subscribe(params => {
      const userId = +params['userId'];
      if (userId) {
        this.usersService.GetUserById(userId)
          .subscribe({
            next: (res) => {
              this.initializingForm(res)
            },
            error: (err) => {
              console.log(err.error);
            }
          });
      }
    });
    this.roleService.GetRoles().subscribe(res => {
      this.roles = res;
    });
  }

  initializingForm(user: User | null) {
    this.userForm = this.fb.group({
      id: [user?.id || 0],
      nameEn: [user?.nameEn || '', Validators.required],
      nameAr: [user?.nameAr || '', Validators.required],
      phone: [user?.phone || ''],
      email: [user?.email || '', [Validators.required, Validators.email]],
      address: [user?.address || ''],
      username: [user?.username || '', Validators.required],
      passwordHash: [user?.passwordHash || '', user ? [] : [Validators.required]], // Only required on create
      roles: [user?.userRoles.map(ur => ur.roleId) || []],
      printHeader: [user?.printHeader || '', Validators.required],
      printFooter: [user?.printFooter || '', Validators.required],
      isActive: [user?.isActive]
    });
  }

  onRoleChange(roleId: number, event: Event): void {
    const isChecked = (event.target as HTMLInputElement).checked;
    const currentRoles = [...this.userForm.get('roles')?.value];

    if (isChecked) {
      currentRoles.push(roleId);
    } else {
      const index = currentRoles.indexOf(roleId);
      if (index > -1) {
        currentRoles.splice(index, 1);
      }
    }

    this.userForm.get('roles')?.setValue(currentRoles);
  }

  get isEditMode(): boolean {
    return this.userForm.get('id')?.value > 0;
  }

  save(): void {
    if (this.userForm.valid) {
      this.usersService.SaveUser(this.userForm.value).subscribe({
        next: (res) => {
          if (res) {
            this.toastr.success('Successfully saved', 'Roles');
            this.router.navigate(['../users'], { relativeTo: this.route })
          }
          else {
            this.toastr.error('Error while saving', 'Role')
          }
        },
        error: (err) => {
          console.log(err.error);
          this.toastr.error(err.error, 'Role')
        }
      });
    }
    else {
      this.toastr.error('Please fill all required fields!', 'Users');
    }
  }

  cancel(): void {
    this.router.navigate(['../users'], { relativeTo: this.route })
  }
}
