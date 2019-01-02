using Microsoft.AspNetCore.Http;
using PZPaymentGatWay.BObjects;
using PZPaymentGatWay.Payers.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PZPaymentGatWay.Payers.Concretes
{
    public class VakifPay : CompanyBase, IPay
    {
        public object Pay(PosForm pf, ref string provNumber, ref string provMessage, IHttpContextAccessor accessor = null)
        {
            try
            {
                var b = new byte[5000];
                var encoding = Encoding.GetEncoding("ISO-8859-9");

                var priceBonvert = pf.Price.ToString("0000000000.00").Replace(",", "");

                var installmentsConvert = pf.Installments == -1 ? "00" : String.Format("{0:00}", pf.Installments);

                var yearConvert = pf.Year.ToString(CultureInfo.InvariantCulture);
                yearConvert = yearConvert.Substring(2, 2);

                var monthConvert = string.Format("{0:00}", pf.Month);

                var khipForIp = Helper.GetIp(accessor);

                provMessage = "kullanici=" + CompanyPosUser + "&sifre=" + CompanyPosPwd + "&islem=PRO&uyeno=" + CompanyMemberNo + "&posno=" + CompanyPosNo + "&kkno=" + pf.CardNumber + "&gectar=" + yearConvert + monthConvert + "&cvc=" + string.Format("{0:000}", pf.SecureCode) + "&tutar=" + priceBonvert + "&provno=000000&taksits=" + installmentsConvert + "&islemyeri=I&uyeref=" + provNumber + "&vbref=6527BB1815F9AB1DE864A488E5198663002D0000&khip=" + khipForIp + "&xcip=" + CompanyXcip;

                b.Initialize();
                b = Encoding.ASCII.GetBytes(provMessage);

                var h1 = WebRequest.Create("https://subesiz.vakifbank.com.tr/vpos724v3/?" + provMessage);
                h1.Method = "GET";

                var wr = h1.GetResponse();
                var s2 = wr.GetResponseStream();

                var buffer = new byte[10000];
                int len = 0, r = 1;
                while (r > 0)
                {
                    Debug.Assert(s2 != null, "s2 != null");
                    r = s2.Read(buffer, len, 10000 - len);
                    len += r;
                }
                Debug.Assert(s2 != null, "s2 != null");
                s2.Close();
                var result = encoding.GetString(buffer, 0, len).Replace("\r", "").Replace("\n", "");


                var msgTemplate = new XmlDocument();
                msgTemplate.LoadXml(result);
                var node = msgTemplate.SelectSingleNode("//Cevap/Msg/Kod");
                Debug.Assert(node != null, "node != null");
                var incomingMonthCode = node.InnerText;

                if (incomingMonthCode == "00")
                {
                    node = msgTemplate.SelectSingleNode("//Cevap/Msg/ProvNo");
                    Debug.Assert(node != null, "node != null");
                    var incomingRefCode = node.InnerText;
                    Result = true;
                    RefferenceNo = incomingRefCode;
                }
                else
                {
                    Result = false;
                    ErrorMessage = "";
                    ErrorCode = incomingMonthCode;
                    switch (ErrorCode)
                    {
                        case "97":
                            ErrorMessage = "Sunucu ip adresini Bankanızla paylaşınız...";
                            break;
                        case "F2":
                            ErrorMessage = "Sanal POS inaktif durumda...";
                            break;
                        case "G0":
                            ErrorMessage = "Bankanızı Arayınız veya vpos724@vakifbank.com.tr adresine hatayı iletiniz..02124736060`I ARAYINIZ..";
                            break;
                        case "G5":
                            ErrorMessage = "Yapılan işlemin Sanal POS ta yetkisi yok....";
                            break;
                        case "78":
                            ErrorMessage = "Ödeme denen kart ile taksitli işlem gerçekleştirilemez....";
                            break;
                        case "81":
                            ErrorMessage = "CVV ya da CAVV bilgisinin gönderilmediği durumda alınır..Eksik güvenlik Bilgisi..";
                            break;
                        case "83":
                            ErrorMessage = "Sistem günsonuna girmeyen işlem iade edilmek isteniyor, işlem iptal edilebilir ya da ancak ertesi gün iade edilebilir....";
                            break;
                        case "01":
                            ErrorMessage = "Eski Kayıt...";
                            break;
                    }
                    return 0;
                }
            }
            catch (Exception)
            {
                Result = false;
                ErrorMessage = SystemError;
                return 0;
            }

            return 1;
        }
    }
}
