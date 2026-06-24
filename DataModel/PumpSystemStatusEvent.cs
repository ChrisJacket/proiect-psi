using System;

namespace DataModel
{
    public class PumpSystemStatusEvent
    {
        public PumpSystemStatusEvent()
        {
            Pumps = new bool[4];
            Lamps = new bool[4];
        }

        public PumpSystemStatusEvent(
            double pressure,
            double setpoint,
            bool[] pumps,
            bool[] lamps,
            bool alarm,
            SystemMode mode,
            double consumptionRate,
            DateTime stateChangedDate)
        {
            Pressure = pressure;
            Setpoint = setpoint;
            Pumps = pumps;
            Lamps = lamps;
            Alarm = alarm;
            Mode = mode;
            ConsumptionRate = consumptionRate;
            StateChangedDate = stateChangedDate;
        }

        public double Pressure { get; set; }

        public double Setpoint { get; set; }

        public bool[] Pumps { get; set; }

        public bool[] Lamps { get; set; }

        public bool Alarm { get; set; }

        public SystemMode Mode { get; set; }

        public double ConsumptionRate { get; set; }

        public DateTime StateChangedDate { get; set; }
    }
}
