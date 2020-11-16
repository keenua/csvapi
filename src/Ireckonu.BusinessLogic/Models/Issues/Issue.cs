namespace Ireckonu.BusinessLogic.Models.Issues
{
    public abstract class Issue
    {
        public string Text { get; }
        public abstract Severity Severity { get; } 

        protected Issue(string text)
        {
            Text = text;
        }
    }
}
