namespace Ireckonu.BusinessLogic.Models
{
    public class Error : Issue
    {
        public override Severity Severity => Severity.Error;

        public Error(string text) : base(text)
        {
        }
    }
}
