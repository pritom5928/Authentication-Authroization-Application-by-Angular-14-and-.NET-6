import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { TokenApiModel } from '../models/token-api.model';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {

  constructor(private _authService: AuthService, private router : Router) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {

    const myToken = this._authService.getToken();

    if(myToken){
      request = request.clone({
        setHeaders: { Authorization: `Bearer ${myToken}` }
      })
    }

    return next.handle(request).pipe(
      catchError((err : any)=> {
        if(err instanceof HttpErrorResponse){
          if(err.status === 401){
            // alert("Token is expired, please login again");
            // this.router.navigate(['login']);

            //handled unauthorized error and refresh token also
            return this.handleUnAuthorizedError(request, next);
          }
        }
        
        return throwError(()=> new Error("Some other error occured"));
      })
    );
  }

  //refresh token
  handleUnAuthorizedError(request : HttpRequest<any>, next : HttpHandler){
    let tokenApiModel = new TokenApiModel();
    tokenApiModel.accessToken = this._authService.getToken()!;
    tokenApiModel.refreshToken = this._authService.getRefreshToken()!;

    return this._authService.renewToken(tokenApiModel)
    .pipe(
      switchMap((data : TokenApiModel)=> {
        this._authService.setToken(data.accessToken);
        this._authService.setRefreshToken(data.refreshToken)

        request = request.clone({
          setHeaders: { Authorization: `Bearer ${data.accessToken}` }
        })
        return next.handle(request);
      }),
      catchError((err)=> {
        return throwError(()=> {
          alert("Token is expired, please login again");
          this.router.navigate(['login']);
        });
      })
    )
  }
}
