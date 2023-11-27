using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Util.Store;

namespace XmlSearch;
public class GDriveManager
{
    private const string AppDataFolderName = "appDataFolder";
    private const string DefaultDataStoreFolderName = ".authdata";
    private const string ClientSecretsFileName = "client_secrets_web.json";
    private static DriveService _driveService { get; set; }

    public GDriveManager() { }

    private GDriveManager(DriveService driveService)
    {
        if (driveService == null)
        {
            throw new ArgumentNullException(nameof(driveService));
        }

        _driveService = driveService;
    }

    public async static Task<GDriveManager> GetInstance()
    {
        if(_driveService == null)
        {
            await InitClientAsync();
        }
        
        return new GDriveManager(_driveService);
    }

    private static async Task InitClientAsync()
    {
        if (_driveService is not null)
        {
            return;
        }
        Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(ClientSecretsFileName);

        var clientSecrets = (await GoogleClientSecrets
            .FromStreamAsync(fileStream)).Secrets;
        var dataStore = new FileDataStore(DefaultDataStoreFolderName);
        var scopes = new[] { DriveService.Scope.DriveAppdata };

        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets
                    , scopes
                    , "user"
                    , CancellationToken.None
                    , dataStore, new CodeReceiver());
        _driveService = new DriveService(new DriveService.Initializer { HttpClientInitializer = credential });
    }

    public async Task<string> SaveToFileAsync(string fileName, string content)
    {
        if (_driveService is null)
        {
            throw new InvalidOperationException("Drive service has not been initialized");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Incorect file name");
        }
        var list = await ListFilesAsync();
        if (list.Contains(fileName))
        {
            throw new ArgumentException();
        }

        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = fileName,
            Parents = new List<string>() { AppDataFolderName }
        };

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var request = _driveService.Files.Create(fileMetadata, stream, "text/plain");
        request.Fields = "id";
        var uploadResult = await request.UploadAsync();
        if (uploadResult.Exception is not null)
        {
            return string.Empty;
        }

        return fileName;
    }


    public async Task<List<string>> ListFilesAsync()
    {
        if (_driveService is null)
        {
            throw new InvalidOperationException("Drive service has not been initialized");
        }

        var request = _driveService.Files.List();
        request.Spaces = AppDataFolderName;
        request.Fields = "files(id, name) ";

        var result = await request.ExecuteAsync();

        List<string> list = new List<string>(result.Files.Count);
        foreach (var file in result.Files)
        {
            list.Add(file.Name);
        }
        return list;
    }

    public async Task<FileList> FileListAsync()
    {
        if (_driveService is null)
        {
            throw new InvalidOperationException("Drive service has not been initialized");
        }

        var request = _driveService.Files.List();
        request.Spaces = AppDataFolderName;
        request.Fields = "files(id, name) ";

        var result = await request.ExecuteAsync();
        
        return result;
    }

    public async Task<string> ReadFileContentAsync(string fileName)
    {
        var id = await GetFileIdByFileName(fileName);
        if (id == null) return null;
        var request = _driveService.Files.Get(id);
        using var stream = new MemoryStream();
        var downloadResult = await request.DownloadAsync(stream);
        if (downloadResult.Exception != null)
        {
            return null;
        }
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        string content = await reader.ReadToEndAsync();
        return content;        
    }

    public async Task<string> GetFileIdByFileName(string fileName)
    {
        var list = await FileListAsync();
        return list.Files.FirstOrDefault(s => s.Name.Equals(fileName), null).Id;
    }
}