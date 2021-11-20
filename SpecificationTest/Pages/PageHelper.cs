using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using Polly;
using Polly.Wrap;
using SpecificationTest.Crosscutting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Pages
{
    class PageHelper
    {
        internal static readonly PolicyWrap WaitForWebElementPolicy = Policy
            .Handle<RetryException>()
            .WaitAndRetryUntilTimeout(
                retryDelay: TimeSpan.FromMilliseconds(10),
                timeout: TimeSpan.FromSeconds(30));

        internal static readonly AsyncPolicyWrap WaitForWebElementPolicyAsync = Policy
            .Handle<RetryException>()
            .WaitAndRetryUntilTimeoutAsync(
                retryDelay: TimeSpan.FromMilliseconds(10),
                timeout: TimeSpan.FromSeconds(30));

        public sealed class RetryException : Exception
        {
            public RetryException(string message) : base(message)
            {
            }

            public RetryException(string message, Exception innerException) : base(message, innerException)
            {
            }

            public RetryException() : base()
            {
            }
        }
    }
}
