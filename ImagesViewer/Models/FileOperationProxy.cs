using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagesViewer.Models
{
    public class FileOperationProxy
    {
        public ImageLoader GetImageLoader(IImageHolder imageHolder, int previewImageHeight)
        {
            return new ImageLoader(imageHolder, previewImageHeight);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool IsImage(string path)
        {
            return File.Exists(path) && ImageHelper.GetImageFormatFor(path) != ImageHelper.ImageFormat.unknown;
        }
    }
}
