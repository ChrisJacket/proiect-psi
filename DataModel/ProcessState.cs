using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel
{
	/// <summary>
	/// In aceasta enumerare ar trebui sa va adaugati starile 
	/// pe care le identificati pentru procesul vostru.
	/// </summary>
	public enum ProcessState
	{
		Off,

		// mod galben intermitent
		BlinkOn,
		BlinkOff,

		// modului automat
		AutoRed,
		AutoYellow,
		AutoGreen
	}
}
