using System;

namespace Ireckonu.BusinessLogic.Exceptions
{
    abstract class BusinessException : Exception
    {
        protected BusinessException() 
        {
        }

        protected BusinessException(string message): base(message) 
        { 
        }
    }
}
