/*  AppUtils.cs
 *  Version: 1.2 (2022.12.06)
 *
 *  Contributor
 *      Arime-chan
 */

namespace Project24.App.Utils
{
    public class UpdaterStats
    {
        public int TotalFilesToUpload { get; set; } = 0;
        public int TotalUploadedFiles { get; set; } = 0;
    }

    public static class UpdaterUtils
    {
        public struct UploadFileInfo
        {
            public string Name;
            public string Path;
            public string HashCode;
        }

        public static UploadFileInfo ComputeUploadFileInfo(string _baseFileName)
        {
            string filename;
            string path;
            string hashCode;

            int pos = _baseFileName.LastIndexOf('/');
            if (pos > 0)
            {
                path = _baseFileName[0..(pos + 1)];
                filename = _baseFileName[(pos + 1)..];
                hashCode = AppUtils.ComputeCyrb53HashCode(path + filename);
            }
            else
            {
                path = "";
                filename = _baseFileName;
                hashCode = AppUtils.ComputeCyrb53HashCode(filename);
            }

            return new UploadFileInfo()
            {
                Name = filename,
                Path = path,
                HashCode = hashCode
            };
        }
    }

}
