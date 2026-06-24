using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using Communicator;
using DataModel;
using Newtonsoft.Json;

namespace Simulator
{
    public class PumpSystemViewModel : ViewModelBase
    {
        private const double TickMs = 100.0;
        private const double PumpFlowPerSecond = 1.5;
        private const double PressureGainPerUnitFlow = 0.35;
        private const double MaxPressure = 12.0;
        private const double MinPressure = 0.0;
        private const double CriticalPressure = 10.5;
        private const double DeadbandBar = 0.15;
        private const int SustainedMsRequired = 1000;
        private const int AlarmDurationMs = 5000;
        private const int NetworkPushIntervalMs = 500;

        private readonly DispatcherTimer _timer;
        private readonly Random _random = new Random();
        private Sender _sender;
        private DateTime _lastNetworkPush = DateTime.MinValue;
        private int _belowSetpointMs;
        private int _aboveSetpointMs;
        private int _alarmRemainingMs;

        public PumpSystemViewModel()
        {
            for (int i = 0; i < 4; i++) Pumps.Add(new PumpViewModel(i));
            _timer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromMilliseconds(TickMs)
            };
            _timer.Tick += OnTick;
        }

        public ObservableCollection<PumpViewModel> Pumps { get; } = new ObservableCollection<PumpViewModel>();

        private double _pressure = 0.0;
        public double Pressure
        {
            get => _pressure;
            private set
            {
                if (Set(ref _pressure, value))
                {
                    Raise(nameof(PressureText));
                    Raise(nameof(PressureBarPercent));
                    Raise(nameof(PressureStatusBrush));
                }
            }
        }

        private double _setpoint = 6.0;
        public double Setpoint
        {
            get => _setpoint;
            set
            {
                if (Set(ref _setpoint, value)) Raise(nameof(SetpointText));
            }
        }

        private double _consumptionRate = 2.0;
        public double ConsumptionRate
        {
            get => _consumptionRate;
            set
            {
                if (Set(ref _consumptionRate, value)) Raise(nameof(ConsumptionText));
            }
        }

        private SystemMode _mode = SystemMode.Off;
        public SystemMode Mode
        {
            get => _mode;
            private set
            {
                if (Set(ref _mode, value))
                {
                    Raise(nameof(ModeText));
                    Raise(nameof(IsAuto));
                    Raise(nameof(IsOff));
                }
            }
        }

        private bool _alarm;
        public bool Alarm
        {
            get => _alarm;
            private set
            {
                if (Set(ref _alarm, value)) Raise(nameof(AlarmBrush));
            }
        }

        private bool _b2Faulted;
        public bool B2Faulted
        {
            get => _b2Faulted;
            private set
            {
                if (Set(ref _b2Faulted, value))
                {
                    Raise(nameof(B2StatusText));
                    Raise(nameof(B2StatusBrush));
                }
            }
        }

        public string B2StatusText => _b2Faulted ? "B2 DEFECT" : "B2 OK";
        public System.Windows.Media.Brush B2StatusBrush => _b2Faulted
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xEF, 0x44, 0x44))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x22, 0xC5, 0x5E));

        public bool IsAuto => Mode == SystemMode.Auto;
        public bool IsOff => Mode == SystemMode.Off;

        public string PressureText => Pressure.ToString("F2") + " bar";
        public string SetpointText => Setpoint.ToString("F1") + " bar";
        public string ConsumptionText => ConsumptionRate.ToString("F1") + " L/s";
        public string ModeText => Mode == SystemMode.Auto ? "AUTO" : "OFF";
        public double PressureBarPercent => Math.Min(100.0, Math.Max(0.0, Pressure / MaxPressure * 100.0));

        public System.Windows.Media.Brush PressureStatusBrush
        {
            get
            {
                if (Pressure >= CriticalPressure) return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xEF, 0x44, 0x44));
                if (Pressure < Setpoint - DeadbandBar) return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF5, 0x9E, 0x0B));
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x22, 0xC5, 0x5E));
            }
        }

        public System.Windows.Media.Brush AlarmBrush => _alarm
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xEF, 0x44, 0x44))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x40, 0x52, 0x66));

        public string AlarmText => _alarm ? "ALARMĂ B1" : "Normal";

        public void Start()
        {
            try
            {
                _sender = new Sender("127.0.0.1", 3000);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Sim] Monitor offline: " + ex.Message);
                _sender = null;
            }
            _timer.Start();
            PushState(force: true);
        }

        public void ToggleSystemPower()
        {
            Mode = Mode == SystemMode.Off ? SystemMode.Auto : SystemMode.Off;
            if (Mode == SystemMode.Off) StopAllPumps();
            PushState(force: true);
        }

        public void StopAllPumps()
        {
            foreach (var p in Pumps) p.Running = false;
            _belowSetpointMs = 0;
            _aboveSetpointMs = 0;
            PushState(force: true);
        }

        public void EmergencyStop()
        {
            Mode = SystemMode.Off;
            StopAllPumps();
        }

        public void ToggleB2Fault()
        {
            B2Faulted = !B2Faulted;
            PushState(force: true);
        }

        public void ToggleAvailability(int pumpIndex)
        {
            var p = Pumps[pumpIndex];
            p.Available = !p.Available;
            if (!p.Available) p.Running = false;
            PushState(force: true);
        }

        private void OnTick(object sender, EventArgs e)
        {
            UpdateCumulativeTimes();
            UpdatePressure();
            UpdateAlarm();
            if (Mode == SystemMode.Auto && !Alarm)
            {
                RunControlLoop();
            }
            PushState(force: false);
        }

        private void UpdateCumulativeTimes()
        {
            foreach (var p in Pumps)
            {
                if (p.Running) p.CumulativeRunningMs += TickMs;
                else p.CumulativeOffMs += TickMs;
            }
        }

        private void UpdatePressure()
        {
            int running = Pumps.Count(p => p.Running);
            double inflow = running * PumpFlowPerSecond;
            double net = inflow - ConsumptionRate;
            double delta = net * PressureGainPerUnitFlow * (TickMs / 1000.0);
            double newPressure = Pressure + delta;
            if (newPressure < MinPressure) newPressure = MinPressure;
            if (newPressure > MaxPressure) newPressure = MaxPressure;
            Pressure = newPressure;
        }

        private void UpdateAlarm()
        {
            if (_alarmRemainingMs > 0)
            {
                _alarmRemainingMs -= (int)TickMs;
                if (_alarmRemainingMs <= 0)
                {
                    Alarm = false;
                    _alarmRemainingMs = 0;
                }
            }
            else if (Pressure >= CriticalPressure)
            {
                Alarm = true;
                _alarmRemainingMs = AlarmDurationMs;
                B2Faulted = false; // după alarmă, B2 se „recalibrează" (revine la normal)
                EmergencyStop();
            }
        }

        private void RunControlLoop()
        {
            // Cu B2 defect: senzorul de presiune folosit de control "vede" mereu 0 bar,
            // deci bucla crede că trebuie să pornească mereu mai multe pompe.
            // Presiunea reală urcă necontrolat → B1 (senzor independent) declanșează alarma.
            if (_b2Faulted)
            {
                _belowSetpointMs += (int)TickMs;
                _aboveSetpointMs = 0;
                if (_belowSetpointMs >= SustainedMsRequired)
                {
                    if (StartLeastUsedPump()) _belowSetpointMs = 0;
                }
                return;
            }

            if (Pressure < Setpoint - DeadbandBar)
            {
                _belowSetpointMs += (int)TickMs;
                _aboveSetpointMs = 0;
                if (_belowSetpointMs >= SustainedMsRequired)
                {
                    if (StartLeastUsedPump()) _belowSetpointMs = 0;
                }
            }
            else if (Pressure > Setpoint + DeadbandBar)
            {
                _aboveSetpointMs += (int)TickMs;
                _belowSetpointMs = 0;
                if (_aboveSetpointMs >= SustainedMsRequired)
                {
                    if (StopMostUsedPump()) _aboveSetpointMs = 0;
                }
            }
            else
            {
                _belowSetpointMs = 0;
                _aboveSetpointMs = 0;
            }
        }

        private bool StartLeastUsedPump()
        {
            var candidate = Pumps
                .Where(p => p.Available && !p.Running)
                .OrderByDescending(p => p.CumulativeOffMs)
                .ThenBy(p => p.Index)
                .FirstOrDefault();
            if (candidate == null) return false;
            candidate.Running = true;
            candidate.CumulativeOffMs = 0;
            PushState(force: true);
            return true;
        }

        private bool StopMostUsedPump()
        {
            var candidate = Pumps
                .Where(p => p.Running)
                .OrderByDescending(p => p.CumulativeRunningMs)
                .ThenBy(p => p.Index)
                .FirstOrDefault();
            if (candidate == null) return false;
            candidate.Running = false;
            candidate.CumulativeRunningMs = 0;
            PushState(force: true);
            return true;
        }

        private void PushState(bool force)
        {
            var now = DateTime.Now;
            if (!force && (now - _lastNetworkPush).TotalMilliseconds < NetworkPushIntervalMs) return;
            _lastNetworkPush = now;

            var ev = new PumpSystemStatusEvent(
                pressure: Math.Round(Pressure, 3),
                setpoint: Setpoint,
                pumps: Pumps.Select(p => p.Running).ToArray(),
                lamps: Pumps.Select(p => p.Available).ToArray(),
                alarm: Alarm,
                mode: Mode,
                consumptionRate: ConsumptionRate,
                stateChangedDate: now);

            if (_sender == null) return;
            try
            {
                string json = JsonConvert.SerializeObject(ev);
                _sender.Send(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Sim] push fail: " + ex.Message);
            }
        }
    }
}
