namespace Ireckonu.BusinessLogic.Models
{
    public abstract class Issue
    {
        public string Text { get; set; }
        public abstract Severity Severity { get; } 
    }
}
