import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import FingerprintJS from '@fingerprintjs/fingerprintjs';

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
  fingerprint: string = '';

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
    FingerprintJS.load().then(fp => {
      fp.get().then(result => {
        this.fingerprint = result.visitorId;
      });
    });
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  login() {
    if (this.loginForm.invalid) return;

    var credentials: { username: string, password: string, fingerprint: string } = {
      username: this.username.value,
      password: this.password.value,
      fingerprint: this.fingerprint
    }

    this.authService.login(credentials).subscribe({
      next: (res) => {
        if(res.status) {
          this.errorMessage = res.errorMessage || '';
        }
        else {
          this.router.navigate(['/home']);
        }
      },
      error: (err) => {
        this.errorMessage = 'Login failed. Check credentials.';
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
