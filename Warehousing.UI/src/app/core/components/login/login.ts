import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

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

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
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
    console.log('Form valid:', this.loginForm.valid);
    console.log('Form errors:', this.loginForm.errors);
    console.log('Username errors:', this.loginForm.get('username')?.errors);
    console.log('Password errors:', this.loginForm.get('password')?.errors);
    
    if (this.loginForm.invalid) {
      console.log('Form is invalid, returning early');
      return;
    }

    // Clear previous error message
    this.errorMessage = '';

    var credentials: { username: string, password: string } = {
      username: this.username.value,
      password: this.password.value
    }
    
    console.log('Sending credentials:', credentials);

    this.authService.login(credentials).subscribe({
      next: (res) => {
        console.log('Login response:', res);
        if(res.success && res.token) {
          const isAdmin = this.authService.isAdmin;
          this.router.navigate([isAdmin ? '/admin' : '/order/2/categories']);
        } else {
          this.errorMessage = res.errorMessage || 'Login failed. Check credentials.';
          console.log('Setting error message:', this.errorMessage);
        }
      },
      error: (err) => {
        console.log('Login error:', err);
        this.errorMessage = err.error.errorMessage;
        console.log('Setting error message (error):', this.errorMessage);
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
