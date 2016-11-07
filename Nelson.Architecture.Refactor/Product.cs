namespace Nelson.Architecture.Refactor
{
    public class Product
    {
        public decimal Price { get; set; }
        public string ProductType { get; set; }
        public DiscountTypes DiscountType { get; set; }
    }
}