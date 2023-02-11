using System.Text;

const string DefaultBaseAddress = "http://192.168.86.198:5555/api/";

var baseAddress = args.Length >= 1 && !string.IsNullOrEmpty(args[0]) ? args[0] : DefaultBaseAddress;
var batched = args.Length >= 2 && bool.Parse(args[1]);
var complete = args.Length >= 3 && bool.Parse(args[2]);
var parts = batched && args.Length >= 4 && bool.Parse(args[3]);
var innerResponse = batched && args.Length >=5 && bool.Parse(args[4]);

var completion = complete ? HttpCompletionOption.ResponseContentRead : HttpCompletionOption.ResponseHeadersRead;
var handler = new HttpClientHandler { AllowAutoRedirect = false, };
var client = new HttpClient(handler);
var requests = new SortedDictionary<string, HttpRequestMessage>
{
    { "accepted", new HttpRequestMessage(HttpMethod.Get, $"{baseAddress}Values/accepted") },
    { "bad", new HttpRequestMessage(HttpMethod.Get, $"{baseAddress}Values/bad") },
    { "error", new HttpRequestMessage(HttpMethod.Get, $"{baseAddress}Values/error") },
    { "get", new HttpRequestMessage(HttpMethod.Get, $"{baseAddress}Values") },
    { "redirect", new HttpRequestMessage(HttpMethod.Get, $"{baseAddress}Values/redirect") },
};

if (batched)
{
    var content = new MultipartContent("mixed", "batch_" + Guid.NewGuid().ToString());
    foreach (var request in requests.Values)
    {
        content.Add(new HttpMessageContent(request));
    }

    var batchedRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseAddress}batch") { Content = content, };
    var response = await client.SendAsync(batchedRequest, completion);

    // Can successfully display the response or its parts but can't do both together in some cases. Either
    // ReadAsMultipartAsync throws an IOException wrapping a WebException about an aborted request or
    // DisplayResponse (well, StreamContent.ReadAsStringAsync()) shows response.Content as empty, depending on
    // which executes second. In .NET Framework at least, can do both together when complete==true.
    if (parts)
    {
        var responseContents = await response.Content.ReadAsMultipartAsync();
        var i = 0;
        foreach (var innerContent in responseContents.Contents)
        {
            // For newer .NET Core releases, cannot get the Stream twice (second time to read the
            // HttpResponseMessage). This fails even if the response itself was complete initially.
            if (innerResponse)
            {
                await DisplayResponse($"batch {i++}", await innerContent.ReadAsHttpResponseMessageAsync());
            }
            else
            {
                await DisplayContent($"direct batch {i}", innerContent);
            }
        }
    }
    else
    {
        await DisplayResponse("batch", response);
    }
}
else
{
    foreach (var keyValuePair in requests)
    {
        var response = await client.SendAsync(keyValuePair.Value, completion);
        await DisplayResponse(keyValuePair.Key, response);
    }
}

static Task DisplayResponse(string name, HttpResponseMessage response)
{
    Console.WriteLine($"{name}: {response.StatusCode}");
    return DisplayContent(name, response.Content);
}

static async Task DisplayContent(string name, HttpContent content)
{
    if (content is null)
    {
        Console.WriteLine($"{name} content: null");
    }
    else
    {
        Console.WriteLine($"{name} content.GetType().Name: {content.GetType().Name}");
        //Console.WriteLine($"content.Headers: {content.Headers}");
        Console.WriteLine($"content.Headers.ContentLength: {content.Headers.ContentLength}");

        var stream = await content.ReadAsStreamAsync();
        Console.WriteLine($"stream.GetType().Name: {stream.GetType().Name}");
        Console.WriteLine($"CanRead: {stream.CanRead}, CanSeek: {stream.CanSeek}, CanWrite: {stream.CanWrite}, " +
            $"Length: {(stream.CanSeek ? stream.Length.ToString() : "unknowable")}");

        try
        {
            // May fail in later .NET core releases because a read only stream has already been consumed
            // under the covers.
            Console.WriteLine($"'{await content.ReadAsStringAsync()}'");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"ReadAsStringAsync: ${exception}");
            using var reader = new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024,
                leaveOpen: true);
            Console.WriteLine($"'{await reader.ReadToEndAsync()}'");
        }
    }

    Console.WriteLine("---");
}
