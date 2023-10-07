import { Component } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import ValidateForm from 'src/app/helpers/validateform';
import { AuthService } from 'src/app/services/auth.service';

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

  constructor(private fb : FormBuilder, private _auth : AuthService, private _router : Router){

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
          alert(res.message);
          this.loginForm.reset();
          this._router.navigate(['dashboard']);
        },
        error: (err)=>{
          alert(err?.error.message)
        }
      })
    }
    else{
      console.log("Form is not valid!!");
      ValidateForm.validateAllFormFields(this.loginForm);
      alert("Your form is invalid")
    }
  }


}
