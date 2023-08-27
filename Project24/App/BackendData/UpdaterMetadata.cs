/*  App/BackendData/UpdaterMetadata.cs
 *  Version: v1.0 (2023.06.29)
 *  
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Project24.App.BackendData
{
    public class VersionInfo
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }
        public int Revision { get; set; }

        public VersionInfo()
        { }

        public VersionInfo(Version _version)
        {
            Major = _version.Major;
            Minor = _version.Minor;
            Build = _version.Build;
            Revision = _version.Revision;
        }


        public override string ToString() => Major + "." + Minor + "." + Build + "." + Revision;
    }

    public sealed class UpdaterMetadata
    {
        public int FilesCount { get; set; }
        public long FilesSize { get; set; }

        public int BatchesCount { get; set; }
        public List<UpdaterBatchMetadata> BatchesMetadata { get; set; }

        [JsonIgnore]
        public Dictionary<long, long> LastMods { get; set; }


        public UpdaterMetadata()
        {
            LastMods = new();
        }
    }

    public sealed class UpdaterBatchMetadata
    {
        public int Id { get; set; } = 0;

        [JsonIgnore]
        public bool IsSuccess { get; set; } = false;

        public int FilesCount { get; set; } = 0;
        public long FilesSize { get; set; } = 0L;

        public Dictionary<long, UpdaterFileMetadata> FilesMetadata { get; set; } = new();


        public UpdaterBatchMetadata()
        { }
    }

    public sealed class UpdaterFileMetadata
    {
        public long HashCode { get; set; }

        public long LastMod { get; set; }

        public UpdaterFileMetadata()
        { }
    }

}
