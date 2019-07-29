using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.CircuitBreaker;
using RegisterUserAPI.DAL;
using RegisterUserAPI.Repository;
using RegistrationAPI.Models;
using RegistrationAPI.TypedClients;
using TransientPolicies;

namespace RegistrationAPI.Controllers
{
    [Produces("application/json")]
    [Route("Register")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private INotificationClient _notificationClient;
        private readonly GabDbContext _db;
        private readonly IUserBL _userbl;
        public RegistrationController(IHttpClientFactory httpClientFactory, GabDbContext db, IUserBL userbl, INotificationClient notificationClient)
        {
            _notificationClient = notificationClient;
            _httpClientFactory = httpClientFactory;
            _db = db;
            _userbl = userbl;
        }

        [HttpGet("GetStates")]
        public List<StateInfo> GetStates()
        {
            return _userbl.GetStateList();
        }


        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromBody] UserInfo userInfo)
        {
            HttpResponseMessage response;

            PolicyDefinitions polly = new PolicyDefinitions();

            var sqlBreakerPolicy = polly.sqlCircuitBreakerPolicy();

            //var client = _httpClientFactory.CreateClient("EmailService");

            try
            {

                if (ModelState.IsValid)
                {
                    try
                    {
                        sqlBreakerPolicy.ExecuteAsync(async () =>
                       {
                           _db.UserInfo.Add(userInfo);
                           await _db.SaveChangesAsync();
                       });
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.ToString());
                    }
                }

                //string requestEndPoint = client.BaseAddress + userInfo.EmailAddress;


                //response = await client.GetAsync(requestEndPoint);

                response = await _notificationClient.SendNotification(userInfo.EmailAddress);


                if (response.IsSuccessStatusCode)
                {
                    string confirmation = await response.Content.ReadAsStringAsync();
                    if (confirmation.Contains("http://notification"))
                    {
                        return BadRequest("Docker Service is down");
                    }
                    return Ok(confirmation);
                }
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("The circuit is now open"))
                {
                    //Return default value on exception. 
                    return BadRequest("Circuit Breaker Policy Invoke to Open State.");
                }
                else if (ex.ToString().Contains("Polly.Bulkhead.BulkheadRejectedException"))
                {
                    return BadRequest("Bulkhead Isolation Policy Invoked.");
                }
                else
                {
                    return BadRequest(ex.ToString());
                }

            }

            return BadRequest("Unknown Exception Occurred.");
        }
    }
}
