import { Component } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import ValidateForm from 'src/app/helpers/validateform';
import { AuthService } from 'src/app/services/auth.service';
import { UserStoreService } from 'src/app/services/user-store.service';
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  isText : boolean = false;
  type : string = "password";
  eyeIcon : string = "fa-eye-slash";
  loginForm !: FormGroup;

  constructor(private fb: FormBuilder, 
    private _auth: AuthService, 
    private _router: Router,
    private _userStore: UserStoreService){
  }

  ngOnInit() : void{
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }


  hideShowPass(){
    this.isText = !this.isText;
    this.type = this.isText ? "text" : "password";
    this.eyeIcon = this.isText ? "fa-eye" : "fa-eye-slash";
  }

  onLogin(){
    if(this.loginForm.valid){
      console.log(this.loginForm.value);
      this._auth.logIn(this.loginForm.value).subscribe({
        next: (res)=>{
          debugger;
          alert(res.message);
          this._auth.setToken(res.accessToken);
          this._auth.setRefreshToken(res.refreshToken);
          const tokenPayload = this._auth.decodedToken();
          this._userStore.setFullNameForStore(tokenPayload.name);
          this._userStore.setRoleForStore(tokenPayload.role);
          this.loginForm.reset();
          this._router.navigate(['dashboard']);
        },
        error: (err)=>{
          alert(err?.error.message);
        }
      })
    }
    else{
      console.log("Form is not valid!!");
      ValidateForm.validateAllFormFields(this.loginForm);
      alert("Your form is invalid");
    }
  }


}
