import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserserviceService } from '../../services/userservice.service';
import { Userinfo } from 'src/app/models/userinfo';

@Component({
  selector: 'app-userregistration',
  templateUrl: './userregistration.component.html',
  styleUrls: ['./userregistration.component.css']
})
export class UserregistrationComponent implements OnInit {
  registerForm: FormGroup;
  submitted = false;
  errorEvent = false;
  infoEvent = false;
  successEvent = false;
  timer: any;
  formSubmitState = '';
  errorStatus = '';
  infoStatus = '';

  mask = [/\d/, /\d/, /\d/, /\d/, /\d/, '-', /\d/, /\d/, /\d/, /\d/];
  stateList: any;
  response: any;

  constructor(
    private formBuilder: FormBuilder,
    private userApi: UserserviceService
  ) {}

  ngOnInit() {
    this.userApi.getstates().subscribe(res => {
      this.stateList = res;
    });

    this.registerForm = this.formBuilder.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      emailAddress: ['', [Validators.required, Validators.email]],
      addressInfo: ['', Validators.required],
      cityInfo: ['', Validators.required],
      stateInfo: ['', Validators.required],
      zipCode: ['', Validators.required]
    });
  }

  get f() {
    return this.registerForm.controls;
  }

  onSubmit() {
    this.submitted = true;
    this.infoEvent = true;
    this.successEvent = false;
    this.errorEvent = false;
    this.infoStatus = 'Please wait, saving registration information...';

    // stop here if form is invalid
    if (this.registerForm.invalid) {
      return;
    }

    let userInfo = new Userinfo();
    userInfo.firstName = this.registerForm.get('firstName').value;
    userInfo.lastName = this.registerForm.get('lastName').value;
    userInfo.emailAddress = this.registerForm.get('emailAddress').value;
    userInfo.homeAddress = this.registerForm.get('addressInfo').value;
    userInfo.city = this.registerForm.get('cityInfo').value;
    userInfo.state = this.registerForm.get('stateInfo').value;
    userInfo.zipCode = '66610';

    this.userApi.register(userInfo).subscribe(
      res => {
        this.response = res;
        this.registerForm.reset();
        console.log('success : ' + res);
        this.successEvent = true;
        this.submitted = false;
        this.infoEvent = false;
        this.errorEvent = false;
        this.formSubmitState = 'Success';
      },
      err => {
        //console.log(err.error);
        this.infoEvent = false;
        this.errorEvent = true;
        this.errorStatus = err;
        this.successEvent = false;
        this.errorStatus = err.error + ' , the Register button has been temporarly disabled. Please try again in 10 seconds.';
      

        this.timer = setInterval(() => {
          this.unlockRegistration();
        }, 10000);
        console.log('error : ' + err);
      }
    );
  }

  unlockRegistration() {
    clearInterval(this.timer);
    this.submitted = false;
    //this.errorEvent = false;
  }
}
