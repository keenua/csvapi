namespace Ireckonu.Data.Mongo
{
    internal class ArticleDocument : Article
    {
        public string _id
        {
            get { return Key; }
            set { Key = value; }
        }

        public ArticleDocument(Article record)
        {
            Key = record.Key;
            ArticleCode = record.ArticleCode;
            ColorCode = record.ColorCode;
            Description = record.Description;
            Price = record.Price;
            DiscountPrice = record.DiscountPrice;
            DeliveredIn = record.DeliveredIn;
            Q1 = record.Q1;
            Size = record.Size;
            Color = record.Color;
        }
    }
}
