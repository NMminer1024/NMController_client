using System;
using System.ComponentModel;

namespace NMController
{

    public class NMDevice : INotifyPropertyChanged
    {
        public String IP { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private String _hashRate;
        public String HashRate
        {
            get { return _hashRate; }
            set
            {
                _hashRate = value;
                OnPropertyChanged("HashRate");
            }
        }

        private String? _share;
        public String? Share
        {
            get { return _share; }
            set
            {
                _share = value;
                OnPropertyChanged("Share");
            }
        }

        private String _netDiff;
        public  String NetDiff
        {
            get { return _netDiff; }
            set
            {
                _netDiff = value;
                OnPropertyChanged("NetDiff");
            }
        }

        private String _poolDiff;
        public String PoolDiff
        {
            get { return _poolDiff; }
            set
            {
                _poolDiff = value;
                OnPropertyChanged("PoolDiff");
            }
        }

        private String _lastDiff;
        public String LastDiff
        {
            get { return _lastDiff; }
            set
            {
                _lastDiff = value;
                OnPropertyChanged("LastDiff");
            }
        }

        private String _bestDiff;
        public String BestDiff
        {
            get { return _bestDiff; }
            set
            {
                _bestDiff = value;
                OnPropertyChanged("BestDiff");
            }
        }

        private Int32 _valid;
        public Int32 Valid
        {
            get { return _valid; }
            set
            {
                _valid = value;
                OnPropertyChanged("Valid");
            }
        }

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }

        private double _temp;
        public double Temp
        {
            get { return _temp; }
            set
            {
                _temp = value;
                OnPropertyChanged("Temp");
            }
        }

        private double _rssi;
        public double RSSI
        {
            get { return _rssi; }
            set
            {
                _rssi = value;
                OnPropertyChanged("RSSI");
            }
        }

        private double _freeHeap;
        public double FreeHeap
        {
            get { return _freeHeap; }
            set
            {
                _freeHeap = value;
                OnPropertyChanged("FreeHeap");
            }
        }

        private String? _version;
        public String? Version
        {
            get { return _version; }
            set
            {
                _version = value;
                OnPropertyChanged("Version");
            }
        }

        private String _boardType;
        public String? BoardType
        {
            get { return _boardType; }
            set
            {
                _boardType = value;
                OnPropertyChanged("BoardType");
            }
        }


        private String _uptime;
        public String Uptime
        {
            get { return _uptime; }
            set
            {
                _uptime = value;
                OnPropertyChanged("Uptime");
            }
        }

        private String? _updateTime;
        public String? UpdateTime
        {
            get { return _updateTime; }
            set
            {
                _updateTime = value;
                OnPropertyChanged("UpdateTime");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is NMDevice device)
            {
                return IP == device.IP;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return IP.GetHashCode();
        }
    }
}