/*  App/P24JsonSerializerContext.cs
 *  Version: v1.0 (2023.06.20)
 *  
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Project24.App.BackendData;
using Project24.Model.Home.Maintenance;

namespace Project24.SerializerContext
{
    [JsonSerializable(typeof(UpdaterPageDataModel))]
    //[JsonSerializable(typeof(Dictionary<long, long>))]
    [JsonSerializable(typeof(VersionInfo))]
    public partial class P24JsonSerializerContext : JsonSerializerContext
    {
    }
}
