namespace Ireckonu.BusinessLogic.Models
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
