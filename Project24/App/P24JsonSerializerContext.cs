/*  App/P24JsonSerializerContext.cs
 *  Version: v1.2 (2023.09.21)
 *  
 *  Author
 *      Arime-chan
 */

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Project24.App.BackendData;

namespace Project24.SerializerContext
{
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(string[]))]
    //[JsonSerializable(typeof(UpdaterPageDataModel))]
    [JsonSerializable(typeof(VersionInfo))]
    public partial class P24JsonSerializerContext : JsonSerializerContext
    {
        public static readonly JavaScriptEncoder FullUnicodeRangeJsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        public static readonly JsonSerializerOptions JsonSerializerOptionsIndented = new()
        {
            Encoder = FullUnicodeRangeJsonEncoder,
            WriteIndented = true
        };
        public static readonly JsonSerializerOptions JsonSerializerOptionsNonIndented = new()
        {
            Encoder = FullUnicodeRangeJsonEncoder,
            WriteIndented = false
        };
    }
}
