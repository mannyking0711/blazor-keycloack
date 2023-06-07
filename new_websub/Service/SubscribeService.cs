using Microsoft.Extensions.Logging;
using new_websub.Controllers;
using System;

namespace new_websub.Service
{
    public class SubscribeService
    {
        private readonly ILogger<SubscribeService> _logger;

        public SubscribeService(ILogger<SubscribeService> logger)
        {
            _logger = logger;
        }

        public String OnFailed()
        {
            _logger.LogError("Subscription Failed");
            return "Subscription failed";
        }

        public String OnSuccess()
        {
            _logger.LogInformation("Subscription success");
            return "Subscription success";
        }

        public String OnCreditCardExpired()
        {
            _logger.LogError("Credit Card Expired");
            return "Credit Card Expired";
        }
    }
}
