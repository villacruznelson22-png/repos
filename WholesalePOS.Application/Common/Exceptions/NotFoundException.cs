using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalePOS.Application.Common.Exceptions
{
    public class NotFoundException : BusinessException
    {
        public NotFoundException(string message)
            : base(message)
        {
        }
    }
}
