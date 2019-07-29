using Polly;
using Polly.Extensions.Http;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace TransientPolicies
{
    public class PolicyDefinitions : ITransientPolicies
    {
       
        public AsyncPolicy<HttpResponseMessage> timeOutPolicy(int duration)
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(duration));
        }

        public Polly.Fallback.AsyncFallbackPolicy<HttpResponseMessage> fallBackPolicy(string defaultMsg)
        {
            return Policy.HandleResult<HttpResponseMessage>(r => (int)r.StatusCode == 502)
               .OrResult(r => (int)r.StatusCode == 500)
               .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
               {
                   Content = new ObjectContent(typeof(string), defaultMsg, new JsonMediaTypeFormatter())
               });
        }

        public Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> waitAndRetry()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // HttpRequestException, 5XX and 408
                .OrResult(response => (int)response.StatusCode == 429) // RetryAfter
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                });
        }

        public Polly.Bulkhead.AsyncBulkheadPolicy<HttpResponseMessage> bulkHeadIsolation(int transactions, int queued)
        {
            return Policy.BulkheadAsync<HttpResponseMessage>(transactions, queued);
        }

        public Polly.CircuitBreaker.AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy(int events, int waittime)
        {
            return HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: events,
                durationOfBreak: TimeSpan.FromSeconds(waittime),OnBreak, OnReset, OnHalfOpen);

        }

        public Polly.CircuitBreaker.AsyncCircuitBreakerPolicy sqlCircuitBreakerPolicy()
        {
            return Policy.Handle<SqlException>(ex => ex.Number == 40613)
           .Or<SqlException>(ex => ex.Number == 40197)
           .Or<SqlException>(ex => ex.Number == 40501)
           .Or<SqlException>(ex => ex.Number == 49918)
           .Or<SqlException>(ex => ex.Number == 4060)
           .Or<SqlException>(ex => ex.Number == 40)
           .CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));
        }

        private void OnBreak(DelegateResult<HttpResponseMessage> responseMessage, TimeSpan timeSpan)
        {
            //Log OnBreak
        }

        private void OnReset()
        {
            //Log OnReset
        }

        private void OnHalfOpen()
        {
            //Log OnHalfOpen
        }
    }
}
