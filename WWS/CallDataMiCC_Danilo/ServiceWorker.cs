using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CallDataMiCC_Danilo
{
	partial class ServiceWorker : ServiceBase
	{
		private Log _log;
		HttpServer _srvr = new HttpServer(5);
		public ServiceWorker()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			// TODO: Adicione aqui o código para iniciar seu serviço.
			_log = new Log();
			HttpStart();
		}

		protected override void OnStop()
		{
			// TODO: Adicione aqui o código para realizar qualquer desmontagem necessária para interromper seu serviço.
			_srvr.Dispose();
		}
		private void HttpStart()
		      {
			int _portHttp = Convert.ToInt32(ConfigurationManager.AppSettings["PORTAHTTP"]);
			string _localIP = ConfigurationManager.AppSettings["IPLOCAL"];
			try
			{
				
				_srvr.Start(_portHttp, _localIP);
			}
			catch (Exception e)
			{
				_log.Send("Initialize", e.Message, "ERRO");
			}
		}
	}
}
