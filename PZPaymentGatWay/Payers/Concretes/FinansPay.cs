using Microsoft.AspNetCore.Http;
using PZPaymentGatWay.BObjects;
using PZPaymentGatWay.Payers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PZPaymentGatWay.Payers.Concretes
{
    public class FinansPay : CompanyBase, IPay
    {
        public object Pay(PosForm pf, ref string provNumber, ref string provMessage, IHttpContextAccessor accessor = null)
        {
            var mycc5pay = new ePayment.cc5payment();

            mycc5pay.host = "https://www.fbwebpos.com/fim/api";
            mycc5pay.name = "sanalpos";
            mycc5pay.password = "Dimple943*";
            mycc5pay.clientid = "601224596";
            mycc5pay.orderresult = 0;

            string provizyonalOrderId = Guid.NewGuid().ToString();
            mycc5pay.oid = provizyonalOrderId;
            mycc5pay.cardnumber = pf.CardNumber.ToString();
            mycc5pay.expmonth = pf.Month.ToString();
            mycc5pay.expyear = pf.Year.ToString();
            mycc5pay.cv2 = String.Format("{0:000}", Convert.ToInt32(pf.SecureCode));
            mycc5pay.subtotal = Convert.ToDecimal(pf.Price).ToString().Replace(",", ".");
            mycc5pay.currency = "949";
            mycc5pay.chargetype = "PreAuth";

            if (pf.Installments.ToString() != "0" || pf.Installments.ToString() != "1")
                mycc5pay.taksit = "";
            else mycc5pay.taksit = pf.Installments.ToString();

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
    }
}
