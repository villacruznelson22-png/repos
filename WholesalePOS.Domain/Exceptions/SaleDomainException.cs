using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalePOS.Domain.Exceptions
{
    public class SaleDomainException : DomainException
    {
        public SaleDomainException(string message) : base(message)
        {
        }
    }
}
