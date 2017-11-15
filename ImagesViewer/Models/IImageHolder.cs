using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagesViewer.Models
{
    public interface IImageHolder : IProgress<int>
    {
        string Path { get; }
        ViewMode ViewMode { get; set;  }
        void ReloadAsync(object sender);
        bool ShowImage { get; }
    }
}
