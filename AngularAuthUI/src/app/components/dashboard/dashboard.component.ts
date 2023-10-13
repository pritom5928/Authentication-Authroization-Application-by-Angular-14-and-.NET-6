import { ApiService } from './../../services/api.service';
import { Component } from '@angular/core';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent {

  public users: any = [];

  constructor(
    private _auth: AuthService,
    private _apiService : ApiService) {

  }

  ngOnInit(){
    this._apiService.getUsers()
    .subscribe(res=> {
      this.users = res;
    });
  }

  logOut(){
    this._auth.signOut();
  }

}
