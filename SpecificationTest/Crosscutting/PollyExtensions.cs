using Polly;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Crosscutting
{
    internal static class PollyExtensions
    {
        internal static AsyncPolicyWrap WaitAndRetryUntilTimeoutAsync(this PolicyBuilder policyBuilder, 
            TimeSpan retryDelay, TimeSpan timeout)
        {
            var timeoutPolicy = Policy.TimeoutAsync(timeout);
            var retryPolicy = policyBuilder.WaitAndRetryForeverAsync(_ => retryDelay);
            return Policy.WrapAsync(timeoutPolicy, retryPolicy);
        }

        internal static PolicyWrap WaitAndRetryUntilTimeout(this PolicyBuilder policyBuilder,
            TimeSpan retryDelay, TimeSpan timeout)
        {
            var timeoutPolicy = Policy.Timeout(timeout);
            var retryPolicy = policyBuilder.WaitAndRetryForever(_ => retryDelay);
            return Policy.Wrap(timeoutPolicy, retryPolicy);
        }
    }
}
