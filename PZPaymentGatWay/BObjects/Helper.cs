using Microsoft.AspNetCore.Http;
using PZPaymentGatWay.Payers.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PZPaymentGatWay.BObjects
{
    public class Helper
    {

        private static IHttpContextAccessor _accessor;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _accessor = httpContextAccessor;
        }

        public static string GetIp(IHttpContextAccessor accessor)
        {
            if(accessor != null)
                return accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            return _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        public static bool IsNumeric(string value)
        {
            int result;
            return int.TryParse(value, out result);
        }

        public static bool IsTcKimlik(string tcKimlikNo)
        {
            //var returnvalue = false;

            //if (tcKimlikNo.Length != 11) return false;
            //var charlar = tcKimlikNo.ToCharArray(0, 10);
            //var sayi = charlar.Sum(item => int.Parse(item.ToString(CultureInfo.InvariantCulture)));

            //var sayistr = sayi.ToString(CultureInfo.InvariantCulture);

            //if (sayistr.Substring(sayistr.Length - 1) == tcKimlikNo.Substring(10))
            //{
            //    returnvalue = true;
            //}
            //return returnvalue;

            return true;
        }

        public static string RandomNumber()
        {
            var r = new Random();
            var strRsayi = r.Next(1, 10000000) + String.Format("{0:T}", DateTime.Now).Replace(":", string.Empty);
            return strRsayi;
        }

        public static object Payer(IPay pay, PosForm pf, string provNum, string provMessage, IHttpContextAccessor access)
        {
            return pay.Pay(pf, ref provNum, ref provMessage, access);
        }
    }

}

