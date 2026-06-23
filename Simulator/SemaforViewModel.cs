using System;
using System.ComponentModel;
using DataModel;
using Communicator;


namespace Simulator
{

	class SemaforViewModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Implementare a interfetei INotifyPropertyChanged, 
		/// este necesar acest lucru ca sa functioneze partea de binding
		/// a proprietatilor din view model cu partea de UI
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
		void OnPropertyChanged(string prop)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}
		}

		private BackgroundWorker worker = new BackgroundWorker();
		private System.Timers.Timer timer = new System.Timers.Timer();
		private Sender _aplicatieMonitorizareRetea;

		public SemaforViewModel() { }

		public void Init()
		{
			_aplicatieMonitorizareRetea = new Sender("127.0.0.1", 3000);
			timer.Elapsed += _timer_Elapsed;
			worker.DoWork += _worker_DoWork;
			worker.RunWorkerAsync();
			IsAutoMode = true;
			IsManualMode = false;
		}

		private ProcessState _currentStateOfTheProcess = ProcessState.Off;
		// starea curenta a procesulul
		public ProcessState TheStateOfTheProcess
		{
			get => _currentStateOfTheProcess;
			set
			{
				_currentStateOfTheProcess = value;
				// cand se schimba starea curenta a procesului
				// se notifica aplicatia de monitorizare din retea.
				// se poate renunta la aceasta notificare
				// si se poate trimite direct starea curenta catre WEB API
				_aplicatieMonitorizareRetea.Send(Convert.ToByte(_currentStateOfTheProcess));
			}
		}

		// cand expira perioada de timp setata are loc o tranzitie din starea curenta in urmatoarea stare a procesului
		private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			_isChangingStateInProgress = false;
			TheStateOfTheProcess = _nextState;
			timer.Stop();
		}

		private bool _isChangingStateInProgress = false;
		private ProcessState _nextState;
		/// <summary>
		/// Acesta metoda pregateste tranzitia din starea curenta a procesului 
		/// catre urmatoarea stare. Tranzitia va avea loc in intervalul de timp specificat
		/// prin al doilea parametru
		/// </summary>
		/// <param name="NextProcessState"></param>
		/// <param name="TimeInterval"></param>
		private void ChangeProcessState(ProcessState NextProcessState, int TimeInterval)
		{
			// un eveniment de tranzitie odata ridicat, trebuie sa fie consumat
			// nu se poate ridica un alt eveniment de tranzitie pana cand tranzitia curenta este efectuata
			if (!_isChangingStateInProgress)
			{
				_isChangingStateInProgress = true;
				_nextState = NextProcessState;
				timer.Interval = TimeInterval;
				timer.Start();
			}
		}
		
		private void _worker_DoWork(object sender, DoWorkEventArgs e)
		{
			while (true)
			{
				ComputeNextState(TheStateOfTheProcess);
				System.Threading.Thread.Sleep(100);
			}
		}

		/// <summary>
		/// vrem sa calculam urmatoarea stare a procesului plecand de la starea curenta a procesului
		/// </summary>
		/// <param name="CurrentState"></param>
		public void ComputeNextState(ProcessState CurrentState)
		{
			switch (CurrentState)
			{
				case ProcessState.Off:
					IsRedForCar = false;
					IsYellowForCar = false;
					IsGreenForCar = false;
					IsRedForPeople = false;
					IsGreenForPeople = false;

					ChangeProcessState(ProcessState.Off, 2000);

					break;
				case ProcessState.BlinkOn:
					IsRedForCar = false;
					IsYellowForCar = true;
					IsGreenForCar = false;
					IsRedForPeople = false;
					IsGreenForPeople = false;

					ChangeProcessState(ProcessState.BlinkOff, 2000);

					break;
				case ProcessState.BlinkOff:
					IsRedForCar = false;
					IsYellowForCar = false;
					IsGreenForCar = false;
					IsRedForPeople = false;
					IsGreenForPeople = false;

					ChangeProcessState(ProcessState.BlinkOn, 2000);

					break;
				case ProcessState.AutoRed:
					IsRedForCar = true;
					IsYellowForCar = false;
					IsGreenForCar = false;
					IsRedForPeople = false;
					IsGreenForPeople = true;

					ChangeProcessState(ProcessState.AutoGreen, 5000);

					break;
				case ProcessState.AutoYellow:
					IsRedForCar = false;
					IsYellowForCar = true;
					IsGreenForCar = false;
					IsRedForPeople = true;
					IsGreenForPeople = false;

					ChangeProcessState(ProcessState.AutoRed, 3000);

					break;
				case ProcessState.AutoGreen:
					IsRedForCar = false;
					IsYellowForCar = false;
					IsGreenForCar = true;
					IsRedForPeople = true;
					IsGreenForPeople = false;

					ChangeProcessState(ProcessState.AutoYellow, 10000);

					break;
			}
		}

		public void ForceNextState(ProcessState NextState)
		{
			_isChangingStateInProgress = false;
			timer.Stop();
			TheStateOfTheProcess = NextState;
		}


		private bool _isRedForCar;
		public bool IsRedForCar
		{
			get
			{
				return _isRedForCar;
			}
			set
			{
				_isRedForCar = value;
				OnPropertyChanged(nameof(IsRedForCarsVisible));
			}
		}

		public System.Windows.Visibility IsRedForCarsVisible
		{
			get
			{
				return _isRedForCar ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
			}
		}

		private bool _isYellowForCar;
		public bool IsYellowForCar
		{
			get
			{
				return _isYellowForCar;
			}
			set
			{
				_isYellowForCar = value;
				OnPropertyChanged(nameof(IsYellowForCarsVisible));
			}
		}

		public System.Windows.Visibility IsYellowForCarsVisible
		{
			get
			{
				return _isYellowForCar ?  System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;				
			}
		}

		private bool _isGreenForCar;
		public bool IsGreenForCar
		{
			get
			{
				return _isGreenForCar;
			}
			set
			{
				_isGreenForCar = value;
				OnPropertyChanged(nameof(IsGreenForCarsVisible));
			}
		}

		public System.Windows.Visibility IsGreenForCarsVisible
		{
			get
			{
				return _isGreenForCar ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;				
			}
		}

		private bool _isGreenForPeople;
		public bool IsGreenForPeople
		{
			get
			{
				return _isGreenForPeople;
			}
			set
			{
				_isGreenForPeople = value;
				OnPropertyChanged(nameof(IsGreenForPeopleVisible));
			}
		}

		private bool _isManualMode = true;
		public bool IsManualMode
		{
			get
			{
				return _isManualMode;
			}
			set
			{
				_isManualMode = value;
				OnPropertyChanged(nameof(IsManualMode));
			}
		}

		private bool _isAutoMode = false;
		public bool IsAutoMode
		{
			get
			{
				return _isAutoMode;
			}
			set
			{
				_isAutoMode = value;
				OnPropertyChanged(nameof(IsAutoMode));
			}
		}

		public System.Windows.Visibility IsGreenForPeopleVisible
		{
			get
			{
				return _isGreenForPeople ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;				
			}
		}

		private bool _isRedForPeople;
		public bool IsRedForPeople
		{
			get
			{
				return _isRedForPeople;
			}
			set
			{
				_isRedForPeople = value;
				OnPropertyChanged(nameof(IsRedForPeopleVisible));
			}
		}

		public System.Windows.Visibility IsRedForPeopleVisible
		{
			get
			{
				return _isRedForPeople ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;				
			}
		}
	}
}
