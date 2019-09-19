using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NotificationAPI.Controllers
{
    [Produces("application/json")]
    [Route("Message")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        // GET: api/Notification
        [HttpGet("HealthCheck")]
        public string HealthCheck()
        {
            return "Ok";
        }

        static int _requestCount = 0;

        //Testing purposes I did not put the BL code into a repository
        [HttpGet("NewUserRegistration/{emailAddress}")]
        public async Task<ActionResult<string>> NewUserRegistration(string emailAddress)
        {
            //Delay for 100 milliseconds - simulate additional work load to test a slow service call downstream. 
            await Task.Delay(100);
            _requestCount++;

            //Uncomment conditional statement to test Polly Policies where service is returning intermittently. 
            //if (_requestCount % 5 == 0)
            //{
            //Disabled to Share the Code
            await SendEmail("nicketrower@gmail.com", "Registration", "<p>Thank you for your registration!</p>");
            return Ok(emailAddress);
            //}

            return StatusCode((int)HttpStatusCode.InternalServerError, "Something went wrong");

        }

        private async Task SendEmail(string email, string subject, string htmlContent)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("support@somecompany.com", "Registration");
            var to = new EmailAddress(email);
            var plainTextContent = Regex.Replace(htmlContent, "<[^>]*>", "");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            await client.SendEmailAsync(msg);
        }
    }
}
