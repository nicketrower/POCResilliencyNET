using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;
        public RegistrationController(IHttpClientFactory httpClientFactory, GabDbContext db, IUserBL userbl, INotificationClient notificationClient, ILogger<RegistrationController> logger)
        {
            _notificationClient = notificationClient;
            _httpClientFactory = httpClientFactory;
            _db = db;
            _userbl = userbl;
            _logger = logger;
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

            try
            {

                if (ModelState.IsValid)
                {
                    //try
                    //{
                    //    await sqlBreakerPolicy.ExecuteAsync(async () =>
                    //   {
                           _db.UserInfo.Add(userInfo);
                           await _db.SaveChangesAsync();
                    //   });
                    //}
                    //catch (Exception ex)
                    //{
                    //    _logger.LogError(ex.ToString());
                    //    return BadRequest(ex.ToString());
                    //}
                }

                
                try
                {
                    response = await _notificationClient.SendNotification(userInfo.EmailAddress);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    return BadRequest(ex.ToString());
                }


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
                    _logger.LogError(ex.ToString());
                    return BadRequest("Circuit Breaker Policy Invoke to Open State.");
                    
                }
                else if (ex.ToString().Contains("Polly.Bulkhead.BulkheadRejectedException"))
                {
                    _logger.LogError(ex.ToString());
                    return BadRequest("Bulkhead Isolation Policy Invoked.");
                }
                else
                {
                    _logger.LogError(ex.ToString());
                    return BadRequest(ex.ToString());
                }

            }

            _logger.LogError("Unknown Exception Occurred.");
            return BadRequest("Unknown Exception Occurred.");
        }
    }
}
