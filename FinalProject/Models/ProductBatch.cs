namespace FinalProject.Models
{
    public class ProductBatch
    {
        public int Id { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


        public DateTime? ExpirationDate { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public int Stock { get; set; }

        //Relational
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
