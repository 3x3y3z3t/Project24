/*  App/P24JsonSerializerContext.cs
 *  Version: v1.4 (2023.10.07)
 *  
 *  Author
 *      Arime-chan
 */

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Project24.App.BackendData;
using Project24.Model.Simulator.FinancialManagement;

namespace Project24.SerializerContext
{
#if SCAFFOLDING
    internal class ScaffoldingHighlight { }
#else
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(string[]))]
    //[JsonSerializable(typeof(UpdaterPageDataModel))]
    [JsonSerializable(typeof(VersionInfo))]
    [JsonSerializable(typeof(ImportExportDataModel))]
    internal partial class P24JsonSerializerContext : JsonSerializerContext
    {
        public static readonly JavaScriptEncoder FullUnicodeRangeJsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);

        public static readonly JsonSerializerOptions JsonSerializerOptionsFullUnicodeRange = new()
        {
            Encoder = FullUnicodeRangeJsonEncoder
        };
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
#endif
}
