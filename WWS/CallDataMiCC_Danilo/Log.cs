using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CallDataMiCC_Danilo
{
	class Log
	{
		private bool _logEnabled;
		public Log()
		{
			_logEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["LOG"]);
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
					FileInfo fileinfo = new FileInfo("C:\\log_calldata_debug_danilo.txt");
					if (fileinfo.Exists && fileinfo.Length >= 1048576)
					{
						FileInfo fileinfo0 = new FileInfo("C:\\log_calldata_debug_danilo.txt.0");
						if (fileinfo0.Exists)
						{
							fileinfo0.Delete();
						}
						File.Move("C:\\log_calldata_debug_danilo.txt", "C:\\log_calldata_debug_danilo.txt.0");
						EventLog.WriteEntry("ServicoURA-CallDataMiCC_Danilo", "Arquivo de Log log_calldata_debug_danilo recriado pois atingiu o limite de tamanho 10MB!", EventLogEntryType.Information);
					}
					//Log arquivo texto para conferência
					DateTime utcNow = DateTime.UtcNow;
					DateTime gmtTime = utcNow.AddHours(-3);
					StreamWriter log = new StreamWriter("C:\\log_calldata_debug_danilo.txt", true, Encoding.UTF8);
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
				EventLog.WriteEntry("ServicoURA-CallDataMiCC_Danilo", ex.Message, EventLogEntryType.Error);
			}
		}
		public void SendOld(string origem, string mensagem, string debugLevel)
		{
			try
			{

				FileInfo fileinfo = new FileInfo("C:\\log_calldata_debug_danilo.txt");
				if (fileinfo.Exists && fileinfo.Length >= 1048576)
				{
					FileInfo fileinfo0 = new FileInfo("C:\\log_calldata_debug_danilo.txt.0");
					if (fileinfo0.Exists)
					{
						fileinfo0.Delete();
					}
					File.Move("C:\\log_calldata_debug_danilo.txt", "C:\\log_calldata_debug_danilo.txt.0");
					EventLog.WriteEntry("ServicoURA-CallDataMiCC_Danilo", "Arquivo de Log log_calldata_debug_danilo recriado pois atingiu o limite de tamanho 10MB!", EventLogEntryType.Information);
				}
				//Log arquivo texto para conferência
				DateTime utcNow = DateTime.UtcNow;
				DateTime gmtTime = utcNow.AddHours(-3);
				StreamWriter log = new StreamWriter("C:\\log_calldata_debug_danilo.txt", true, Encoding.UTF8);
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
			}
			catch (Exception ex)
			{
				EventLog.WriteEntry("ServicoURA-CallDataMiCC_Danilo", ex.Message, EventLogEntryType.Error);
			}
		}
	}
}

