using CommunityToolkit.Maui.Storage;
using System.Collections.ObjectModel;
using System.Xml;
using XmlSearch.Model;

namespace XmlSearch;

public partial class MainPage : ContentPage
{
    IFileSaver fileSaver;
    static GDriveManager driveManager;

    public ObservableCollection<string> attr { get; set; } = new();
    public ObservableCollection<string> list { get; set; } = new();
    public ObservableCollection<Sale> sales { get; set; } = new();

    public MainPage(IFileSaver fileSaver)
    {
        InitializeComponent();
        
        BindingContext = this;
        SourceGDriveRButton.CheckedChanged += GDriveSource_CheckedChangedAsync;

        this.fileSaver = fileSaver;
    }

    private void OpenButton_Click(object sender, EventArgs e)
    {
        
        var _document = new XmlDocument();
        _document.Load("/Users/rsnhn/Projects/XmlSearch/XmlSearch/sales.xml");
        var _saleNodes = new List<XmlNode>();

        var _root = _document.SelectSingleNode("descendant::Sales");
        _saleNodes = _root.ChildNodes.Cast<XmlNode>().ToList();
        sales = new ObservableCollection<Sale>(_saleNodes.Select(CreateSaleFromNode).ToList());
        dataGrid.ItemsSource = sales;


    }

    private Sale CreateSaleFromNode(XmlNode saleNode)
    {
        var sale = new Sale();

        var invoiceId = saleNode.SelectSingleNode("descendant::InvoiceId")?.InnerText;
        var branch = saleNode.SelectSingleNode("descendant::Branch")?.InnerText;
        var city = saleNode.SelectSingleNode("descendant::City")?.InnerText;
        var customerType = saleNode.SelectSingleNode("descendant::CustomerType")?.InnerText;
        var gender = saleNode.SelectSingleNode("descendant::Gender")?.InnerText;
        var productLine = saleNode.SelectSingleNode("descendant::ProductLine")?.InnerText;
        var unitPrice = saleNode.SelectSingleNode("descendant::UnitPrice")?.InnerText;
        var quantity = saleNode.SelectSingleNode("descendant::Quantity")?.InnerText;
        var tax = saleNode.SelectSingleNode("descendant::Tax")?.InnerText;
        var total = saleNode.SelectSingleNode("descendant::Total")?.InnerText;
        var date = saleNode.SelectSingleNode("descendant::Date")?.InnerText;
        var time = saleNode.SelectSingleNode("descendant::Time")?.InnerText;
        var payment = saleNode.SelectSingleNode("descendant::Payment")?.InnerText;
        var costOfGoods = saleNode.SelectSingleNode("descendant::CostOfGoods")?.InnerText;
        var rating = saleNode.SelectSingleNode("descendant::Rating")?.InnerText;

        return SaleFactory.Instance.CreateSale(invoiceId, branch, city, customerType, gender, productLine, unitPrice,
            quantity, tax, total, date, time, payment, costOfGoods, rating);
    }


    private void SaveButton_Click(object sender, EventArgs e)
    {

    }

    private async void ClearButton_Click(object sender, EventArgs e)
    {
        

        //var fileList = await driveManager.FileListAsync();
        //var service = driveManager.getDriveService();
        //foreach (var child in fileList.Files)
        //{
        //    await service.Files.Delete(child.Id).ExecuteAsync();
        //}

        var list = await driveManager.ListFilesAsync();
        //textInput.Text = list.Count != 0 ? String.Join(", ", list.ToArray()) : "empty";
    }

    private async void GDriveSource_CheckedChangedAsync(object sender, CheckedChangedEventArgs e)
    {
        RadioButton radio = (RadioButton)sender;
        driveManager ??= await GDriveManager.Create();
        if(radio.IsChecked)
        {
            var glist = await driveManager.ListFilesAsync();
            foreach (var fileName in glist)
            {
                list.Add(fileName);
            }
            //list = (ObservableCollection<string>) list.Distinct();
        }
        filePicker.IsVisible = radio.IsChecked;
    }
}


