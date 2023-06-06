

namespace CallDataMiCC_Danilo
{
	partial class ServiceWorker
	{
		#region Inicialização
		/// <summary> 
		/// Variável de designer necessária.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Limpar os recursos que estão sendo usados.
		/// </summary>
		/// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
		/// <summary> 
		/// Método necessário para suporte ao Designer - não modifique 
		/// o conteúdo deste método com o editor de código.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ServiceName = "CallDataMiCC_Danilo";
			//this.HttpStart();
		}
		#endregion

		//private void HttpStart()
        //{
		//	int _portHttp = Convert.ToInt32(ConfigurationManager.AppSettings["PORTAHTTP"]);
		//	string _localIP = ConfigurationManager.AppSettings["IPLOCAL"];
		//	try
		//	{
		//		HttpServer srvr = new HttpServer(5);
		//		srvr.Start(_portHttp, _localIP);
		//	}
		//	catch (Exception e)
		//	{
		//		Log("Initialize", e.Message, "ERRO");
		//	}
		//}
		//private static void Log(string origem, string mensagem, string debugLevel)
		//{
		//	//Log arquivo texto para conferência
		//	DateTime utcNow = DateTime.UtcNow;
		//	DateTime gmtTime = utcNow.AddHours(-3);
		//	StreamWriter log = new StreamWriter("C:\\users\\public\\log_calldata_debug_danilo.txt", true, Encoding.UTF8);
		//	switch (debugLevel)
		//	{
		//		case "INFO":
		//			log.Write("\r\n " + gmtTime.ToString() + " -- INFO: ");
		//			log.Write(origem.ToString() + " -- ");
		//			log.Write(mensagem.ToString());
		//			log.Close();
		//			break;
		//		case "ERRO":
		//			log.Write("\r\n " + gmtTime.ToString() + " -- ERRO: ");
		//			log.Write(origem.ToString() + " -- ");
		//			log.Write(mensagem.ToString());
		//			log.Close();
		//			break;
		//	}
		//}
	}
}
