namespace Ireckonu.BusinessLogic.Models
{
    public class UploadServiceSettings
    {
        /// <summary>
        /// How many records to keep in memory before dumping them into db
        /// </summary>
        public int BufferSize { get; set; }
    }
}
