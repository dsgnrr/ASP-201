namespace ASP_201.Models.Home
{
    public class PassDataModel
    {
        public String Header { get; set; } = null;
        public String Title { get; set; } = null;
        public List<Product> Products { get; set; } = null;
    }

    public class Product
    {
        public String Name { get; set; } = null;
        public Double Price { get; set; }
        public String Image { get; set; } = null;
    }

}
