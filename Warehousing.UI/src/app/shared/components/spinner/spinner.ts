import { Component } from '@angular/core';
import { Loader } from '../../../core/services/loader';

@Component({
  selector: 'app-spinner',
  standalone: false,
  templateUrl: './spinner.html',
  styleUrl: './spinner.scss'
})
export class Spinner {

  constructor(public loaderService: Loader) {}
}
