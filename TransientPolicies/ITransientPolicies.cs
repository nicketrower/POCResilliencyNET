using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace TransientPolicies
{
    public interface ITransientPolicies
    {
        AsyncPolicy<HttpResponseMessage> timeOutPolicy(int duration);

        Polly.Fallback.AsyncFallbackPolicy<HttpResponseMessage> fallBackPolicy(string defaultMsg);

        Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> waitAndRetry();

        Polly.Bulkhead.AsyncBulkheadPolicy<HttpResponseMessage> bulkHeadIsolation(int transactions, int queued);

        Polly.CircuitBreaker.AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy(int events, int waittime);

        Polly.CircuitBreaker.AsyncCircuitBreakerPolicy sqlCircuitBreakerPolicy();
    }
}
