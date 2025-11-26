import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class Loader {

  private _loading = new BehaviorSubject<boolean>(false);
  public readonly loading = this._loading.asObservable();
  private activeRequests = 0;

  show() {
    this.activeRequests++;
    if (this.activeRequests > 0) {
      this._loading.next(true);
    }
  }

  hide() {
    this.activeRequests--;
    if (this.activeRequests <= 0) {
      this.activeRequests = 0; // Ensure it doesn't go negative
      this._loading.next(false);
    }
  }

  // Reset in case of errors
  reset() {
    this.activeRequests = 0;
    this._loading.next(false);
  }
}
