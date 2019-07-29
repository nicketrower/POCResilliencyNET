using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistrationAPI.TypedClients
{
    public interface INotificationClient
    {
        Task<System.Net.Http.HttpResponseMessage> SendNotification(string emailAddress);
    }
}
