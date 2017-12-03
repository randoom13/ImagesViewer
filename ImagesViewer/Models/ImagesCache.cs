using ImagesViewer.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImagesViewer.Models
{
    public class ImagesCache : IDisposable
    {
        private class LoadedImageInfoResult
        {
            public BitmapImageInfo ImageInfo { get; private set; }
            public ViewMode ViewMode { get; private set; }

            public LoadedImageInfoResult(BitmapImageInfo imageInfo, ViewMode viewMode)
            {
                ImageInfo = imageInfo;
                ViewMode = viewMode;
            }
        }

        public ImagesCache(FileOperationProxy fileOperationProxy)
        {
            _fileOperationProxy = fileOperationProxy;
        }

        public void StopLoading()
        {
            _loadingImageCTS.Cancel();
            _loadingImageCTS = new CancellationTokenSource();
        }

        public void RemoveImage(IImageHolder imageHolder)
        {
            _imageInfosLibrary.Remove(imageHolder.Path);
        }

        public bool ShowAllPreviewImages { get; set; }

        public void LoadImages(IEnumerable<IImageHolder> availableImages)
        {
            foreach (var image in availableImages.Where(h => !h.ShowImage))
                image.ReloadAsync(this);
        }

        public void LeaveOnlyImages(IEnumerable<IImageHolder> availableImages)
        {
            StopLoading();

            var visiblePaths = availableImages.Select(i => i.Path).Distinct().ToList();
            _imageInfosLibrary = _imageInfosLibrary.Where(i => visiblePaths.Contains(i.Key)).
                ToDictionary(it => it.Key, it => it.Value);
            LoadImages(availableImages);
        }

        public void ReloadPreviewImages(IEnumerable<IImageHolder> availableImages)
        {
            foreach (var holder in availableImages)
            {
                holder.ViewMode = ViewMode.Preview;
                if (!holder.ShowImage)
                    holder.ReloadAsync(this);
            }
        }

        public bool ImageExists(IImageHolder image)
        {
            return _fileOperationProxy.FileExists(image.Path);
        }

        public bool UnableToGetImageInfo(object sender, IImageHolder image)
        {
            return !ShowAllPreviewImages && sender != this && image.ViewMode == ViewMode.Preview;
        }

        public Task<BitmapImageInfo> GetImageInfoAsync(object sender, IImageHolder imageHolder)
        {
            return GetImageInfoAsync(sender, imageHolder, _loadingImageCTS.Token);
        }

        private async Task<BitmapImageInfo> GetImageInfoAsync(object sender, IImageHolder image, CancellationToken ct)
        {
            if (null == image)
                throw new ArgumentNullException("imageHolder");

            if (UnableToGetImageInfo(sender, image))
                return null;

            LoadedImageInfoResult imageInfoResult;
            if (_imageInfosLibrary.TryGetValue(image.Path, out imageInfoResult) &&
                imageInfoResult.ViewMode == image.ViewMode)
                return imageInfoResult.ImageInfo;

            await _loadingImagesSS.WaitAsync();
            try
            {
                if (ct.IsCancellationRequested)
                    return null;
                if (_imageInfosLibrary.TryGetValue(image.Path, out imageInfoResult) &&
                    imageInfoResult.ViewMode == image.ViewMode)
                    return imageInfoResult.ImageInfo;

                if (ct.IsCancellationRequested)
                    return null;
                using (var loader = _fileOperationProxy.GetImageLoader(image, _previewImageHeight))
                {
                    var imageInfo = loader.LoadImageInfo();
                    if (ct.IsCancellationRequested)
                        return null;
                    _imageInfosLibrary[image.Path] = new LoadedImageInfoResult(imageInfo, image.ViewMode);
                    return imageInfo;
                }
            }
            finally
            {
                _loadingImagesSS.Release();
            }
        }

        public void Dispose()
        {
            _loadingImageCTS.Cancel();
            _loadingImageCTS.Dispose();
            _loadingImagesSS.Dispose();
        }

        private CancellationTokenSource _loadingImageCTS = new CancellationTokenSource();
        private readonly int _previewImageHeight = 100;
        private SemaphoreSlim _loadingImagesSS = new SemaphoreSlim(1);
        private Dictionary<string, LoadedImageInfoResult> _imageInfosLibrary = new Dictionary<string, LoadedImageInfoResult>();
        public readonly FileOperationProxy _fileOperationProxy;
    }
}
