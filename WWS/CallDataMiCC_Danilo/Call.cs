using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallDataMiCC_Danilo
{
	class Call
	{
		public string _agentId { get; set; }
		public string _card { get; set; }
		public string _type { get; set; }
		public string _mode { get; set; }
		public string _phone { get; set; }
		public int _callId { get; set; }
		public DateTime _holdStart { get; set; }
		public TimeSpan _holdTime { get; set; }

	}
}
