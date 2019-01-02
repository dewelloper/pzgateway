using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace PZPaymentGatWay.BObjects
{
    public class CompanyBase : ControllerBase
    {
        protected const string CompanyPosUser = "";
        protected const string CompanyPosPwd = "";
        protected const string CompanyMemberNo = "";
        protected const string CompanyPosNo = "";
        protected const string CompanyXcip = "";
        protected const string SystemError = "Bankayla bağlantı kurulamadı ! Lütfen daha sonra tekrar deneyin.";

        protected static IHttpContextAccessor _accessor;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _accessor = httpContextAccessor;
        }

        public bool Result { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }

        // Bank return values
        public string Code { get; set; }
        public string GroupId { get; set; }
        public string TransId { get; set; }
        public string RefferenceNo { get; set; }

    }
}
