namespace Ireckonu.BusinessLogic.Models
{
    public class ValueError : Issue
    {
        public override Severity Severity => Severity.Error;
    }
}
