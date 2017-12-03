using Caliburn.Micro;
using ImagesViewer.Events;
using System.Windows.Input;
using ImagesViewer.Models;
using ImagesViewer.Controls;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace ImagesViewer.ViewModels
{
    public class ImagesListViewModel : Screen
    {
        public ImagesListViewModel(IEventAggregator eventAggregator,
            ImagesCache imagesCache,
            BindableCollection<ImageViewModel> images,
            FileOperationProxy fileOperationProxy)
        {
            Images = images;
            _eventAggregator = eventAggregator;
            _imagesCache = imagesCache;
            Images.CollectionChanged += delegate { NotifyOfPropertyChange(() => EmptyImagesList); };
            DropHandler = new DropHandler(this, fileOperationProxy);
        }

        public DropHandler DropHandler { get; private set; }

        public bool EmptyImagesList { get { return !Images.Any(); } }

        public BindableCollection<ImageViewModel> Images { get; private set; }

        public ImageViewModel SelectedImage
        {
            get { return _selectedImage; }
            set
            {
                _selectedImage = value;
                NotifyOfPropertyChange(() => SelectedImage);
            }
        }

        public void AddRange(IEnumerable<string> filePaths)
        {
            var images = filePaths.Select(path => new ImageViewModel(path, _imagesCache)).ToList();
            Images.AddRange(images);
            images.ForEach(i => i.Index = Images.IndexOf(i));
        }

        public void OnPreviewMouseDown(MouseButtonEventArgs args)
        {
            if (args.ClickCount == 2)
            {
                _imagesCache.StopLoading();
                _eventAggregator.PublishOnCurrentThread(new MoveForwardEvent());
                args.Handled = true;
            }
        }

        public void ReloadCache()
        {
            _imagesCache.ReloadPreviewImages(_visibleImages);
        }

        public bool HasVisibleScroll
        {
            set 
            {
               _imagesCache.ShowAllPreviewImages = value;
               if (!value)
               {
                   _visibleImages = Images.ToList();
                   _imagesCache.LoadImages(_visibleImages);
               }
            }
        }

        public Range VisibleItemsRange
        {
            set
            {
                if (value != null && _imagesCache.ShowAllPreviewImages)
                {
                    _visibleImages = Images.Skip(value.Index).Take(value.Count).ToList();
                    _imagesCache.LeaveOnlyImages(_visibleImages);
                }
            }
        }

        private List<ImageViewModel> _visibleImages;
        private ImageViewModel _selectedImage;
        private IEventAggregator _eventAggregator;
        private ImagesCache _imagesCache;
    }
}
