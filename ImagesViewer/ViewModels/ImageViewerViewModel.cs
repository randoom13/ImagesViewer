using Caliburn.Micro;
using ImagesViewer.Events;
using System;
using ImagesViewer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagesViewer.ViewModels
{
    public class ImageViewerViewModel : Screen, ICancellable
    {
        public ImageViewerViewModel(IEventAggregator eventAggregator,
            BindableCollection<ImageViewModel> images, ImagesCache imagesCacheHolder)
        {
            Images = images;
            _imagesCache = imagesCacheHolder;
            _eventAggregator = eventAggregator;
        }

        public BindableCollection<ImageViewModel> Images { get; private set; }

        public ImageViewModel SelectedImage
        {
            get { return _selectedImage; }
            set
            {
                if (_selectedImage != null && !ReferenceEquals(_selectedImage, value))
                {
                    _selectedImage.ViewMode = Models.ViewMode.Preview;
                    _imagesCache.RemoveImage(_selectedImage);
                }
                _selectedImage = value;
                _selectedImage.ViewMode = Models.ViewMode.Full;
                NotifyOfPropertyChange(() => SelectedImage);
            }
        }

        public void Cancel()
        {
            _eventAggregator.PublishOnUIThread(new MoveBackEvent());
        }

        private ImageViewModel _selectedImage;
        private IEventAggregator _eventAggregator;
        private ImagesCache _imagesCache;
    }
}
