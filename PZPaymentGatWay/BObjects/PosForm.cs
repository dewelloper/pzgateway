using System;
using System.Collections.Generic;
using System.Text;

namespace PZPaymentGatWay.BObjects
{
    public class PosForm
    {
        public string CardOwner { get; set; }
        public long CardNumber { get; set; }
        public int SecureCode { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public double Price { get; set; }
        public int Installments { get; set; }
        public string Explanation { get; set; }
        public string CardType { get; set; }
    }
}
