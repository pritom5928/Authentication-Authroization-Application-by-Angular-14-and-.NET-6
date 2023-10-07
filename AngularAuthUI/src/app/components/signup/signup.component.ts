import { FormGroup, FormBuilder, Validators, FormControl } from '@angular/forms';
import { Component } from '@angular/core';
import ValidateForm from 'src/app/helpers/validateform';

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

  constructor(private fb : FormBuilder){

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
      console.log(this.signUpForm.value)
    }
    else{
      console.log("Form is not valid!!");
      ValidateForm.validateAllFormFields(this.signUpForm);
      alert("Your form is invalid")
    }
  }

 

}
