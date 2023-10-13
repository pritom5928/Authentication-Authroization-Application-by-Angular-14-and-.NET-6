import { Router } from '@angular/router';
import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http'

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private baseUrl: string = "https://localhost:7056/api/User/";

  constructor(private http : HttpClient, private router : Router) { 

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

  getToken(){
    return localStorage.getItem('token');
  }

  isLoggedIn() : boolean{
    return !!localStorage.getItem('token');
  }

  signOut(){
    localStorage.clear();
    this.router.navigate(['login']);
  }

}
