using System.Globalization;
using System.Text.RegularExpressions;

namespace XmlSearch.Model;

public class SaleFactory
{
	private static SaleFactory _instance = null;

	public static SaleFactory Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new SaleFactory();
			}

			return _instance;
		}
	}

	public Sale CreateSale(string invoiceIdStr, string branchStr, string cityStr, string customerTypeStr, string genderStr, string productLineStr, string unitPriceStr, string quantityStr, string taxStr, string totalStr, string dateStr, string timeStr, string paymentStr, string costOfGoodsStr, string ratingStr)
	{

		var sale = new Sale();
		bool isValid = true;

		isValid &= new Regex(@"^\d{3}-\d{2}-\d{4}$").IsMatch(invoiceIdStr);
		sale.InvoiceId = invoiceIdStr;

		isValid &= Enum.TryParse(branchStr, out Branch branch);
		sale.MarketBranch = branch;

		sale.City = cityStr;

		isValid &= Enum.TryParse(customerTypeStr, out CustomerType customerType);
		sale.CustomerType = customerType;

		isValid &= Enum.TryParse(genderStr, out Gender gender);
		sale.Gender = gender;

		sale.ProductLine = productLineStr;

		isValid &= double.TryParse(unitPriceStr, out double unitPrice);
		sale.ProductUnitPrice = unitPrice;

		isValid &= int.TryParse(quantityStr, out int quantity);
		sale.ProductQuantity = quantity;

		isValid &= double.TryParse(taxStr, out double tax);
		sale.ProductTax = tax;

		isValid &= double.TryParse(totalStr, out double total);
		sale.ProductTotal = total;

		isValid &= DateTime.TryParseExact(dateStr, "M/d/yyyy", null, DateTimeStyles.None, out var date);
		sale.Date = date;

		isValid &= TimeSpan.TryParseExact(timeStr, @"h\:mm", null, TimeSpanStyles.None, out var time);
		sale.Date += time;

		isValid &= Enum.TryParse(paymentStr, out Payment payment);
		sale.Payment = payment;

		isValid &= double.TryParse(costOfGoodsStr, out double costOfGoods);
		sale.ProductCostWithoutTax = costOfGoods;

		isValid &= double.TryParse(ratingStr, out double rating);
		sale.Rating = rating;

		return isValid ? sale : null;
	}
}
