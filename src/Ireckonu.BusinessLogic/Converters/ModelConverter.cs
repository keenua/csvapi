using Ireckonu.BusinessLogic.Models;
using Ireckonu.Data;

namespace Ireckonu.BusinessLogic.Converters
{
    public class ModelConverter : IModelConverter
    {
        public Article ToModel(CsvRecord record)
        {
            var article = new Article
            {
                Key = record.Key,
                ArticleCode = record.ArtikelCode,
                ColorCode = record.ColorCode,
                Description = record.Description,
                Price = record.Price,
                DiscountPrice = record.DiscountPrice,
                DeliveredIn = record.DeliveredIn,
                Q1 = record.Q1,
                Size = record.Size,
                Color = record.Color
            };

            return article;
        }
    }
}
