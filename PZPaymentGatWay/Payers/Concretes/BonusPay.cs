using Microsoft.AspNetCore.Http;
using PZPaymentGatWay.BObjects;
using PZPaymentGatWay.Payers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZPaymentGatWay.Payers.Concretes
{
    public class BonusPay : CompanyBase, IPay
    {
        public object Pay(PosForm pf, ref string provNumber, ref string provMessage, IHttpContextAccessor accessor = null)
        {
            string expMonth = string.Format("{0:00}", Convert.ToInt32(pf.Month));
            string expYear = string.Format("{0:00}", Convert.ToInt32(pf.Year.ToString().Substring(2, 2)));

            String responseStr = "";
            String format = "{0}={1}&";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(format, "ShopCode", "2927");
            sb.AppendFormat(format, "PurchAmount", Convert.ToDecimal(pf.Price));
            sb.AppendFormat(format, "Currency", "949");
            string provizyonalOrderId = Guid.NewGuid().ToString();
            sb.AppendFormat(format, "OrderId", provizyonalOrderId);//orderId);

            sb.AppendFormat(format, "InstallmentCount", pf.Installments);
            sb.AppendFormat(format, "txnType", "Auth");
            sb.AppendFormat(format, "orgOrderId", "");
            sb.AppendFormat(format, "UserCode", "sedamakapi"); // api için olanı yazdık
            sb.AppendFormat(format, "UserPass", "tE3mx");
            sb.AppendFormat(format, "SecureType", "NonSecure");
            sb.AppendFormat(format, "Pan", pf.CardNumber.ToString());
            sb.AppendFormat(format, "Expiry", expMonth + expYear);
            sb.AppendFormat(format, "Cvv2", pf.SecureCode.ToString());
            sb.AppendFormat(format, "BonusAmount", 0);
            sb.AppendFormat(format, "CardType", pf.CardNumber.ToString().StartsWith("4") ? 0 : 1);
            sb.AppendFormat(format, "Lang", "TR");
            sb.AppendFormat(format, "MOTO", "1");
            //sb.AppendFormat(format, "Description", "yeni işlem"); //Request.Form["Description"]
            //Eğer 3D doğrulaması yapılmış ise aşağıdaki alanlar da gönderilmelidir*/							/*
            //sb.AppendFormat(format, "PayerAuthenticationCode", Request.Form["PayerAuthenticationCode"]);
            //sb.AppendFormat(format, "Eci", Request.Form["Eci"]);//Visa - 05,06 MasterCard 01,02 olabilir	
            //sb.AppendFormat(format, "PayerTxnId", Request.Form["PayerTxnId"]);	
            try
            {
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("https://spos.denizbank.com/mpi/Default.aspx");

                byte[] parameters = System.Text.Encoding.GetEncoding("ISO-8859-9").GetBytes(sb.ToString());
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = parameters.Length;
                System.IO.Stream requeststream = request.GetRequestStream();
                requeststream.Write(parameters, 0, parameters.Length);
                requeststream.Close();
                //Response.Write(sb.ToString());

                System.Net.HttpWebResponse resp = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.StreamReader responsereader = new System.IO.StreamReader(resp.GetResponseStream(), System.Text.Encoding.GetEncoding("ISO-8859-9"));

                responseStr = responsereader.ReadToEnd();
            }
            catch (Exception ex)
            {
                return ErrorMessage = "ErrorMessage=" + ex.Message;
            }

            return responseStr;
        }
    }
}
