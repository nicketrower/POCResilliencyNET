import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Userinfo } from '../models/userinfo';

@Injectable({
  providedIn: 'root'
})
export class UserserviceService {

  constructor(private http: HttpClient) { }

  register(user: Userinfo) {
    return this.http.post('http://13.89.138.147/Register/RegisterUser', user);
  }

  getstates() {
    return this.http.get('http://13.89.138.147/Register/GetStates');
  }

}
