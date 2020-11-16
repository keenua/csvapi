namespace Ireckonu.BusinessLogic.Models.Issues
{
    public class Error : Issue
    {
        public override Severity Severity => Severity.Error;

        public Error(string text) : base(text)
        {
        }
    }
}
