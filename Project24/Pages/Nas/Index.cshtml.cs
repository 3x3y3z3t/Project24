/*  Index.cshtml.cs
 *  Version: 1.0 (2022.10.04)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.IO;
using TusDotNetClient;
using Microsoft.Extensions.FileProviders;

namespace Project24.Pages.Nas
{
    [Authorize(Roles = Constants.ROLE_ADMIN)]
    public partial class IndexModel : PageModel
    {
        public enum FileType
        {
            Unset = 0,
            Directory,
            File
        }

        public class FileModel
        {
            public FileType FileType { get; set; } = FileType.Unset;
            public string Name { get; set; }
            public string Fullname { get; set; }
            public DateTime LastModified { get; set; }
            public long Size { get; set; }

            public FileModel()
            {

            }

        }

        public class DataModel
        {
            public string FullPath { get; set; }
            public IList<string> PathLayer { get; set; }
            public IList<FileModel> Files { get; set; }


            //public DataModel()
            //{ }

            public DataModel(string _fullpath, IList<FileModel> _files)
            {
                FullPath = _fullpath.Replace('\\', '/');
                Files = _files;

                string[] layers = FullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                PathLayer = new List<string>(layers);
            }
        }

        //[BindProperty]
        public DataModel Data { get; set; } = null;



        public IndexModel(ILogger<IndexModel> _logger)
        {


        }

        public async Task<IActionResult> OnGetAsync()
        {
            WriteDriveStatsFile();

            string currentPathLayer = "./";
            TempData["CurrentPath"] = currentPathLayer;

            IList<FileModel> list = GetFilesInDirectory(currentPathLayer);

            Data = new DataModel(currentPathLayer, list);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return BadRequest();


            


        }

        public async Task<IActionResult> OnGetExploreAsync(int? _id)
        {
            string currentPath = (string)TempData["CurrentPath"];
            if (string.IsNullOrEmpty(currentPath))
                return Page();

            if (!_id.HasValue)
            {
                currentPath = "./";
            }
            else if (_id < 0)
            {
                string[] layers = currentPath.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
                currentPath = "";
                for (int i = 0; i < layers.Length && i <= _id * -1; ++i)
                {
                    currentPath += layers[i] + "/";
                }
            }

            IList<FileModel> list = GetFilesInDirectory(currentPath);

            if (!_id.HasValue || _id < 0)
            {
                Data = new DataModel(currentPath, list);
                return Page();
            }

            if (_id > list.Count)
            {
                return Page(); // TODO: maybe return BadRequest();
            }

            if (list[_id.Value].FileType == FileType.Directory)
            {
                string newPathLayer = currentPath + list[_id.Value].Name + "/";
                list = GetFilesInDirectory(newPathLayer);

                Data = new DataModel(newPathLayer, list);

                return Page();
            }

            return Page(); // TODO: maybe return BadRequest();
        }

        public async Task<IActionResult> OnGetDownloadAsync(int? _id)
        {
            if (!_id.HasValue)
                return Page();

            string currentPath = (string)TempData["CurrentPath"];
            if (string.IsNullOrEmpty(currentPath))
                return Page();

            IList<FileModel> list = GetFilesInDirectory(currentPath);

            if (_id < 0 || _id > list.Count)
            {
                return Page(); // TODO: maybe return BadRequest();
            }

            if (list[_id.Value].FileType != FileType.File)
            {
                return Page(); // TODO: maybe return BadRequest();
            }

            FileModel file = list[_id.Value];

            Response.Clear();

            Response.Headers.Add("Content-Disposition", string.Format("attachment; filename={0}", file.Name));
            Response.Headers.Add("Content-Length", file.Size.ToString());
            Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;

            string filePath = file.Fullname.Replace(file.Name, "");
            var fileInfo = new PhysicalFileProvider(filePath).GetFileInfo(file.Name);

            await Response.SendFileAsync(fileInfo);

            return Page();

                //byte[] fileContents = System.IO.File.ReadAllBytes(list[_id.Value].Fullname);

                //return File(fileContents, System.Net.Mime.MediaTypeNames.Application.Octet, list[_id.Value].Name);
            

        }

        private IList<FileModel> GetFilesInDirectory(string _pathLayer)
        {
            List<FileModel> list = new List<FileModel>();

            if (string.IsNullOrEmpty(_pathLayer))
                return list;

            TempData["CurrentPath"] = _pathLayer;

            string fullPath = Path.GetFullPath(Constants.NasRoot + _pathLayer, Constants.WorkingDir);

            string[] directories = Directory.GetDirectories(fullPath);
            string[] files = Directory.GetFiles(fullPath);

            foreach (string directory in directories)
            {
                DirectoryInfo di = new DirectoryInfo(directory);

                FileModel fm = new FileModel()
                {
                    FileType = FileType.Directory,
                    Name = di.Name,
                    LastModified = di.LastWriteTime
                };

                list.Add(fm);
            }

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);

                FileModel fm = new FileModel()
                {
                    FileType = FileType.File,
                    Name = fi.Name,
                    Fullname = fi.FullName,
                    LastModified = fi.LastWriteTime,
                    Size = fi.Length
                };

                list.Add(fm);
            }

            return list;
        }

        private void WriteDriveStatsFile(bool _force = true)
        {
            string dirRoot = Directory.GetDirectoryRoot(Constants.WorkingDir);
            string statusFileFullname = Path.GetFullPath(Constants.NasRoot, Constants.WorkingDir) + "/stats.txt";

            if (_force || !System.IO.File.Exists(statusFileFullname))
            {

                string fileContent = "";

                DriveInfo[] dis = DriveInfo.GetDrives();
                foreach (var di in dis)
                {
                    if (di.RootDirectory.ToString() == dirRoot)
                    {
                        fileContent += "NAS stats:\r\n";

                        fileContent += "Total:                  " + di.TotalSize + "\r\n";
                        fileContent += "    Used:               " + (di.TotalSize - di.AvailableFreeSpace) + "\r\n";
                        fileContent += "    Free:               " + di.AvailableFreeSpace + "\r\n";

                        break;
                    }
                }

                try
                {
                    System.IO.File.WriteAllText(statusFileFullname, fileContent, System.Text.Encoding.UTF8);
                }
                catch (Exception _e)
                { }
            }
        }




    }

}
