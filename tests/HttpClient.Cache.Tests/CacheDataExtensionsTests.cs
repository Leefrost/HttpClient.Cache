using System.Net;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;

namespace HttpClient.Cache.Tests;

public class CacheDataExtensionsTests
{
    [Fact]
    public void Pack_CreateABytesFromCacheData_ReturnByteArray()
    {
        var message = Encoding.UTF8.GetBytes("Here is a message");
        var response =
            new HttpResponseMessage { Content = new ByteArrayContent(message), StatusCode = HttpStatusCode.OK };
        var data = new CacheData(message, response);

        var packData = data.Pack();

        using (new AssertionScope())
        {
            var chars = new char[packData.Length / sizeof(char)];
            Buffer.BlockCopy(packData, 0, chars, 0, packData.Length);
            var json = new string(chars);
            
            var unpackedData = JsonConvert.DeserializeObject<CacheData>(json);
            
            packData.Should().NotBeNullOrEmpty();
            unpackedData.Should().BeEquivalentTo(data);
        }
    }
    
    [Fact]
    public void UnPack_CreateCacheDataEn_ReturnByteArray()
    {
        var message = Encoding.UTF8.GetBytes("Here is a message");
        var response =
            new HttpResponseMessage { Content = new ByteArrayContent(message), StatusCode = HttpStatusCode.OK };
        var cacheData = new CacheData(message, response);
        var serializeObject =  JsonConvert.SerializeObject(cacheData);
        var bytes = new byte[serializeObject.Length * sizeof(char)];
        Buffer.BlockCopy(serializeObject.ToCharArray(), 0, bytes, 0, bytes.Length);

        var unpackData = bytes.Unpack();

        unpackData.Should().NotBeNull().And.BeEquivalentTo(cacheData);
    }
}