namespace ETLPROYECTOELECT1.Models
{
    public class Orders
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }


        public string Status { get; set; } = string.Empty;

        public Customer Customer { get; set; } = null!;

        public List<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();
    }
}