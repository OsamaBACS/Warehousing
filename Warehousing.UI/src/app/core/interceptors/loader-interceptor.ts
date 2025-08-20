import { HttpEvent, HttpInterceptorFn } from '@angular/common/http';
import { Loader } from '../services/loader';
import { inject } from '@angular/core';
import { finalize, Observable } from 'rxjs';

export const LoaderInterceptor: HttpInterceptorFn = 
(
  req, 
  next,
  loaderService: Loader = inject(Loader)
) : Observable<HttpEvent<any>> => {
  loaderService.show();
  return next(req).pipe(
    finalize(() => loaderService.hide())
  );
};
