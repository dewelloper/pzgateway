using Microsoft.AspNetCore.Http;
using PZPaymentGatWay.BObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PZPaymentGatWay.Payers.Interfaces
{
    public interface IPay
    {
        object Pay(PosForm pf, ref string provNumber, ref string provMessage, IHttpContextAccessor accessor = null);
    }
}
