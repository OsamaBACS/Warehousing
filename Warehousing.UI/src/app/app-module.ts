import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { Login } from './core/components/login/login';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { JwtInterceptor } from './core/interceptors/jwt-interceptor';
import { MySharedModule } from './shared/my-shared-module';
import { ReactiveFormsModule } from '@angular/forms';
import { LoaderInterceptor } from './core/interceptors/loader-interceptor';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import { ModalModule, BsModalService } from 'ngx-bootstrap/modal';
import { HomeComponent } from './home/home.component';

export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

@NgModule({
  declarations: [
    App,
    Login,
    HomeComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    MySharedModule,
    ReactiveFormsModule,
    ModalModule.forRoot(),
    HttpClientModule,
    TranslateModule.forRoot({
      defaultLanguage: 'en',
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient]
      }
    }),
    BrowserAnimationsModule,
    ToastrModule.forRoot({
      positionClass: 'toast-bottom-right',
      preventDuplicates: true,
      timeOut: 3000,
    }),
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([JwtInterceptor, LoaderInterceptor])),
    BsModalService
  ],
  bootstrap: [App]
})
export class AppModule { }
