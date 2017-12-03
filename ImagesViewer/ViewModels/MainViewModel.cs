using Caliburn.Micro;
using ImagesViewer.Events;
using ImagesViewer.Models;

namespace ImagesViewer.ViewModels
{
    public class MainViewModel : Conductor<Screen>.Collection.OneActive, IHandle<MoveForwardEvent>, IHandle<MoveBackEvent>
    {
        public MainViewModel()
        {
            var images = new BindableCollection<ImageViewModel>();
            var fileOperationProxy = new FileOperationProxy();
            var imagesCache = new ImagesCache(fileOperationProxy);
            _eventAggregator = new EventAggregator();
            _eventAggregator.Subscribe(this);
            _imagesList = new ImagesListViewModel(_eventAggregator, imagesCache, images, fileOperationProxy);
            _imageViewer = new ImageViewerViewModel(_eventAggregator, images, imagesCache);
            this.Items.Add(_imagesList);
            this.Items.Add(_imageViewer);
            this.ActiveItem = _imagesList;
            CancelActiveViewCommand = new RelayCommand<object>(_ =>
            {
                var refreshCommand = ActiveItem as ICancellable;
                if (null != refreshCommand)
                    refreshCommand.Cancel();
            });
        }

        public void Handle(MoveForwardEvent message)
        {
            _imageViewer.SelectedImage = _imagesList.SelectedImage;
            ActiveItem = _imageViewer;
        }

        public void Handle(MoveBackEvent message)
        {
            ActiveItem = _imagesList;
           _imagesList.ReloadCache();
        }

        public RelayCommand<object> CancelActiveViewCommand { get; private set; }

        private readonly ImagesListViewModel _imagesList;
        private readonly ImageViewerViewModel _imageViewer;
        private IEventAggregator _eventAggregator;
    }
}
