using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RegistrationAPI.TypedClients
{
    public class NotificationClient: INotificationClient
    {
        private HttpClient _client;
        private ILogger<NotificationClient> _logger;

        public NotificationClient(HttpClient client, ILogger<NotificationClient> logger, IConfiguration config)
        {
            _client = client;
            _client.BaseAddress = new Uri($"http://blocknotification.azurewebsites.net/Message/NewUserRegistration/");
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _logger = logger;
        }

        public async Task<HttpResponseMessage> SendNotification(string emailAddress)
        {
            
            string requestEndPoint = _client.BaseAddress + emailAddress;

            return await _client.GetAsync(requestEndPoint);
        }
    }
}
