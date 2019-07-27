using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.Wrap;
using SpecificationTest.Crosscutting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpecificationTest.Pages
{
    class PageHelper
    {
        internal readonly static PolicyWrap WaitForWebElementPolicy = Policy
            .Handle<AssertFailedException>()
            .WaitAndRetryUntilTimeout(
                retryDelay: TimeSpan.FromMilliseconds(100),
                timeout: TimeSpan.FromSeconds(10));

        internal readonly static AsyncPolicyWrap WaitForWebElementPolicyAsync = Policy
            .Handle<AssertFailedException>()
            .WaitAndRetryUntilTimeoutAsync(
                retryDelay: TimeSpan.FromMilliseconds(100),
                timeout: TimeSpan.FromSeconds(10));
    }
}
