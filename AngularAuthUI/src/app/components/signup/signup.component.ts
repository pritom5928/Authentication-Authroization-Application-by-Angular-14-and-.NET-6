import { FormGroup, FormBuilder, Validators, FormControl } from '@angular/forms';
import { Component } from '@angular/core';
import ValidateForm from 'src/app/helpers/validateform';
import { AuthService } from 'src/app/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css']
})
export class SignupComponent {

  isText : boolean = false;
  type : string = "password";
  eyeIcon : string = "fa-eye-slash";
  signUpForm !: FormGroup;
 

  constructor(private fb: FormBuilder, private _auth: AuthService, private _router: Router){

  }

  ngOnInit(): void{
    this.signUpForm = this.fb.group({
      firstName : ['', Validators.required],
      lastName : ['', Validators.required],
      email : ['', Validators.required],
      userName: ['', Validators.required],
      password: ['', Validators.required]
    })
  }


  hideShowPass(){
    this.isText = !this.isText;
    this.type = this.isText ? "text" : "password";
    this.eyeIcon = this.isText ? "fa-eye" : "fa-eye-slash";
  }

  onSignUp(){
    debugger;
    if(this.signUpForm.valid)
    {
      console.log(this.signUpForm.value);
      this._auth.signUp(this.signUpForm.value)
        .subscribe({
          next: (res) =>{
            alert(res.message);
            this.signUpForm.reset();
            this._router.navigate(['login']);
          },
          error: (err) =>{
            alert(err?.error.message)
          } 
        })
    }
    else{
      console.log("Form is not valid!!");
      ValidateForm.validateAllFormFields(this.signUpForm);
      alert("Your form is invalid")
    }
  }

 

}
