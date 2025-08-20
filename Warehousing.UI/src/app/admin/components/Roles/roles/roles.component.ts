import { Component, OnInit } from '@angular/core';
import { RoleService } from '../../../services/role.service';
import { LanguageService } from '../../../../core/services/language.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Role } from '../../../models/role';

@Component({
  selector: 'app-roles',
  standalone: false,
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.scss'
})
export class RolesComponent implements OnInit {

  roles: Role[] = [];

  constructor(
    private roleService: RoleService,
    public lang: LanguageService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService,
  ) { }

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles() {
    this.roleService.GetRoles().subscribe(res => {
      this.roles = res;
    });
  }

  openForm(roleId: number | null) {
    if (roleId) {
      this.router.navigate(['../roles-form'], { relativeTo: this.route, queryParams: { roleId: roleId } });
    }
    else {
      this.router.navigate(['../roles-form'], { relativeTo: this.route });
    }
  }
}
