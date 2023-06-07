using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WWS_Epygi
{
	class Log
	{
		private bool _logEnabled;
		private string _logFile;
		public Log()
		{
			_logEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["LOG"]);
			_logFile = ConfigurationManager.AppSettings["LOGFILE"];
		}
		public void Send(string origem, string mensagem, string debugLevel)
		{
			try
			{
				// Defina o nome do seu semáforo com um identificador único para evitar conflitos com outros semáforos
				string semaphoreName = "logSemaphore_" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString();

				using (SemaphoreSlim semaphore = new SemaphoreSlim(1))
				{
					semaphore.Wait();

					// Se o semáforo foi adquirido, você pode escrever no arquivo de log com segurança
					FileInfo fileinfo = new FileInfo(_logFile);
					if (fileinfo.Exists && fileinfo.Length >= 1048576)
					{
						FileInfo fileinfo0 = new FileInfo(_logFile+".0");
						if (fileinfo0.Exists)
						{
							fileinfo0.Delete();
						}
						File.Move(_logFile, _logFile+".0");
						EventLog.WriteEntry("WWS_log_debug", "Arquivo de Log "+ _logFile + " recriado pois atingiu o limite de tamanho 10MB!", EventLogEntryType.Information);
					}
					//Log arquivo texto para conferência
					DateTime utcNow = DateTime.UtcNow;
					DateTime gmtTime = utcNow.AddHours(-3);
					StreamWriter log = new StreamWriter(_logFile, true, Encoding.UTF8);
					switch (debugLevel)
					{
						case "INFO":
							if (_logEnabled)
							{
								log.Write("\r\n " + gmtTime.ToString() + " -- INFO: ");
								log.Write(origem.ToString() + " -- ");
								log.Write(mensagem.ToString());
								log.Close();
							}
							break;
						case "ERRO":
							log.Write("\r\n " + gmtTime.ToString() + " -- ERRO: ");
							log.Write(origem.ToString() + " -- ");
							log.Write(mensagem.ToString());
							log.Close();
							break;
					}
					log.Dispose();

					// Libere o semáforo para permitir que outras threads possam escrever no arquivo de log
					semaphore.Release();
				}
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry("WWS_log_debug", ex.Message, EventLogEntryType.Error);
			}
		}
		
	}
}


