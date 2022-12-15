/*  NasBrowserViewModel.cs
 *  Version: 1.0 (2022.12.14)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using Project24.App;

namespace Project24.Models.Nas
{
    public class NasBrowserViewModel
    {
        public string Path { get; set; } = "";
        public List<string> PathLayers { get; set; } = new List<string>();
        public List<NasUtils.FileModel> Files { get; set; } = new List<NasUtils.FileModel>();
        public bool IsUploadMode { get; set; } = false;

        public NasBrowserViewModel()
        { }
    }

}
