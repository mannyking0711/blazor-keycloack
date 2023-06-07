using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using new_websub.Models;
using new_websub.Service;
using new_websub.ViewModels;
using Stripe;
using System;
using System.Collections.Generic;

namespace new_websub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscribeController : Controller
    {
        private readonly SubscribeService _subscribeService;
        private readonly IOptions<StripeSettings> _stripeSettings;

        public SubscribeController(
            IOptions<StripeSettings> stripeSettings,
            SubscribeService subscribeService
        )
        {
            _stripeSettings = stripeSettings;
            _subscribeService = subscribeService;
        }


        private static readonly string ONE_EMPLOYEE_KEY = "price_1NFAjqCwdAzCY2QbubBZLGbm";
        private static readonly string TWO_EMPLOYEE_KEY = "price_1NFAkaCwdAzCY2QbbzCGU9hI";
        private static readonly string THREE_EMPLOYEE_KEY = "price_1NFsWACwdAzCY2QbsuTMFJTs";

        [HttpPost]
        public IActionResult Subscription([FromBody] SubscribeViewModel viewModel)
        {
            #region Get Plan ID

            string planId;
            switch (viewModel.Plan)
            {
                case "one":
                    planId = ONE_EMPLOYEE_KEY;
                    break;
                case "two":
                    planId = TWO_EMPLOYEE_KEY;
                    break;
                case "three":
                    planId = THREE_EMPLOYEE_KEY;
                    break;
                default:
                    return BadRequest("Type is not correct!");
            }

            #endregion

            StripeConfiguration.SetApiKey(_stripeSettings.Value.SecretKey);


            try
            {
                #region Create a new user or Get existing user

                var cusService = new CustomerService();
                var customers = cusService.List(new CustomerListOptions
                {
                    Email = viewModel.Email,
                    Limit = 1
                });
                Customer customer;

                if (customers.Data.Count == 0)
                {
                    customer = cusService.Create(new CustomerCreateOptions
                    {
                        Email = viewModel.Email
                    });
                }
                else
                {
                    customer = customers.Data[0];
                }

                #endregion


                #region Payment Method Attach

                var paymentAttachOption = new PaymentMethodAttachOptions
                {
                    CustomerId = customer.Id
                };
                new PaymentMethodService().Attach(viewModel.PaymentMethodId, paymentAttachOption);

                #endregion


                #region Customer Update by PaymentMethod

                var customerUpdateOption = new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethodId = viewModel.PaymentMethodId
                    }
                };
                cusService.Update(customer.Id, customerUpdateOption);

                #endregion


                #region Subscribe

                var subOptions = new SubscriptionCreateOptions
                {
                    CustomerId = customer.Id,
                    Items = new List<SubscriptionItemOption>
                    {
                        new SubscriptionItemOption
                        {
                            PlanId = planId,
                            Quantity = 1
                        }
                    },
                    DefaultPaymentMethodId = viewModel.PaymentMethodId
                };
                var subscription = new SubscriptionService().Create(subOptions);

                #endregion

                return Ok(_subscribeService.OnSuccess());
            }
            catch (Exception ex)
            {
                return StatusCode(500, _subscribeService.OnFailed());
            }
        }
    }
}