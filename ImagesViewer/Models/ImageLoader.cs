using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImagesViewer.Models
{
    public enum ViewMode
    {
        Preview,
        Full
    }

    public class ImageLoader : IDisposable
    {
        private readonly int _previewImageHeight;
        private IImageHolder _imageHolder;
        public ImageLoader(IImageHolder imageHolder, int previewImageHeight)
        {
            _imageHolder = imageHolder;
            _previewImageHeight = previewImageHeight;
        }

        public BitmapImageInfo LoadImageInfo()
        {
            BitmapImage image = new BitmapImage();
            try
            {
                image.DownloadProgress += OnDownloadProgress;
                image.BeginInit();
                if (_imageHolder.ViewMode == ViewMode.Preview)
                {
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.DecodePixelHeight = _previewImageHeight;
                }
                image.UriSource = new Uri(_imageHolder.Path);
                image.EndInit();
                var fileInfo = new FileInfo(_imageHolder.Path);
                return new BitmapImageInfo((BitmapImage)image.GetAsFrozen(), fileInfo.Length);
            }
            finally
            {
                image.DownloadProgress -= OnDownloadProgress;
            }
        }
        
        private void OnDownloadProgress(object sender, DownloadProgressEventArgs arg)
        {
           _imageHolder.Report(arg.Progress);
        }

        public void Dispose()
        {
            _imageHolder = null;
        }
    }
}
