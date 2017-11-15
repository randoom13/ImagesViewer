using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagesViewer.ViewModels
{
    public class ProgressViewModel : PropertyChangedBase
    {
        public void Reset()
        {
            Maximum = 100;
            Value = 0;
        }

        public bool Show 
        { 
            get { return _show; } 
            set { _show = value; NotifyOfPropertyChange(() => Show); } 
        }

        public double Maximum
        {
            get { return _maximum; }
            private set { _maximum = value; NotifyOfPropertyChange(() => Maximum); }
        }
        public double Value { get { return _value; } set { _value = value; NotifyOfPropertyChange(() => Value); } }
        
        private double _maximum;
        private double _value;
        private bool _show = false;
    }
}
