/*  Helpers/JsonSerializerContext.cs
 *  Version: v1.1 (2023.10.06)
 *  
 *  Author
 *      Arime-chan
 */

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace AppHelper
{
    [JsonSerializable(typeof(Appsettings_))]
    internal partial class P24JsonSerializerContext : JsonSerializerContext
    {
        internal static readonly JavaScriptEncoder FullUnicodeRangeJsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        internal static readonly JsonSerializerOptions JsonSerializerOptionsIndented = new()
        {
            Encoder = FullUnicodeRangeJsonEncoder,
            WriteIndented = true
        };
        internal static readonly JsonSerializerOptions JsonSerializerOptionsNonIndented = new()
        {
            Encoder = FullUnicodeRangeJsonEncoder,
            WriteIndented = false
        };
    }

}
