using CommunityToolkit.Maui.Storage;
using Mopups.Interfaces;

namespace XmlSearch;

public partial class MainPage : ContentPage
{
    IFileSaver fileSaver;
    IPopupNavigation popupNavigation;
    static GDriveManager driveManager;

    public MainPage(IFileSaver fileSaver, IPopupNavigation popupNavigation)
    {
        InitializeComponent();
        this.fileSaver = fileSaver;
        this.popupNavigation = popupNavigation;
    }

    private void OpenXmlButton_Click(object sender, EventArgs e)
    {

    }

    private void OpenXslButton_Click(object sender, EventArgs e)
    {

    }

    private void TransformToHTML_Click(object sender, EventArgs e)
    {

    }

    private async void Clear_Click(object sender, EventArgs e)
    {
        if (driveManager == null)
        {
            driveManager = await GDriveManager.Create();
        }

        //var fileList = await driveManager.FileListAsync();
        //var service = driveManager.getDriveService();
        //foreach (var child in fileList.Files)
        //{
        //    await service.Files.Delete(child.Id).ExecuteAsync();
        //}

        var list = await driveManager.ListFilesAsync();
        //textInput.Text = list.Count != 0 ? String.Join(", ", list.ToArray()) : "empty";
    }


}


