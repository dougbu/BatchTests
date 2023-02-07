const string BaseAddress = "http://192.168.86.21:5555/api/";

var handler = new HttpClientHandler { AllowAutoRedirect = false, };
var client = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress), };

var response = await client.GetAsync("Values/accepted");
await DisplayResponse("accepted", response);

//response = await client.GetAsync("Values/bad");
//await DisplayResponse("bad", response);

//response = await client.GetAsync("Values/error");
//await DisplayResponse("error", response);

//response = await client.GetAsync("Values/redirect");
//await DisplayResponse("redirect", response);

var content = new MultipartContent("mixed", "batch_" + Guid.NewGuid().ToString());
content.Add(new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, $"{BaseAddress}Values/accepted")));
content.Add(new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, $"{BaseAddress}Values/bad")));
content.Add(new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, $"{BaseAddress}Values/error")));
content.Add(new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, $"{BaseAddress}Values/redirect")));

var request = new HttpRequestMessage(HttpMethod.Post, "batch") { Content = content, };
response = await client.SendAsync(request);

var responseContents = await response.Content.ReadAsMultipartAsync();
var i = 0;
foreach(var innerContent in responseContents.Contents)
{
    Console.WriteLine("---");
    await DisplayContent($"direct batch {i}", innerContent);
    Console.WriteLine("---");
    await DisplayResponse($"batch {i++}", await innerContent.ReadAsHttpResponseMessageAsync());
}

await DisplayResponse("batch", response);

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

        var stream = await content.ReadAsStreamAsync();
        Console.WriteLine($"stream.GetType().Name: {stream.GetType().Name}");
        Console.WriteLine($"CanRead: {stream.CanRead}, CanSeek: {stream.CanSeek}, CanWrite: {stream.CanWrite}");

        try
        {
            var length = stream.Length;
            Console.WriteLine($"Length: {length}");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Length: {exception.GetType().Name}, {exception.Message}");
        }

        try
        {
            Console.WriteLine($"'{await content.ReadAsStringAsync()}'");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"ReadAsStringAsync: {exception}");
        }
    }
}
