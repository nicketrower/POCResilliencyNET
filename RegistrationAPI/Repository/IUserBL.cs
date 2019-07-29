using Microsoft.AspNetCore.Mvc;
using RegistrationAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegisterUserAPI.Repository
{
    public interface IUserBL
    {
        Task<string> PostUserRegistration(UserInfo userInfo);

        UserInfo GetUserInformation(int id);

        List<StateInfo> GetStateList();
    }
}
