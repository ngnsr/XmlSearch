using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Collections.ObjectModel;
using XmlSearch.XML;
using XmlSearch.Model;
using XmlSearch.Utils;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Text;

namespace XmlSearch;

public partial class MainPage : ContentPage
{
    IFileSaver fileSaver;
    static GDriveManager driveManager;

    public ObservableCollection<string> attr { get; set; } = new();
    public ObservableCollection<string> gFileList { get; set; } = new();
    public ObservableCollection<Sale> sales { get; set; } = new();

    public FilterManager filterManager;

    public IXmlAnalyser xmlAnalyser { get; set; }

    public string SelectedStrategy { get; set; }

    public enum XMLStrategies
    {
        DOM,
        SAX,
        LINQ
    }

    public MainPage(IFileSaver fileSaver)
    {
        BindingContext = this;
        InitializeComponent();

        Initialize();

        this.fileSaver = fileSaver;
    }

    private void Initialize()
    {
        GUploadFilePicker.IsVisible = false;
        SaveAs.IsVisible = false;

        DomStrategy.IsChecked = true;

        SourceLocalRButton.IsChecked = true;

        OpenXmlRButton.IsChecked = true;

        SaveAsXml.IsChecked = true;

        SourceGDriveRButton.CheckedChanged += GDriveSource_CheckedChangedAsync;
        OpenXmlRButton.CheckedChanged += OpenXmlRButton_CheckedChangedAsync;
        attr = new ObservableCollection<string>(Enum.GetNames<Filters>());
        attrPicker.ItemsSource = attr;
    }

    private async void OpenButton_Click(object sender, EventArgs e)
    {
        if (SourceLocalRButton.IsChecked)
        {
            await OpenLocalFile();
        }
        else if (SourceGDriveRButton.IsChecked)
        {
            await OpenGDriveFile();
        }
    }

    private async Task OpenLocalFile()
    {
        // call picker, get path, update FileManager, update gui DataGrid
        if (OpenXmlRButton.IsChecked)
        {
            FileResult result = await FilePicker.PickAsync(FilePickerOptions.GetXmlPickOptions());

            if (result != null)
            {
                UpdateXmlFile(new Utils.File(result.FullPath));
            }
        }
        else if (OpenXslRButton.IsChecked)
        {
            FileResult result = await FilePicker.PickAsync(FilePickerOptions.GetXslPickOptions());
            if (result != null)
            {
                UpdateXslFile(new Utils.File(result.FullPath));
            }
        }
    }

    private async Task OpenGDriveFile()
    {
        if (GUploadFilePicker.SelectedIndex == -1)
        {
            await Toast.Make("Оберіть файл, який потрібно відкрити!", ToastDuration.Long).Show();
            return;
        }

        var fileName = GUploadFilePicker.SelectedItem.ToString();
        var content = await driveManager.ReadFileContentAsync(fileName);
        if (content != null)
        {
            Utils.File file = new Utils.File(fileName, content);
            if (OpenXmlRButton.IsChecked) UpdateXmlFile(file);
            else UpdateXslFile(file);
        }
    }

    private void UpdateXmlFile(Utils.File file)
    {
        var instance = FileManager.GetInstance();
        if (instance.xml == file) return;

        instance.xml = file;
        XmlFileName.Text = file.FileName;
        ShowAllSales();
    }

    private void ShowAllSales()
    {
        try
        {
            ChangeAnalyserMethodIfNeeded();
        }
        catch { return; }

        var list = xmlAnalyser.GetAllSales();

        sales = new ObservableCollection<Sale>(list);
        dataGrid.ItemsSource = sales;
    }

    private void UpdateXslFile(Utils.File file)
    {
        FileManager.GetInstance().xsl = file;
        XslFileName.Text = file.FileName;
    }

    private async void SaveButton_Click(object sender, EventArgs e)
    {
        await SaveFile();
    }

    private async Task SaveFile()
    {
        string content = "";
        string fileName = "";

        if (SaveAsXml.IsChecked)
        {
            if (sales.Count == 0)
            {
                await ShowToast("Xml порожній!");
                return;
            }
            content = GetXmlWithHeader();
            fileName = XmlFileName.Text;
        }
        else if (SaveAsXls.IsChecked)
        {
            if (FileManager.GetInstance().xsl == null)
            {
                await ShowToast("Xsl файл не додано!");
                return;
            }
            content = FileManager.GetInstance().xsl.Content;
            fileName = XslFileName.Text;
        }
        else if (SaveAsHtml.IsChecked)
        {
            if (sales.Count == 0)
            {
                await ShowToast("Xml порожній!");
                return;
            }
            content = await GetHtml();
            fileName = "Sales.html";
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            await ShowToast("Файл порожній!");
            return;
        }

        if (SourceLocalRButton.IsChecked)
        {
            await SaveToLocalFile(content, fileName);
        }
        else if (SourceGDriveRButton.IsChecked)
        {
            await SaveToGDrive(content, fileName);
        }
    }

    private async Task SaveToLocalFile(string content, string fileName)
    {
        string homePath = GetHomePath();
        await fileSaver.SaveAsync(homePath, fileName, new MemoryStream(Encoding.UTF8.GetBytes(content)), new CancellationTokenSource().Token);
    }

    private async Task SaveToGDrive(string content, string fileName)
    {
        try
        {
            fileName = SaveAs.Text;
            await driveManager.SaveToFileAsync(fileName, content);
        }
        catch (ArgumentException)
        {
            await ShowToast("Такий файл присутній. Спробуйте ще раз!");
        }
    }

    private static async Task ShowToast(string message)
    {
        await Toast.Make(message, ToastDuration.Long).Show();
    }

    private static string GetHomePath()
    {
        return (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            ? Environment.GetEnvironmentVariable("HOME")
            : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
    }

    private void ClearButton_Click(object sender, EventArgs e)
    {
        attrPicker.SelectedIndex = 0;
        KeywordEntry.Text = "";
        ShowAllSales();
    }

    private async Task UpdateGFileList()
    {
        driveManager ??= await GDriveManager.GetInstance();
        gFileList.Clear();
        List<string> glist;
        List<string> list = await driveManager.ListFilesAsync();
        if (OpenXmlRButton.IsChecked)
        {
            glist = list.Where(s => s.EndsWith(".xml")).ToList();
        }
        else
        {
            glist = list.Where(s => s.EndsWith(".xsl")).ToList();
        }

        foreach (var fileName in glist)
        {
            gFileList.Add(fileName);
        }
    }

    private async void GDriveSource_CheckedChangedAsync(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            await UpdateGFileList();
        }

        GUploadFilePicker.IsVisible = e.Value;
        SaveAs.IsVisible = e.Value;
    }

    private async void OpenXslRBUtton_CheckedChangedAsync(object sender, CheckedChangedEventArgs e)
    {
        await UpdateGFileList();
    }

    private async void OpenXmlRButton_CheckedChangedAsync(object sender, CheckedChangedEventArgs e)
    {
        await UpdateGFileList();
    }

    private bool XmlFileIsNull()
    {
        return FileManager.GetInstance().xml == null;
    }

    async private void ChangeAnalyserMethodIfNeeded()
    {
        if (XmlFileIsNull())
        {
            throw new ArgumentException();
        }

        try
        {
            if (SelectedStrategy.Equals(XMLStrategies.DOM.ToString()))
            {
                xmlAnalyser = new DomXmlAnalyser();
            }
            else if (SelectedStrategy.Equals(XMLStrategies.SAX.ToString()))
            {
                xmlAnalyser = new SaxXmlAnalyser();
            }
            else if (SelectedStrategy.Equals(XMLStrategies.LINQ.ToString()))
            {
                xmlAnalyser = new LinqXmlAnalyser();
            }
        }
        catch
        {
            await Toast.Make("Файл некоректний!", ToastDuration.Long).Show();
            FileManager.GetInstance().xml = null;
            return;
        }

        filterManager ??= new FilterManager(xmlAnalyser);
        filterManager.UpdateAnalyser(xmlAnalyser);
    }

    async void SearchButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            ChangeAnalyserMethodIfNeeded();
        }
        catch { return; }

        if (!Enum.TryParse(attrPicker.SelectedItem.ToString(), out Filters CurrentFilter)) return;
        var list = await filterManager.HandleFilter(CurrentFilter, KeywordEntry.Text);
        if (list == null)
        {
            return;
        }

        sales = new ObservableCollection<Sale>(list);
        dataGrid.ItemsSource = sales;
    }

    private string GetXmlWithHeader()
    {
        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + GetXml();
    }

    private async Task<string> GetHtml()
    {
        var xml = GetXml();

        XslCompiledTransform transform = new XslCompiledTransform();
        if (FileManager.GetInstance().xsl == null)
        {
            await Toast.Make("Xsl файл не додано!", ToastDuration.Long).Show();
            return null;
        }
        using (StringReader sr = new StringReader(FileManager.GetInstance().xsl.Content))
        {
            using (XmlReader xr = XmlReader.Create(sr))
            {
                transform.Load(xr);
            }
        }

        StringWriter results = new StringWriter();
        XmlReader xmlStringReader = XmlReader.Create(new StringReader(xml));
        transform.Transform(xmlStringReader, null, results);
        return results.ToString();
    }

    private string GetXml()
    {
        var invoiceIds = sales.Select(s => s.InvoiceId);
        XElement root = XElement.Load(new MemoryStream(Encoding.UTF8.GetBytes(FileManager.GetInstance().xml.Content))); ;
        var saleElements = root.Elements().Where(x => invoiceIds.Contains(x.Element("InvoiceId").Value));
        var xml = $"<Sales>{string.Concat(saleElements)}</Sales>";
        return xml;
    }
}
