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

        public void RemoveImageFor(string path)
        {
            LoadedImageInfoResult res;
            _imageInfosLibrary.Remove(path);
        }

        public void LeaveImagesOnlyIn(IEnumerable<IImageHolder> availableHolders)
        {
            _autoLoadingPreviewImage = false;
            _loadingImageCTS.Cancel();
            _loadingImageCTS = new CancellationTokenSource();

            var visiblePaths = availableHolders.Select(i => i.Path).Distinct().ToList();
            _imageInfosLibrary = _imageInfosLibrary.Where(i => visiblePaths.Contains(i.Key)).
                ToDictionary(it=>it.Key,it=>it.Value);
            foreach (var holder in availableHolders.Where(h => !h.ShowImage))
                holder.ReloadAsync(this);
        }

        public void ReloadImages(IEnumerable<IImageHolder> availableHolders)
        {
            foreach (var holder in availableHolders)
            {
                holder.ViewMode = ViewMode.Preview;
                if (!holder.ShowImage)
                    holder.ReloadAsync(this);
            }
        }

        public bool HasImageFile(string path)
        {
            return _fileOperationProxy.FileExists(path);
        }

        public bool CanGetImageInfo(object sender, IImageHolder imageHolder)
        {
            return !(!_autoLoadingPreviewImage && sender != this && imageHolder.ViewMode == ViewMode.Preview);
        }

        public Task<BitmapImageInfo> GetImageInfoAsync(object sender, IImageHolder imageHolder)
        {
            return GetImageInfoAsync(sender, imageHolder, _loadingImageCTS.Token);
        }

        private async Task<BitmapImageInfo> GetImageInfoAsync(object sender, IImageHolder imageHolder, CancellationToken ct)
        {
            if (null == imageHolder)
                throw new ArgumentNullException("imageHolder");

            if (!CanGetImageInfo(sender, imageHolder))
                return null;

            LoadedImageInfoResult imageInfoResult;
            if (_imageInfosLibrary.TryGetValue(imageHolder.Path, out imageInfoResult) &&
                imageInfoResult.ViewMode == imageHolder.ViewMode)
                return imageInfoResult.ImageInfo;

            await _loadingImageSS.WaitAsync(ct);
            try
            {
                if (_imageInfosLibrary.TryGetValue(imageHolder.Path, out imageInfoResult) &&
                    imageInfoResult.ViewMode == imageHolder.ViewMode)
                    return imageInfoResult.ImageInfo;

                ct.ThrowIfCancellationRequested();
                using (var loader = _fileOperationProxy.GetImageLoader(imageHolder, _previewImageHeight))
                {
                    var imageInfo = loader.LoadImageInfo();
                    ct.ThrowIfCancellationRequested();
                    _imageInfosLibrary[imageHolder.Path] = new LoadedImageInfoResult(imageInfo, imageHolder.ViewMode);
                    return imageInfo;
                }
            }
            finally
            {
                _loadingImageSS.Release();
            }
        }

        public void Dispose()
        {
            _loadingImageCTS.Cancel();
            _loadingImageCTS.Dispose();
            _loadingImageSS.Dispose();
        }

        private bool _autoLoadingPreviewImage = true;
        private CancellationTokenSource _loadingImageCTS = new CancellationTokenSource();
        private readonly int _previewImageHeight = 100;
        private SemaphoreSlim _loadingImageSS = new SemaphoreSlim(1);
        private Dictionary<string, LoadedImageInfoResult> _imageInfosLibrary = new Dictionary<string, LoadedImageInfoResult>();
        public readonly FileOperationProxy _fileOperationProxy;
    }
}
