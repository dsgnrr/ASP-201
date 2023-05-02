namespace ASP_201.Data.Entity
{
    public class Rate
    {
        public Guid ItemId  { get; set; }  //  Composite
        public Guid UserId  { get; set; }  //  primary key
        public int  Rating  { get; set; }
    }
}

