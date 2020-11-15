using System;
using System.Collections.Generic;
using System.Text;

namespace Ireckonu.BusinessLogic.Models
{
    public abstract class Error : Issue
    {
        public override Severity Severity => Severity.Error;
    }
}
