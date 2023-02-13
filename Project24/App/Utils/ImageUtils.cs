/*  App/Utils/ImageUtils.cs
 *  Version: 1.0 (2023.02.13)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using System.IO;
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Project24.App.Utils
{
    public static class ImageUtils
    {
        public static bool GenerateThumbnailIfNotExisted(P24ImageModelBase _image)
        {
            return GenerateThumbnailIfNotExisted(_image.Path, _image.Name);
        }

        public static bool GenerateThumbnailIfNotExisted(List<P24ImageViewModel> _images)
        {
            foreach(var image in _images)
            {
                GenerateThumbnailIfNotExisted(image.Path, image.Name);
            }

            return true;
        }

        public static bool GenerateThumbnailIfNotExisted(string _path, string _name)
        {
            string srcPath = Path.GetFullPath(DriveUtils.DataRootPath + "/" + _path + "/" + _name);
            string thumbDir = Path.GetFullPath(DriveUtils.DataRootPath + "/thumb/" + _path);
            if (!File.Exists(srcPath) || File.Exists(thumbDir + "/" + _name))
                return false;

            Directory.CreateDirectory(thumbDir);

            Image image = Image.Load(srcPath);

            float scaleX = (float)AppConfig.ImageMaxWidth / image.Width;
            float scaleY = (float)AppConfig.ImageMaxHeight / image.Height;

            float scale = scaleX;
            if (scale > scaleY)
                scale = scaleY;

            int newX = (int)(image.Width * scale);
            int newY = (int)(image.Height * scale);

            image.Mutate(_img => _img.Resize(newX, newY, KnownResamplers.Lanczos3));

            image.Save(thumbDir + "/" + _name);

            image.Dispose();

            return true;
        }
    }

}
