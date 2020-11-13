
using BasicMvvm;
using Windows.UI.Xaml.Media.Imaging;

namespace PingTool.Models
{
    public class PingMassage : BindableBase
    {
        private string _ipAddress;
        private int _size;
        private long _time;
        private int _ttl;
        private string _response;
        public string Response
        {
            get { return _response; }
            set
            {
                _response = value;
                OnPropertyChanged();
            }
        }
        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                OnPropertyChanged();
            }
        }
        public int Size
        {
            get { return _size; }
            set
            {
                _size = value;
                OnPropertyChanged();
            }
        }
        public long Time
        {
            get { return _time; }
            set
            {
                _time = value;
                OnPropertyChanged();
            }
        }
        public int Ttl
        {
            get { return _ttl; }
            set
            {
                _ttl = value;
                OnPropertyChanged();
            }
        }
    }
}
