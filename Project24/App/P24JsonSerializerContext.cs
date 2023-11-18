/*  App/P24JsonSerializerContext.cs
 *  Version: v1.5 (2023.11.19)
 *  
 *  Author
 *      Arime-chan
 */

using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Project24.App;
using Project24.App.BackendData;
using Project24.Model.Simulator.FinancialManagement;

namespace Project24.Serializer
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

    internal static class JsonSerializerOptionsFactory
    {
        internal enum Options : int
        {
            None = 0,

            UseFullRangeUnicode = 1 << 1,
            WriteIndented = 1 << 2,
            IgnoreNullEntries = 1 << 3,
        }


        public static readonly JavaScriptEncoder FullUnicodeRangeJsonEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);


        public static JsonSerializerOptions Get(Options _options)
        {
            if (!m_RequestedOptions.ContainsKey(_options))
                m_RequestedOptions[_options] = ConstructRequestedOptions(_options);

            return m_RequestedOptions[_options];
        }

        private static JsonSerializerOptions ConstructRequestedOptions(Options _options)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();

            for (int i = 0; i < 32; ++i)
            {
                if (MiscUtils.IsFlagSet(_options, Options.UseFullRangeUnicode))
                    options.Encoder = FullUnicodeRangeJsonEncoder;

                if (MiscUtils.IsFlagSet(_options, Options.WriteIndented))
                    options.WriteIndented = true;

                if (MiscUtils.IsFlagSet(_options, Options.IgnoreNullEntries))
                    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            }

            return options;
        }


        private static Dictionary<Options, JsonSerializerOptions> m_RequestedOptions = new()
        {
            { 0, new JsonSerializerOptions { } }
        };
    }
#endif

}
