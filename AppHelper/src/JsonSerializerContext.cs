/*  JsonSerializerContext.cs
 *  Version: v1.0 (2023.08.25)
 *  
 *  Contributor
 *      Arime-chan
 */

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace AppHelper
{
    [JsonSerializable(typeof(Appsettings_))]
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
