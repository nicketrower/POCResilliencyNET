using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using RegistrationAPI.Models;

namespace RegisterUserAPI.Repository
{
    public class UserBL : IUserBL
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserBL(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public UserInfo GetUserInformation(int id)
        {
            return new UserInfo { FirstName = "Nick", LastName = "Trower", City = "Topeka", EmailAddress = "nicketrower@gmail.com", State = "KS", ZipCode = "66610", Id = 1 };
        }

        public async Task<string> PostUserRegistration(UserInfo userInfo)
        {

            //Polly chained policies for this named Http connection is automatically invoked
            var client = _httpClientFactory.CreateClient("ASEConnecter");

            var response = await client.GetStringAsync(client.BaseAddress.ToString() + "/EmailAPI/NewUserRegistration/" + userInfo.EmailAddress);

            return response;
        }

        public List<StateInfo> GetStateList()
        {
            List<StateInfo> stateInfo = new List<StateInfo>();
            stateInfo.Add(new StateInfo { state = "AR" });
            stateInfo.Add(new StateInfo { state = "AL" });
            stateInfo.Add(new StateInfo { state = "AZ" });
            stateInfo.Add(new StateInfo { state = "KS" });
            stateInfo.Add(new StateInfo { state = "TX" });

            return stateInfo;
        }
    }
}
