using System;
using System.Collections.Generic;
using System.Text;

namespace Ireckonu.BusinessLogic.Models.Issues
{
    public class Warning : Issue
    {
        public override Severity Severity => Severity.Warning;

        public Warning(string text) : base(text)
        {
        }
    }
}
