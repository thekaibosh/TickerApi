using System.Xml.Serialization;

public class XmlResult<T> : IResult
{
    private static readonly XmlSerializer Serializer = new(typeof(T));

    private readonly T _result;

    public XmlResult(T result)
    {
        _result = result;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        using var ms = new MemoryStream();

        Serializer.Serialize(ms, _result);
        ms.Position = 0;

        httpContext.Response.ContentType = "application/xml";
        await ms.CopyToAsync(httpContext.Response.Body);
    }
}