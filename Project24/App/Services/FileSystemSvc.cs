/*  App/Service/FileSystemSvc.cs
 *  Version: v1.0 (2023.06.27)
 *  
 *  Contributor
 *      Arime-chan
 */


using System.Collections.Generic;
using System.IO;

namespace Project24.App.Services
{
    public class FileSystemSvc
    {
        public string AppRoot { get; private set; }
        public string AppNextRoot { get; private set; }
        public string AppPrevRoot { get; private set; }


        public FileSystemSvc()
        {
            AppRoot = Directory.GetCurrentDirectory();
            AppNextRoot = Path.GetFullPath(AppRoot + "/" + Constants.AppNextDir);
            AppPrevRoot = Path.GetFullPath(AppRoot + "/" + Constants.AppPrevDir);



            ReconstructDirectories();
        }


        public void ReconstructDirectories()
        {
            Directory.CreateDirectory(AppNextRoot);
            Directory.CreateDirectory(AppPrevRoot);



        }

        public List<P24FileInfo> GetFiles(string _fullPath)
        {
            if (!IsPathValid(_fullPath))
                return null;


            List<P24FileInfo> result = new();
            DirectoryInfo dirInfo = new(_fullPath);
            if (!dirInfo.Exists)
                return result;

            var filesEnum = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories);

            foreach (FileInfo file in filesEnum)
            {
                result.Add(new P24FileInfo(file));
            }

            return result;
        }



        public List<P24FileInfo> GetFilesInMain() => GetFiles(AppRoot);
        public List<P24FileInfo> GetFilesInNext() => GetFiles(AppNextRoot);
        public List<P24FileInfo> GetFilesInPrev() => GetFiles(AppPrevRoot);



        private bool IsPathValid(string _path)
        {

            return true;
        }


    }

}
