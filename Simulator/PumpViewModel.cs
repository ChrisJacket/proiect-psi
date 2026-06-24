using System.Windows.Media;

namespace Simulator
{
    public class PumpViewModel : ViewModelBase
    {
        public PumpViewModel(int index)
        {
            Index = index;
            Label = "M" + (index + 1);
            LampLabel = "P" + (index + 1);
            ButtonLabel = "S" + (index + 1);
        }

        public int Index { get; }
        public string Label { get; }
        public string LampLabel { get; }
        public string ButtonLabel { get; }

        private bool _available = true;
        public bool Available
        {
            get => _available;
            set
            {
                if (Set(ref _available, value))
                {
                    Raise(nameof(LampBrush));
                    Raise(nameof(MotorBrush));
                    Raise(nameof(StatusText));
                }
            }
        }

        private bool _running;
        public bool Running
        {
            get => _running;
            set
            {
                if (Set(ref _running, value))
                {
                    Raise(nameof(MotorBrush));
                    Raise(nameof(StatusText));
                }
            }
        }

        public double CumulativeRunningMs { get; set; }
        public double CumulativeOffMs { get; set; }

        public Brush LampBrush => _available
            ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))
            : new SolidColorBrush(Color.FromRgb(0x40, 0x52, 0x66));

        public Brush MotorBrush
        {
            get
            {
                if (!_available) return new SolidColorBrush(Color.FromRgb(0x40, 0x52, 0x66));
                return _running
                    ? new SolidColorBrush(Color.FromRgb(0x22, 0xD3, 0xEE))
                    : new SolidColorBrush(Color.FromRgb(0x2A, 0x38, 0x49));
            }
        }

        public string StatusText
        {
            get
            {
                if (!_available) return "indisponibilă";
                return _running ? "pornită" : "oprită";
            }
        }
    }
}
