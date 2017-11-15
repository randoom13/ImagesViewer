using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImagesViewer.Models
{
    public class BitmapImageInfo
    {
      public BitmapImage Image { get; private set; }
      public long KBLength { get; private set; }
      public BitmapImageInfo(BitmapImage image, long length) 
      {
          Image = image;
          KBLength = length / 1024;
      }
    }
}
