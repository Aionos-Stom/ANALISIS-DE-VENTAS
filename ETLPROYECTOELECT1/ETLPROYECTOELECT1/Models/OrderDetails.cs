namespace ETLPROYECTOELECT1.Models
{
    public class OrderDetails
    {
        public int OrderID { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        public Orders Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}