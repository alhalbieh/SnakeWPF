using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SnakeWPF.Models
{
    public class CustomTimer : DispatcherTimer, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public new TimeSpan Interval
        {
            get { return base.Interval; }
            set { base.Interval = value; NotifyPropertyChanged(); }
        }

        public CustomTimer()
        {

        }
    }
}
