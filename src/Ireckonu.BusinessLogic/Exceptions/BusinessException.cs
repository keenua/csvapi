using System;

namespace Ireckonu.BusinessLogic.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException() 
        {
        }

        public BusinessException(string message): base(message) 
        { 
        }
    }
}
