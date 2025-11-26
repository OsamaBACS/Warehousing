import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login implements OnInit {
  loginForm: FormGroup;
  errorMessage = '';
  showPassword = false;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    public languageService: LanguageService
  ) {
    this.loginForm = fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    // Fingerprint functionality removed
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  login() {
    if (this.loginForm.invalid) {
      // Mark all fields as touched to show validation errors
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
      return;
    }

    // Clear previous error message
    this.errorMessage = '';
    this.isSubmitting = true;

    const credentials: { username: string, password: string } = {
      username: this.username.value,
      password: this.password.value
    };

    this.authService.login(credentials).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        if(res.success && res.token) {
          const isAdmin = this.authService.isAdmin;
          this.router.navigate([isAdmin ? '/app/admin/dashboard' : '/app/order/2/categories']);
        } else {
          this.errorMessage = res.errorMessage || '';
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.errorMessage || '';
      }
    });
  }

  get username(): FormControl {
    return this.loginForm.get('username') as FormControl;
  }

  get password(): FormControl {
    return this.loginForm.get('password') as FormControl;
  }

}
