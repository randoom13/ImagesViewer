using GongSolutions.Wpf.DragDrop;
using ImagesViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImagesViewer.ViewModels
{
    public class DropHandler : IDropTarget
    {
        public DropHandler(ImagesListViewModel imagesList, FileOperationProxy fileOperationProxy)
        {
            _fileOperationProxy = fileOperationProxy;
            _imagesList = imagesList;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            var dragFileList = GetFileDropList(dropInfo);
            dropInfo.Effects = dragFileList.Any(path => _fileOperationProxy.IsImage(path)) ?
                DragDropEffects.Copy : DragDropEffects.None;
        }

        public void Drop(IDropInfo dropInfo)
        {
            var dragFileList = GetFileDropList(dropInfo);
            _imagesList.AddRange(dragFileList.Where(path => _fileOperationProxy.IsImage(path)));
        }

        private static IEnumerable<string> GetFileDropList(IDropInfo dropInfo)
        {
            var dataObject = dropInfo.Data as DataObject;
            if (dataObject == null)
                return Enumerable.Empty<string>();
            return dataObject.GetFileDropList().OfType<string>();
        }

        private ImagesListViewModel _imagesList;
        private FileOperationProxy _fileOperationProxy;
    }
}
