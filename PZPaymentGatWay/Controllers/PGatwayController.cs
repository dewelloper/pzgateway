using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PZPaymentGatWay.BObjects;
using PZPaymentGatWay.interfaces;
using PZPaymentGatWay.Payers.Concretes;

namespace PZPaymentGatWay.Controllers
{

    [ApiController]
    [Authorize()]
    [Route("api/[controller]")]
    public class PGatwayController : CompanyBase
    {
        private string _provMessage = "";
        private string _provNumber = "";

        private readonly IOrderHistoryRepository _orderHistoryRepository;

        public PGatwayController(IOrderHistoryRepository orderHistoryRepository)
        {
            _orderHistoryRepository = orderHistoryRepository;
        }

        [HttpGet("new")]
        public IActionResult Get()
        {
            foreach (var claim in HttpContext.User.Claims)
            {
                Console.WriteLine("Claim Type: {0}:\nClaim Value:{1}\n", claim.Type, claim.Value);
            }
            var promotionCode = Guid.NewGuid();
            return new ObjectResult($"Your promotion code is {promotionCode}");
        }


        // POST api/PGatway
        [HttpGet("pay")]
        public async Task<ActionResult> Post([FromBody] PosForm postForm)
        {
            //0 unsuccess 1 success
            _provNumber = Helper.RandomNumber();

            object isPaymentSuccess = 0;

            if (postForm.CardType == "Vakıf Bank")
                isPaymentSuccess = Helper.Payer(new VakifPay(), postForm, _provNumber, _provMessage, _accessor);
            else if (postForm.CardType == "Bonus")
            {
                isPaymentSuccess = Helper.Payer(new BonusPay(), postForm, _provNumber, _provMessage, _accessor);

                int errIndex = isPaymentSuccess.ToString().IndexOf("ErrorMessage=");
                int errEndIndex = isPaymentSuccess.ToString().IndexOf(";", errIndex);
                ErrorMessage = isPaymentSuccess.ToString().Substring(errIndex + 13, errEndIndex - (errIndex + 13));

                if (ErrorMessage.Trim() == "")
                    isPaymentSuccess += "|1";
                else isPaymentSuccess += "|0";
            }
            else if (postForm.CardType == "Maximum")
            {
                isPaymentSuccess = Helper.Payer(new MaximumPay(), postForm, _provNumber, _provMessage, _accessor);
                if (isPaymentSuccess.ToString().Contains("Approv"))
                    isPaymentSuccess += "|1";
                else isPaymentSuccess += "|0";
            }
            else if (postForm.CardType == "Finans")
            {
                isPaymentSuccess = Helper.Payer(new MaximumPay(), postForm, _provNumber, _provMessage, _accessor);

                if (isPaymentSuccess.ToString().Contains("Approv"))
                    isPaymentSuccess += "|1";
                else isPaymentSuccess += "|0";
            }

            var result = await _orderHistoryRepository.AddOrderHistory(new OrderHistory()
            {
                PosForm = postForm,
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            });

            if (Convert.ToInt32(isPaymentSuccess.ToString().Substring(isPaymentSuccess.ToString().Length-1, 1)) == 1)
            {
                return new OkObjectResult("Ödeme başarılı!");
            }

            return new ObjectResult(isPaymentSuccess);
        }


    }
}
