using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalePOS.Application.Common.Exceptions
{
    public abstract class BusinessException : Exception
    {
        protected BusinessException(string message)
            : base(message)
        {
        }
    }
}
