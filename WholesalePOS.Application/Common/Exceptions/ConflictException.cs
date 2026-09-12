using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalePOS.Application.Common.Exceptions
{
    public class ConflictException : BusinessException
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}
