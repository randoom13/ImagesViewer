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

        public void AddRanged(IEnumerable<string> filePaths)
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
            _imagesCache.ReloadImages(_availableHolders);
        }

        public Range VisibleItemsRange
        {
            set
            {
                if (value != null)
                    ReInvalidateImagesCache(value);
            }
        }

        private void ReInvalidateImagesCache(Range value)
        {
            _availableHolders = Images.Skip(value.Index).Take(value.Count).ToList();
            if (value.Index == 0 && value.Count == Images.Count)
                return;
            _imagesCache.LeaveImagesOnlyIn(_availableHolders);
        }

        private List<ImageViewModel> _availableHolders;
        private ImageViewModel _selectedImage;
        private IEventAggregator _eventAggregator;
        private ImagesCache _imagesCache;
    }
}
