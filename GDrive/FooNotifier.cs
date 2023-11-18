using System;
namespace XmlSearch;
public class FooNotifier
{
    private static readonly TaskCompletionSource<Uri> _tcs = new TaskCompletionSource<Uri>();

    public static void Notify(Uri uri)
    {
        _tcs.SetResult(uri);
    }

    public static async Task<Uri> WaitForResponse()
    {
        return await _tcs.Task;
    }
}

