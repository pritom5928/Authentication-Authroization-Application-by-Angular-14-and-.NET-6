import { Component } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import ValidateForm from 'src/app/helpers/validateform';

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

  constructor(private fb : FormBuilder){

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

  onSubmit(){
    if(this.loginForm.valid){
      console.log(this.loginForm.value);
    }
    else{
      console.log("Form is not valid!!");
      ValidateForm.validateAllFormFields(this.loginForm);
      alert("Your form is invalid")
    }
  }

  
}
