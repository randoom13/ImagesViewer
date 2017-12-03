using Caliburn.Micro;
using ImagesViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ImagesViewer.ViewModels
{
    public class ImageViewModel : PropertyChangedBase, IImageHolder
    {
        public ImageViewModel(string filePath, ImagesCache imagesCacheHolder)
        {
            ViewMode = ViewMode.Preview;
            Progress = new ProgressViewModel();
            Path = filePath;
            _imagesCacheHolder = imagesCacheHolder;
        }

        public async void ReloadAsync(object sender)
        {
            _loadedImageInfoTask = Task.Run(async() =>await LoadImageInfoAsync(sender));
            await _loadedImageInfoTask.ConfigureAwait(false);
        }

        public string Path { get; private set; }

        public ViewMode ViewMode
        {
            get { return _viewMode; }
            set
            {
                var isChanged = _viewMode != value;
                _viewMode = value;
                if (isChanged)
                {
                    _loadedImageInfoTask = null;
                    _imageInfoWeakRef = new WeakReference((BitmapImageInfo)null);
                }
            }
        }

        public ProgressViewModel Progress { get; private set; }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                NotifyOfPropertyChange(() => ErrorMessage);
                NotifyOfPropertyChange(() => ShowErrorMessage);
            }
        }

        public string ToolTip
        {
            get { return _toolTip; }
            set
            {
                var isChanged = EqualityComparer<string>.Default.Equals(_toolTip, value);
                _toolTip = value;
                if (isChanged)
                    NotifyOfPropertyChange(() => ToolTip);
            }
        }

        public void Report(int value)
        {
            Progress.Value = value;
        }

        public bool ShowErrorMessage { get { return !string.IsNullOrEmpty(ErrorMessage); } }

        public bool ShowImage  { get { return _imageInfoWeakRef.IsAlive; } }

        public BitmapImageInfo ImageInfo
        {
            get
            {
                if (_loadedImageInfoTask == null)
                    ReloadAsync(this);

                return _imageInfoWeakRef.IsAlive? (BitmapImageInfo)_imageInfoWeakRef.Target : null;
            }
            set
            {
                _imageInfoWeakRef = new WeakReference(value);
                NotifyOfPropertyChange(() => ShowImage);
                NotifyOfPropertyChange(() => ImageInfo);
            }
        }

        public int Index 
        { 
            get { return _index; } 
            set { _index = value; NotifyOfPropertyChange(() => Index); } 
        }

        public bool ShowRing
        { 
            get { return _showRing; } 
            set { _showRing = value; NotifyOfPropertyChange(() => ShowRing); } 
        }

        private void FillToolTipByImageInfo()
        {
            if (!_imageInfoWeakRef.IsAlive)
                return;

            BitmapImageInfo imageInfo = _imageInfoWeakRef.Target as BitmapImageInfo;
            if (imageInfo == null)
                return;
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(imageInfo.Image.PixelHeight);
            stringBuilder.Append("x");
            stringBuilder.Append(imageInfo.Image.PixelWidth);
            stringBuilder.AppendLine();
            stringBuilder.Append(imageInfo.KBLength);
            stringBuilder.Append(" KB");
            ToolTip = stringBuilder.ToString();
        }

        private async Task LoadImageInfoAsync(object sender)
        {
            try
            {
                ImageInfo = null;
                ErrorMessage = "";
                ToolTip = "";
                if (!_imagesCacheHolder.ImageExists(this))
                {
                    ErrorMessage = "Can't load image.";
                    ToolTip = "No file";
                    return;
                }
                if (_imagesCacheHolder.UnableToGetImageInfo(sender, this))
                    return;

                ShowRing = true;
                ErrorMessage = "";
                ImageInfo = await _imagesCacheHolder.GetImageInfoAsync(sender, this);
                FillToolTipByImageInfo();
                Progress.Show = false;
                ShowRing = false;
                ErrorMessage = "";
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                {
                    Progress.Show = false;
                    ShowRing = false;
                    ErrorMessage = "Can't load image.";
                    ToolTip = ex.Message;
                    return;
                }

                if (ex is UnauthorizedAccessException)
                {
                    Progress.Show = false;
                    ShowRing = false;
                    ErrorMessage = "Access is denied.";
                    ToolTip = ex.Message;
                    return;
                }

                throw;
            }
        }

        private WeakReference _imageInfoWeakRef = new WeakReference((BitmapImageInfo)null);
        private int _index;
        private bool _showRing = false;
        private Task _loadedImageInfoTask;
        private ImagesCache _imagesCacheHolder;
        private ViewMode _viewMode;
        private string _toolTip;
        private string _errorMessage;
    }
}
