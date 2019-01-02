using Microsoft.AspNetCore.Http;
using PZPaymentGatWay.BObjects;
using PZPaymentGatWay.Payers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PZPaymentGatWay.Payers.Concretes
{
    public class MaximumPay : CompanyBase, IPay
    {
        public object Pay(PosForm pf, ref string provNumber, ref string provMessage, IHttpContextAccessor accessor = null)
        {
            var mycc5pay = new ePayment.cc5payment();

            mycc5pay.host = "https://sanalpos.isbank.com.tr/servlet/cc5ApiServer";
            //mycc5pay.host="https://testsanalpos.est.com.tr/servlet/cc5ApiServer";
            mycc5pay.name = "sanalpos";
            mycc5pay.password = "Dimple943*";
            mycc5pay.clientid = "700659404610";
            mycc5pay.orderresult = 0;

            string provizyonalOrderId = Guid.NewGuid().ToString();
            mycc5pay.oid = provizyonalOrderId;
            mycc5pay.cardnumber = pf.CardNumber.ToString();
            mycc5pay.expmonth = pf.Month.ToString();
            mycc5pay.expyear = pf.Year.ToString();
            mycc5pay.cv2 = String.Format("{0:000}", Convert.ToInt32(pf.SecureCode));
            mycc5pay.subtotal = Convert.ToDecimal(pf.Price).ToString().Replace(",", ".");
            mycc5pay.currency = "949";
            mycc5pay.chargetype = "Auth";

            mycc5pay.taksit = pf.Installments.ToString() == "0" ? "1" : pf.Installments.ToString();


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
    }
}
