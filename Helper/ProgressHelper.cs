using AIGS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGS.Helper
{
    public class ProgressHelper: ViewMoudleBase
    {
        public enum STATUS
        {
            WAIT,
            RUNNING,
            COMPLETE,
            ERROR,
            CANCLE,
        }

        private float  _value;
        private string _errmsg;
        private STATUS _status;
        public System.Windows.Media.SolidColorBrush _statuscolor;
        public string _statusmsg { get; set; }

        public  float Value { get { return _value; } set { _value = value; OnPropertyChanged(); } }
        public string Errmsg { get { return _errmsg; } set { _errmsg = value; OnPropertyChanged(); } }
        public System.Windows.Media.SolidColorBrush StatusColor { get { return _statuscolor; } set { _statuscolor = value; OnPropertyChanged(); } }
        private STATUS Status { get { return _status; } set { _status = value; OnPropertyChanged(); } }
        public string StatusMsg
        {
            get
            {
                switch(_status)
                {
                    case STATUS.COMPLETE:
                        StatusColor = System.Windows.Media.Brushes.Green;
                        return "[SUCCESS]";
                    case STATUS.CANCLE:
                        StatusColor = System.Windows.Media.Brushes.DarkOrange;
                        return "[CANCEL]";
                    case STATUS.ERROR:
                        StatusColor = System.Windows.Media.Brushes.DarkRed;
                        return "[ERR]";
                    case STATUS.WAIT:
                        StatusColor = System.Windows.Media.Brushes.Green;
                        return "[WAIT]";
                    default:
                        StatusColor = System.Windows.Media.Brushes.Green;
                        if (_value == 0 && _statusmsg.IsNotBlank())
                            return _statusmsg;
                        return _value.ToString("#0.00") + "%";
                }
            }
            set
            {
                _statusmsg = value;
                OnPropertyChanged();
            }
        }

        public ProgressHelper()
        {
            Clear();

        }

        public void Clear()
        {
            Value       = 0;
            Errmsg      = null;
            Status      = STATUS.WAIT;
            StatusColor = System.Windows.Media.Brushes.Green;
            StatusMsg   = "";
        }

        public void Update(long lCurSize, long lAllSize)
        {
            try{
                Value = (float)(lCurSize * 100) / lAllSize;
                StatusMsg = "";
            }
            catch { }
        }

        public void SetStatus(STATUS status)
        {
            Status = status;
            StatusMsg = "";
        }
        public STATUS GetStatus()
        {
            return Status;
        }
    }
}
