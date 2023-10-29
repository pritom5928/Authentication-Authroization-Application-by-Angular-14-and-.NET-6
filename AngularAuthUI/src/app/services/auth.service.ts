import { Router } from '@angular/router';
import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {JwtHelperService} from '@auth0/angular-jwt';
import { TokenApiModel } from '../models/token-api.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private baseUrl: string = "https://localhost:7056/api/User/";
  private userPaylod : any;

  constructor(private http : HttpClient, private router : Router) { 
    this.userPaylod = this.decodedToken();
  }

  signUp(userObj : any){
    return this.http.post<any>(`${this.baseUrl}register`, userObj);
  }

  logIn(loginObj: any){
    return this.http.post<any>(`${this.baseUrl}authenticate`, loginObj);
  }

  setToken(tokenValue : string){
    localStorage.setItem('token', tokenValue);
  }

  setRefreshToken(tokenValue : string){
    localStorage.setItem('refreshToken', tokenValue);
  }

  getToken(){
    return localStorage.getItem('token');
  }

  getRefreshToken() {
    return localStorage.getItem('refreshToken');
  }

  isLoggedIn() : boolean{
    return !!localStorage.getItem('token');
  }

  signOut(){
    localStorage.clear();
    this.router.navigate(['login']);
  }

  decodedToken(){
    const jwtHelper = new JwtHelperService();
    const token = this.getToken()!;
    console.log(jwtHelper.decodeToken(token));
    return jwtHelper.decodeToken(token);
  }

  getFullNameFromToken(){
    if(this.userPaylod)
      return this.userPaylod.name;
  }

  getRoleFromToken() {
    if (this.userPaylod)
      return this.userPaylod.role;
  }

  renewToken(tokenApi : TokenApiModel){
    return this.http.post(`${this.baseUrl}refresh`, tokenApi);
  }

}
