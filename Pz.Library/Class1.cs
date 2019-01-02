using System;

namespace Pz.Library
{
    public class Class1
    {
        #region Property

        // CompanyBankPosKnowledges
        private const string CompanyPosUser = "0001";
        private const string CompanyPosPwd = "44766657";
        private const string CompanyMemberNo = "000092247";
        private const string CompanyPosNo = "VP009235";
        private const string CompanyXcip = "CSZDRDR4HK";
        private const string SystemError = "Bankayla bağlantı kurulamadı ! Lütfen daha sonra tekrar deneyin.";

        private PosForm pForm = null;

        public bool Result { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }

        // Bank return values
        public string Code { get; set; }
        public string GroupId { get; set; }
        public string TransId { get; set; }
        public string RefferenceNo { get; set; }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                this.RadioButtonList1.Items.Add(new ListItem(String.Format("<img src=\"Images/world_logo_2.png\" class=\"img-responsive\"/>")));
                this.RadioButtonList1.Items.Add(new ListItem(String.Format("<img src=\"Images/bonus_logo_2.gif\" class=\"img-responsive\"/>")));
                this.RadioButtonList1.Items.Add(new ListItem(String.Format("<img src=\"Images/maximum_logo_2.gif\" class=\"img-responsive\"/>")));
                this.RadioButtonList1.Items.Add(new ListItem(String.Format("<img src=\"Images/finans_logo.png\" class=\"img-responsive\" />")));
                this.RadioButtonList1.Items.Add(new ListItem(String.Format("<img src=\"Images/digerkartlar.jpg\" class=\"img-responsive\"/>")));

                int index = 0;
                foreach (ListItem item in RadioButtonList1.Items)
                {
                    item.Attributes.Add("onclick", "ChangePaymentBarVisibility(" + index++ + ");");
                }

                SetSecureProtocol(true);
                //LoadInstallments();
            }
        }

        public static void SetSecureProtocol(bool bSecure)
        {
            string redirectUrl = null;
            var context = HttpContext.Current;
            // if we want HTTPS and it is currently HTTP
            if (bSecure && !context.Request.IsSecureConnection)
                redirectUrl = context.Request.Url.ToString().Replace("http:", "https:");
            else
                // if we want HTTP and it is currently HTTPS
                if (!bSecure && context.Request.IsSecureConnection)
                redirectUrl = context.Request.Url.ToString().Replace("https:", "http:");
            //else in all other cases we don't need to redirect
            // check if we need to redirect, and if so use redirectUrl to do the job
            if (redirectUrl != null && !redirectUrl.Contains("Payment"))
                context.Response.Redirect(redirectUrl);
        }

        string cardType = "";

        protected void btnCrditCardPay_Click(object sender, EventArgs e)
        {
            bool isBankSelected = false;
            foreach (ListItem li in RadioButtonList1.Items)
            {
                isBankSelected = li.Selected == true;
                if (isBankSelected)
                    break;
            }

            string instHolder = Request.Form["instHolder"].ToString().Trim();
            string Lira = Request.Form["Lira"].ToString().Trim();
            string cardnumber = Request.Form["cardnumber"].ToString().Trim();
            string cardOwner = Request.Form["cardOwner"].ToString().Trim();
            string cvv = Request.Form["cvv"].ToString().Trim();
            if (!isBankSelected && instHolder == "" || Lira == "" || Lira == "0" || cardnumber == "" || cardnumber.Length < 16 ||
                cardOwner == "" || cvv == "" || cvv.Length < 3)
            {
                Response.Write("<script language=javascript>alert('Lütfen formdaki alanların tümünü doldurun!');</script>");
                return;
            }

            var fixArray = new string[8];
            fixArray[0] = String.Format("{0}", Request.Form["instHolder"]);
            fixArray[1] = String.Format("{0}", Request.Form["Lira"] + "," + Request.Form["Kurus"]);
            fixArray[2] = String.Format("{0}", Request.Form["cardnumber"]);
            fixArray[3] = String.Format("{0}", Request.Form["cardOwner"]);
            fixArray[4] = String.Format("{0}", Request.Form["month"]);
            fixArray[5] = String.Format("{0}", Request.Form["year"]);
            fixArray[6] = String.Format("{0}", Request.Form["cvv"]);
            fixArray[7] = String.Format("{0}", Request.Form["Expl"]);

            if (ConfirmOrder(fixArray))
            {
                var cari = (Cari)base.Session["cari"];
                lblFirmaAdi.Text = cari._firmAdi;
                lblFirmaKodu.Text = cari._firmKodu;
                lblGecerlilikTarihi.Text = pForm.Month.ToString() + "/" + pForm.Year.ToString();
                lblKartBankasi.Text = cardType;
                lblKartNumarasi.Text = pForm.CardNumber.ToString().Substring(0, 6) + "****" + pForm.CardNumber.ToString().Substring(12, 4);
                lblKartSahibiAdi.Text = pForm.CardOwner.ToString();
                lblProvizyon.Text = provizyonNumarasi;
                lblTaksit.Text = pForm.Installments.ToString();
                lblTarih.Text = DateTime.Now.ToString();
                lblTutar.Text = pForm.Price.ToString();

                SqlConnection connection = new SqlConnection(AddInConnProvider.GetConn());
                connection.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "InsertProceeds";
                cmd.Connection = connection;
                cmd.Parameters.AddWithValue("@FirmName", lblFirmaAdi.Text);
                cmd.Parameters.AddWithValue("@FirmCode", lblFirmaKodu.Text);
                cmd.Parameters.AddWithValue("@CardBankName", lblKartBankasi.Text);
                cmd.Parameters.AddWithValue("@CardNumber", pForm.CardNumber.ToString());
                cmd.Parameters.AddWithValue("@CardOwner", lblKartSahibiAdi.Text);
                cmd.Parameters.AddWithValue("@ProvisionNumber", lblProvizyon.Text + "|" + lblGecerlilikTarihi.Text + "|" + fixArray[6].ToString());
                cmd.Parameters.AddWithValue("@Installment", Convert.ToInt32(lblTaksit.Text));
                cmd.Parameters.AddWithValue("@TotalPrice", Convert.ToDecimal(lblTutar.Text));
                cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }

                ScriptManager.RegisterStartupScript(this, GetType(), "PrintVisible", "PrintVisible(''); alert('Ödeme işleminiz başarılı şekilde gerçekleştirilmiştir... Teşekkürler!');", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "print", "alert('Ödeme başarısız olmuştur..! " + ErrorMessage + "');", true);
            }
        }

        //private void LoadInstallments()
        //{
        //    SqlConnection connection = new SqlConnection(AddInConnProvider.GetConn());
        //    connection.Open();

        //    SqlCommand cmd = new SqlCommand();
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.CommandText = "SelectProceedsByName";
        //    cmd.Connection = connection;
        //    //var cmd = new SqlCommand("InsertProceeds", connection);
        //    cmd.Parameters.AddWithValue("@FirmCode", Session["FirmCode"]);
        //    try
        //    {
        //        SqlDataAdapter da = new SqlDataAdapter();
        //        DataTable dt = new DataTable();
        //        da.SelectCommand = cmd;
        //        da.Fill(dt);
        //        grdInstallments.DataSource = dt;
        //        grdInstallments.DataBind();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        connection.Close();
        //    }
        //}

        private static void SendEMail(string subject, string body)
        {
            var client = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = false,
                Host = "smtp.sedasanoto.com.tr",
                Port = 587
            };

            var credentials = new NetworkCredential("odeme@sedasanoto.com.tr", "*123Qwe*");
            client.UseDefaultCredentials = false;
            client.Credentials = credentials;

            var msg = new MailMessage { From = new MailAddress("odeme@sedasanoto.com.tr") };
            msg.CC.Add(new MailAddress("muhasebe@sedaford.com"));
            msg.To.Add(new MailAddress("riza_akin@hotmail.com"));
            msg.To.Add(new MailAddress("sedaford@hotmail.com"));
            msg.To.Add(new MailAddress("nur.kumru@sedaford.com"));

            msg.Subject = subject;
            msg.IsBodyHtml = true;
            msg.Body = body;

            client.Send(msg);
        }

        string provizyonForAll = "";

        public bool ConfirmOrder(string[] collection)
        {
            string errMessage = "";

            bool basarilimi = false;
            var txtManyInstallment = collection[0] == "" ? "1" : collection[0];
            var price = Convert.ToDecimal(collection[1]);
            var cardNumber = Convert.ToInt64(collection[2]);
            var cardOwner = collection[3];
            var month = Convert.ToInt32(collection[4]);
            var year = Convert.ToInt32(collection[5]);
            DateTime monthyear = new DateTime(year, month, 1);
            var secureCode = Convert.ToInt32(collection[6]);
            // ReSharper disable InconsistentNaming
            var Price = Convert.ToDouble(price);
            // ReSharper restore InconsistentNaming
            var installments = Convert.ToInt32((object)txtManyInstallment);

            pForm = new PosForm
            {
                CardNumber = cardNumber,
                CardOwner = cardOwner,
                Month = month,
                Year = year,
                SecureCode = secureCode,
                Price = Price,
                Installments = installments
            };

            //0 unsuccess 1 success
            provizyonNumarasi = Helper.RandomNumber();
            int isPaymentSuccess = 0;
            if (RadioButtonList1.Items[0].Selected)
            {
                cardType = "Vakıf Bank";
                //isPaymentSuccess = PayWithVakifBank(pForm);
                isPaymentSuccess = PayWithVakifBankNew(pForm);
            }
            else if (RadioButtonList1.Items[1].Selected || RadioButtonList1.Items[4].Selected)
            {
                cardType = "Bonus";
                string expMonth = string.Format("{0:00}", Convert.ToInt32(pForm.Month));
                string expYear = string.Format("{0:00}", Convert.ToInt32(pForm.Year.ToString().Substring(2, 2)));

                string msg = PayWithBonus(Convert.ToDecimal(pForm.Price)
                    , 0, pForm.Installments, pForm.CardNumber.ToString()
                    , expMonth + expYear, pForm.SecureCode.ToString()
                    , cardNumber.ToString().StartsWith("4") ? 0 : 1); // visa 0, master 1
                int errIndex = msg.IndexOf("ErrorMessage=");
                int errEndIndex = msg.IndexOf(";", errIndex);
                ErrorMessage = msg.Substring(errIndex + 13, errEndIndex - (errIndex + 13));
                if (ErrorMessage.Trim() == "")
                    isPaymentSuccess = 1;
                else isPaymentSuccess = 0;
            }
            else if (RadioButtonList1.Items[2].Selected)
            {
                cardType = "Maximum";
                string expMonth = string.Format("{0:00}", Convert.ToInt32(pForm.Month));
                string expYear = string.Format("{0:00}", Convert.ToInt32(pForm.Year.ToString().Substring(2, 2)));


                string msg = PayWithMaximum(Convert.ToDecimal(pForm.Price)
                    , 0, pForm.Installments, pForm.CardNumber.ToString()
                    , String.Format("{0:000}", Convert.ToInt32(pForm.SecureCode))
                    , cardNumber.ToString().StartsWith("4") ? 0 : 1, monthyear); // visa 0, master 1
                if (msg.Contains("Approv"))
                    isPaymentSuccess = 1;
                else isPaymentSuccess = 0;
            }
            else if (RadioButtonList1.Items[3].Selected)
            {
                cardType = "Finans";
                string expMonth = string.Format("{0:00}", Convert.ToInt32(pForm.Month));
                string expYear = string.Format("{0:00}", Convert.ToInt32(pForm.Year.ToString().Substring(2, 2)));

                string msg = PayWithFinans(Convert.ToDecimal(pForm.Price)
                    , 0, pForm.Installments, pForm.CardNumber.ToString()
                    , String.Format("{0:000}", Convert.ToInt32(pForm.SecureCode))
                    , cardNumber.ToString().StartsWith("4") ? 0 : 1, monthyear); // visa 0, master 1
                if (msg.Contains("Approv"))
                    isPaymentSuccess = 1;
                else isPaymentSuccess = 0;
            }
            #region Order

            var currentDate = DateTime.Now;
            var explanation = "";
            var exp = collection[7];
            if (exp != null) { explanation = exp; }
            var cari = (Cari)base.Session["cari"];
            var instType = "";
            switch (installments)
            {
                case 0:
                    instType = "Tek Çekim";
                    break;
                case 2:
                    instType = "2+4 Taksit";
                    break;
                case 3:
                    instType = "3+4 Taksit";
                    break;
                case 4:
                    instType = "4+4 Taksit";
                    break;
            }

            var payInf = "Ödeme başarısız!";
            if (isPaymentSuccess == 1)
            {
                payInf = "Ödeme başarılı!";
                basarilimi = true;
            }
            else
            {
                HttpContext.Current.Session["ErrorMessages"] = errMessage + "  " + ErrorMessage;
                return false;
            }

            var orderMail = "Ödeme yapan: " + cari._firmAdi + "<br/>Firma Kodu" + cari._firmKodu + "<br/>Kart Tipi:" + cardType + "<br/> *** Ödeme Tutarı : " + price + "<br/> *** Ödeme Şekli : " + instType + " <br/>**** Ödeme Tarihi : " + currentDate + "<br/> **** Ödeme Açıklaması : " + explanation + "<br/>Provizyon no: " + provizyonForAll;
            var subject = "Seda Ford Online Ödeme ! " + payInf;
            var body = orderMail;
            try
            {
                SendEMail(subject, body);
                HttpContext.Current.Session["Message"] = "Mail Gönderildi.";
            }
            catch (Exception ex)
            {
                HttpContext.Current.Session["Message"] = "Mail gönderilirken hata oluştu." + ex.Message;
            }
            #endregion

            HttpContext.Current.Session["ErrorMessages"] = ErrorCode + " ~ " + payInf.ToUpper() + " ~ " + ErrorMessage;

            return basarilimi;
        }

        #region Banks

        public int PayWithVakifBankNew(PosForm pf)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);

            XmlElement rootNode = xmlDoc.CreateElement("VposRequest");
            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);
            xmlDoc.AppendChild(rootNode);

            //eklemek istediğiniz diğer elementleride bu şekilde ekleyebilirsiniz.
            XmlElement merchantNode = xmlDoc.CreateElement("MerchantId");
            XmlElement passwordNode = xmlDoc.CreateElement("Password");
            XmlElement terminalNode = xmlDoc.CreateElement("TerminalNo");
            XmlElement transactionTypeNode = xmlDoc.CreateElement("TransactionType");
            XmlElement transactionIdNode = xmlDoc.CreateElement("TransactionId");
            XmlElement currencyAmountNode = xmlDoc.CreateElement("CurrencyAmount");
            XmlElement currencyCodeNode = xmlDoc.CreateElement("CurrencyCode");
            XmlElement panNode = xmlDoc.CreateElement("Pan");
            XmlElement cvvNode = xmlDoc.CreateElement("Cvv");
            XmlElement expiryNode = xmlDoc.CreateElement("Expiry");
            XmlElement ClientIpNode = xmlDoc.CreateElement("ClientIp");
            XmlElement transactionDeviceSourceNode = xmlDoc.CreateElement("TransactionDeviceSource");
            XmlElement installmentCountNode = xmlDoc.CreateElement("NumberOfInstallments");

            //yukarıda eklediğimiz node lar için değerleri ekliyoruz.
            XmlText merchantText = xmlDoc.CreateTextNode("000000000092247");
            XmlText passwordtext = xmlDoc.CreateTextNode("CSZDRDR4HK");//Se4711077   CSZDRDR4HK    //old pwd 44766657
            XmlText terminalNoText = xmlDoc.CreateTextNode("VP009235");
            XmlText transactionTypeText = xmlDoc.CreateTextNode("Sale");
            XmlText transactionIdText = xmlDoc.CreateTextNode(Guid.NewGuid().ToString("N")); //uniqe olacak şekilde düzenleyebilirsiniz.
            var priceBonvert = pf.Price.ToString("###.00").Replace(",", ".");
            XmlText currencyAmountText = xmlDoc.CreateTextNode(priceBonvert); //"90.50" tutarı nokta ile gönderdiğinizden emin olunuz.
            XmlText currencyCodeText = xmlDoc.CreateTextNode("949");
            XmlText panText = xmlDoc.CreateTextNode(pf.CardNumber.ToString());
            XmlText cvvText = xmlDoc.CreateTextNode(pf.SecureCode.ToString());
            XmlText expiryText = xmlDoc.CreateTextNode(pf.Year.ToString() + pf.Month.ToString("00"));//"201611"
            var khipForIp = GetIp(); // will be replaced on live with GetIp
            XmlText ClientIpText = xmlDoc.CreateTextNode(khipForIp);//"190.20.13.12"
            XmlText transactionDeviceSourceText = xmlDoc.CreateTextNode("0");
            if (pf.Installments.ToString().Trim() == "" || pf.Installments.ToString().Trim() == "0")
                pf.Installments = 1;
            XmlText installmentCountText = null;
            if (pf.Installments > 1)
            {
                installmentCountText = xmlDoc.CreateTextNode(pf.Installments.ToString());
            }

            rootNode.AppendChild(merchantNode);
            rootNode.AppendChild(passwordNode);
            rootNode.AppendChild(terminalNode);
            rootNode.AppendChild(transactionTypeNode);
            rootNode.AppendChild(transactionIdNode);
            rootNode.AppendChild(currencyAmountNode);
            rootNode.AppendChild(currencyCodeNode);
            rootNode.AppendChild(panNode);
            rootNode.AppendChild(cvvNode);
            rootNode.AppendChild(expiryNode);
            rootNode.AppendChild(ClientIpNode);
            rootNode.AppendChild(transactionDeviceSourceNode);
            if (pf.Installments > 1)
                rootNode.AppendChild(installmentCountNode);

            merchantNode.AppendChild(merchantText);
            passwordNode.AppendChild(passwordtext);
            terminalNode.AppendChild(terminalNoText);
            transactionTypeNode.AppendChild(transactionTypeText);
            transactionIdNode.AppendChild(transactionIdText);
            currencyAmountNode.AppendChild(currencyAmountText);
            currencyCodeNode.AppendChild(currencyCodeText);
            panNode.AppendChild(panText);
            cvvNode.AppendChild(cvvText);
            expiryNode.AppendChild(expiryText);
            ClientIpNode.AppendChild(ClientIpText);
            transactionDeviceSourceNode.AppendChild(transactionDeviceSourceText);
            if (pf.Installments > 1)
                installmentCountNode.AppendChild(installmentCountText);

            string xmlMessage = xmlDoc.OuterXml;

            byte[] dataStream = Encoding.UTF8.GetBytes("prmstr=" + xmlMessage);
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create("https://onlineodeme.vakifbank.com.tr:4443/VposService/v3/Vposreq.aspx");//Vpos adresi
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = dataStream.Length;
            webRequest.KeepAlive = false;
            string responseFromServer = "";

            using (Stream newStream = webRequest.GetRequestStream())
            {
                newStream.Write(dataStream, 0, dataStream.Length);
                newStream.Close();
            }

            using (WebResponse webResponse = webRequest.GetResponse())
            {
                using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    responseFromServer = reader.ReadToEnd();
                    reader.Close();
                }

                webResponse.Close();
            }

            if (string.IsNullOrEmpty(responseFromServer))
            {
                return 0;
            }

            var xmlResponse = new XmlDocument();
            xmlResponse.LoadXml(responseFromServer);
            var resultCodeNode = xmlResponse.SelectSingleNode("VposResponse/ResultCode");
            var resultDescriptionNode = xmlResponse.SelectSingleNode("VposResponse/ResultDescription");
            string resultCode = "";
            string resultDescription = "";

            if (resultCodeNode != null)
            {
                resultCode = resultCodeNode.InnerText;
            }
            if (resultDescriptionNode != null)
            {
                resultDescription = resultDescriptionNode.InnerText;
            }

            if (xmlResponse.SelectSingleNode("VposResponse/ResultCode").InnerText != "0000")
            {
                ErrorMessage = xmlResponse.SelectSingleNode("VposResponse/ResultDetail").InnerText;
                return 0;
            }

            return 1;
        }

        string provizyonMesaji = "";
        string provizyonNumarasi = "";
        public int PayWithVakifBank(PosForm pf)
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
                //yearConvert + monthConvert => 0000
                //CCV => 000
                //Price => 000000000100
                // Helper.RandomNumber => 200501011234567890
                //KHip =>195.0.0.24
                //test
                //pf.CardNumber = 4938410000000000;
                var khipForIp = GetIp(); // will be replaced on live with GetIp
                //string provizyonMesaji = "kullanici=" + _companyPosUser + "&sifre=" + _companyPosPwd + "&islem=PRO&uyeno=" + _companyMemberNo + "&posno=" + _companyPosNo + "&kkno=" + pf.CardNumber + "&gectar=" + yearConvert + monthConvert + "&cvc=" + string.Format("{0:000}", pf.SecureCode) + "&tutar=" + priceBonvert + "&provno=000000&taksits=" + installmentsConvert + "&islemyeri=I&uyeref=" + Helper.RandomNumber() + "&vbref=6527BB1815F9AB1DE864A488E5198663002D0000&khip=" + Helper.GetIp() + "&xcip=" + _companyXcip;

                provizyonForAll = provizyonNumarasi;
                provizyonMesaji = "kullanici=" + CompanyPosUser + "&sifre=" + CompanyPosPwd + "&islem=PRO&uyeno=" + CompanyMemberNo + "&posno=" + CompanyPosNo + "&kkno=" + pf.CardNumber + "&gectar=" + yearConvert + monthConvert + "&cvc=" + string.Format("{0:000}", pf.SecureCode) + "&tutar=" + priceBonvert + "&provno=000000&taksits=" + installmentsConvert + "&islemyeri=I&uyeref=" + provizyonNumarasi + "&vbref=6527BB1815F9AB1DE864A488E5198663002D0000&khip=" + khipForIp + "&xcip=" + CompanyXcip;

                b.Initialize();
                // ReSharper disable RedundantAssignment
                b = Encoding.ASCII.GetBytes(provizyonMesaji);
                // ReSharper restore RedundantAssignment

                var h1 = WebRequest.Create("https://subesiz.vakifbank.com.tr/vpos724v3/?" + provizyonMesaji);
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

        public string PayWithBonus(decimal price, int orderId, int instCount, string cardNumber,
            string exprDate, string ccv2, int visaOrMaster)
        {
            String responseStr = "";
            String format = "{0}={1}&";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(format, "ShopCode", "2927");
            sb.AppendFormat(format, "PurchAmount", price);
            sb.AppendFormat(format, "Currency", "949");
            string provizyonalOrderId = Guid.NewGuid().ToString();
            provizyonForAll = provizyonalOrderId;
            sb.AppendFormat(format, "OrderId", provizyonalOrderId);//orderId);
            if (RadioButtonList1.Items[4].Selected)
                instCount = 1;
            sb.AppendFormat(format, "InstallmentCount", instCount);
            sb.AppendFormat(format, "txnType", "Auth");
            sb.AppendFormat(format, "orgOrderId", "");
            sb.AppendFormat(format, "UserCode", "sedamakapi"); // api için olanı yazdık
            sb.AppendFormat(format, "UserPass", "tE3mx");
            sb.AppendFormat(format, "SecureType", "NonSecure");
            sb.AppendFormat(format, "Pan", cardNumber);
            sb.AppendFormat(format, "Expiry", exprDate);
            sb.AppendFormat(format, "Cvv2", ccv2);
            sb.AppendFormat(format, "BonusAmount", 0);
            sb.AppendFormat(format, "CardType", visaOrMaster);
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

        public string PayWithMaximum(decimal price, int orderId, int instCount, string cardNumber,
            string ccv2, int visaOrMaster, DateTime date)
        {
            ///start
            ///
            var mycc5pay = new ePayment.cc5payment();

            mycc5pay.host = "https://sanalpos.isbank.com.tr/servlet/cc5ApiServer";
            //mycc5pay.host="https://testsanalpos.est.com.tr/servlet/cc5ApiServer";
            mycc5pay.name = "sanalpos";
            mycc5pay.password = "Dimple943*";
            mycc5pay.clientid = "700659404610";
            mycc5pay.orderresult = 0;

            string provizyonalOrderId = Guid.NewGuid().ToString();
            provizyonForAll = provizyonalOrderId;
            mycc5pay.oid = provizyonalOrderId;
            mycc5pay.cardnumber = cardNumber;
            mycc5pay.expmonth = date.Month.ToString();
            mycc5pay.expyear = date.Year.ToString();
            mycc5pay.cv2 = ccv2;
            mycc5pay.subtotal = price.ToString().Replace(",", ".");
            mycc5pay.currency = "949";
            mycc5pay.chargetype = "Auth";

            mycc5pay.taksit = instCount.ToString() == "0" ? "1" : instCount.ToString();


            string processResult = mycc5pay.processorder();
            string Procreturncode = mycc5pay.procreturncode;
            string responseStr = "";
            responseStr += mycc5pay.errmsg;
            ErrorMessage = mycc5pay.errmsg;
            responseStr += mycc5pay.oid;
            responseStr += mycc5pay.groupid;
            responseStr += mycc5pay.appr;
            responseStr += mycc5pay.refno;
            responseStr += mycc5pay.transid;
            responseStr += mycc5pay.Extra("HOSTMSG");

            return responseStr + " :: " + ErrorMessage;
        }

        public string PayWithFinans(decimal price, int orderId, int instCount, string cardNumber,
            string ccv2, int visaOrMaster, DateTime date)
        {
            var mycc5pay = new ePayment.cc5payment();

            mycc5pay.host = "https://www.fbwebpos.com/fim/api";
            mycc5pay.name = "sanalpos";
            mycc5pay.password = "Dimple943*";
            mycc5pay.clientid = "601224596";
            mycc5pay.orderresult = 0;

            string provizyonalOrderId = Guid.NewGuid().ToString();
            mycc5pay.oid = provizyonalOrderId;
            mycc5pay.cardnumber = cardNumber;
            mycc5pay.expmonth = date.Month.ToString();
            mycc5pay.expyear = date.Year.ToString();
            mycc5pay.cv2 = ccv2;
            mycc5pay.subtotal = price.ToString().Replace(",", ".");
            mycc5pay.currency = "949";
            mycc5pay.chargetype = "PreAuth";

            if (instCount.ToString() != "0" || instCount.ToString() != "1")
                mycc5pay.taksit = "";
            else mycc5pay.taksit = instCount.ToString();

            //işlem yapılıyor
            string resultprocess = mycc5pay.processorder();
            string Result1 = mycc5pay.procreturncode;
            string ErrMsg = mycc5pay.errmsg;
            ErrorMessage = mycc5pay.errmsg;
            string Oid1 = mycc5pay.oid;
            string GroupId = mycc5pay.groupid;
            string appr1 = mycc5pay.appr;
            string refno = mycc5pay.refno;
            string transid = mycc5pay.transid;
            string Extra = mycc5pay.Extra("HOSTMSG");

            string message = "";
            if (resultprocess == "1" && appr1 == "Approved")
            {
                message = "Approv";
                message += "ISLEM BASARILI BIR SEKILDE GONDERILDI VE ONAYLANDI !!!";
            }
            else if (resultprocess == "1" && appr1 == "Declined")
                message = "ISLEM BASARILI BIR SEKILDE GONDERILDI FAKAT ONAY ALAMADI !!!";
            else if (resultprocess == "1" && appr1 == "Error")
                message = "ISLEM BASARILI BIR SEKILDE GONDERILDI FAKAT ONAY ALAMADI !!!";
            else if (resultprocess == "1" && Result1 != "00")
                message = "ISLEM BASARILI BIR SEKILDE GONDERILDI FAKAT ONAY ALAMADI !!!";
            else if (resultprocess == "0")
                message = "ISLEM GONDERILEMEDI,BANKAYA BAGLANTI KURULAMADI !!!";

            ErrMsg = message;

            return message;
        }

        public string GetIp()
        {
            var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            return ip;
        }

        #endregion

        public class PosForm
        {
            public string CardOwner { get; set; }
            public long CardNumber { get; set; }
            public int SecureCode { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }
            public double Price { get; set; }
            public int Installments { get; set; }
        }

        public class Helper
        {
            public static string GetIp()
            {
                return string.IsNullOrEmpty(HttpContext.Current.Request.ServerVariables["remote_addr"]) ? "127.0.0.1" : HttpContext.Current.Request.ServerVariables["remote_addr"];
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
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "PrintVisible", "PrintVisible('');", true);
        }

        //protected void grdInstallments_RowDataBound(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
        //{
        //    if (e.Row.RowType == DataControlRowType.DataRow)
        //    {
        //        e.Row.Cells[3].Text = e.Row.Cells[3].Text.Substring(0, 6) + "****" + e.Row.Cells[3].Text.Substring(12, 4);
        //        e.Row.Attributes["onclick"] = "javascript:PrintVisible(this);";
        //    }
        //}

        //protected void grdInstallments_RowCreated(object sender, GridViewRowEventArgs e)
        //{
        //    if (e.Row.RowType == DataControlRowType.DataRow)
        //    {
        //        e.Row.Attributes.Add("onmouseover", "this.style.backgroundColor='silver';this.style.cursor='pointer';");
        //        e.Row.Attributes.Add("onmouseout", "this.style.backgroundColor='#FFF7E7';");
        //    }
        //}

    }
}
}
