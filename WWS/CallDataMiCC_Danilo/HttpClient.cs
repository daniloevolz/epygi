using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CallDataMiCC_Danilo
{
	class HttpClient : Log
	{
		private string _header;
		private string _token;
		//private Log _log;

		public HttpClient()
		{

			_header = ConfigurationManager.AppSettings["HEADER"];
			_token = ConfigurationManager.AppSettings["TOKEN"];
			//_log = new Log();
		}

		public int Send(object objjson, string url)
		{
			HttpWebResponse resposta;
			StreamReader reader;
			//DateTime data = Convert.ToDateTime(dataAcordada);
			//dataAcordada = data.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
			//string dataFollowUp = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");

			try
			{
				//Log arquivo texto para conferência
				//Log("AgendaPagamento", urlCartoes, "INFO");
				//

				// Chama o Webservice.
				//var objjson = new
				//{
				//	cpf = usuario.cpf,
				//	dataAcordada = dataAcordada,
				//	dataFollowUp = dataFollowUp
				//};



				var serializer = new JavaScriptSerializer();
				var serializedResult = serializer.Serialize(objjson);

				//Log arquivo texto para conferência
				Send("HttpClient.Send", url+" = "+serializedResult, "INFO");
				//
				ServicePointManager.MaxServicePointIdleTime = 1000;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				WebRequest request = WebRequest.Create(url);
				request.Headers.Add(_header, _token);
				request.Method = "POST";
				request.ContentType = "application/json";
				//request.Headers["Authentication-token"] = token;
				byte[] byteArray = Encoding.UTF8.GetBytes(serializedResult);
				request.ContentLength = byteArray.Length;

				Stream dataStream = request.GetRequestStream();
				dataStream.Write(byteArray, 0, byteArray.Length);
				dataStream.Close();

				resposta = (HttpWebResponse)request.GetResponse();
				Stream stream = resposta.GetResponseStream();
				reader = new StreamReader(stream, Encoding.UTF8);

				//return result;
				int statusCode = (int)resposta.StatusCode;
				resposta.Close();
				return statusCode;
			}
			catch (TimeoutException timeProblem)
			{
				//Log arquivo texto para conferência
				Send("HttpClient.Send", timeProblem.Message, "ERRO");
				//
				return 500;
			}
			catch (Exception ex)
			{
				//Log arquivo texto para conferência
				Send("HttpClient.Send", ex.Message, "ERRO");
				//
				return 500;
			}
		}
	}
}

