namespace XmlSearch.Model;

public class Sale
{
    public string InvoiceId { get; set; }
    public Branch MarketBranch { get; set; }
    public string City { get; set; }
    public CustomerType CustomerType { get; set; }
    public Gender Gender { get; set; }
    public string ProductLine { get; set; }
    public double ProductUnitPrice { get; set; }
    public int ProductQuantity { get; set; }
    public double ProductCostWithoutTax { get; set; }
    public double ProductTax { get; set; }
    public double ProductTotal { get; set; }
    public DateTime Date { get; set; }
    public Payment Payment { get; set; }
    public double Rating { get; set; }
}
