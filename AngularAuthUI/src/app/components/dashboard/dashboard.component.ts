import { ApiService } from './../../services/api.service';
import { Component } from '@angular/core';
import { AuthService } from 'src/app/services/auth.service';
import { UserStoreService } from 'src/app/services/user-store.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent {

  public users: any = [];
  public fullName : string = "";
  public role: string = "";

  constructor(
    private _auth: AuthService,
    private _apiService : ApiService,
    private _userStoreService : UserStoreService) {

  }

  ngOnInit(){
    this._apiService.getUsers()
    .subscribe(res=> {
      this.users = res;
    });

    this._userStoreService.getFullNameFromStore()
    .subscribe(res=> {
      const fullNameFromToken = this._auth.getFullNameFromToken();
      this.fullName = res || fullNameFromToken;
    });

    this._userStoreService.getRoleFromStore()
    .subscribe(res=>{
      const roleFromToken = this._auth.getRoleFromToken();
      this.role = res || roleFromToken;
    })

  }

  logOut(){
    this._auth.signOut();
  }
}
