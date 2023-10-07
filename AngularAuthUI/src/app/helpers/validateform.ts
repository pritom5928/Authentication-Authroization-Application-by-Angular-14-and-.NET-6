import { FormControl, FormGroup } from "@angular/forms";

export default class ValidateForm{
    static validateAllFormFields(signUpForm: FormGroup) {
        Object.keys(signUpForm.controls).forEach(field => {
            const control = signUpForm.get(field);

            if (control instanceof FormControl) {
                control.markAsDirty({ onlySelf: true });
            }
            else if (control instanceof FormGroup) {
                this.validateAllFormFields(control);
            }
        });
    }
}