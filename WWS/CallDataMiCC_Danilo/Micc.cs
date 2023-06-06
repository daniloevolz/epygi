using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCASCOMCLIENTLib;

namespace CallDataMiCC_Danilo
{
	class Micc : Log
	{
		private int _port;
		private string _server;
		private string _localIP;
		public bool _connected;
		private bool _sendEvents;
		private string _urlTotvsEvents;
		private string _urlTotvsIncoming;

		CCASClient cCASClient = new CCASClient();
		List<Call> listaCalls = new List<Call>();
		List<Agent> listaAgents = new List<Agent>();
		HttpClient httpClient = new HttpClient();
		//SqlClient sqlClient = new SqlClient();
		SqlClient sqlClient = SqlClient.GetInstance();
		//Log Log = new Log();


		public Micc()
		{
			_server = ConfigurationManager.AppSettings["SERVIDOR"];
			_port = Convert.ToInt32(ConfigurationManager.AppSettings["PORTA"]);
			_localIP = ConfigurationManager.AppSettings["IPLOCAL"];
			_sendEvents = Convert.ToBoolean(ConfigurationManager.AppSettings["ENVIAREVENTOSRECEPTIVO"]);
			_urlTotvsEvents = ConfigurationManager.AppSettings["URLTOTVSEVENTOS"];
			_urlTotvsIncoming = ConfigurationManager.AppSettings["URLTOTVSRECEPTIVO"];
			//sqlClient.Open();
			//sqlClient.Clear();

		}
		public void Disconnect()
        {
			cCASClient.Uninitialize();
        }
		public void Connect()
		{
			cCASClient.Initialize();
			try
			{
				cCASClient.ConnectWithDirectConnect(_server, _port, _localIP);
				_connected = true;
				try
				{
					cCASClient.OnEvent += CCASClient_OnEvent;


				}
				catch (Exception e)
				{
					Send("OnEvent", e.Message, "ERRO");
				}
			}
			catch (Exception e)
			{
				Send("Connect", e.Message, "ERRO");
			}
		}
		public int MakeCall(int lAgentRecID, string bstrDialString, string card, string type, string mode)
		{
			int status;
			cCASClient.GetVoiceReadyStatus(lAgentRecID, out status);
			if (status == 1)
			{
				cCASClient.MakeCall(lAgentRecID, bstrDialString);
				Call call = new Call();
				call._agentId = Convert.ToString(lAgentRecID);
				call._phone = Convert.ToString(bstrDialString);
				call._card = Convert.ToString(card);
				call._type = Convert.ToString(type);
				call._mode = Convert.ToString(mode);
				call._callId = 0;
				listaCalls.Add(call);
				return status;

			}
			else
			{
				return status;
			}

		}
		public string AgentReason(int lAgentRecID)
		{
			string reason = "";
			int index = listaAgents.FindIndex(listaAgents => listaAgents._agentId.Contains(Convert.ToString(lAgentRecID)));
			if (index == -1)
			{
				//Console.WriteLine("Agente ignorado, fora da pilha:");
			}
			else
			{
				reason = listaAgents[index]._reason;
			}
			return reason;
		}
		public int HangupCall(int lAgentRecID)
		{
			int status = 0;
			if (_connected == true)
			{
				try
				{
					int index = listaCalls.FindIndex(listaCalls => listaCalls._agentId.Contains(Convert.ToString(lAgentRecID)));
					if (index != -1)
					{
						cCASClient.HangupCall(lAgentRecID, listaCalls[index]._callId);
						status = 1;
					}

				}
				catch
				{
					status = 0;
				}

			}
			return status;
		}
		private void CCASClient_OnEvent(CCASEvent pEvent)
		{
			DateTime UtcNow = DateTime.UtcNow;
			DateTime gmtNow = UtcNow.AddHours(-3);
			//Console.WriteLine("Evento Recebido :::: ");
			string strData = "Indisponível";
			string strName = "";
			int intData;
			int intVoiceReady;
			int intType;
			pEvent.GetType(out intType);
			switch (intType)
			{
				case 1:
					{
						//Console.WriteLine("Loggon Recebido ===== " + intType.ToString());
						pEvent.GetLongValue("RecID", out intData);
						pEvent.GetStringValue("Name", out strName);
						pEvent.GetLongValue("VoiceReady", out intVoiceReady);

						if (intVoiceReady == 1)
						{
							strData = "";
						}
						Agent agent = new Agent();
						agent._agentId = Convert.ToString(intData);
						agent._agentName = strName;
						agent._reason = strData;
						listaAgents.Add(agent);
						//if (sqlClient._connected is true)
						//{
						//	Task t = new Task(() => { sqlClient.InsertAgent(intData, strName, strData); });
						//	t.Start();
						//}
						//else
						//{
						//	sqlClient.Open();
						//	Task t = new Task(() => { sqlClient.InsertAgent(intData, strName, strData); });
						//	t.Start();
						//}
						//Log arquivo texto para conferência
						Send("===== " + intType.ToString() + " - Logon =====", "id " + intData, "INFO");
						break;
					}
				case 2:
					{
						//Console.WriteLine("Loggof Recebido ===== " + intType.ToString());
						pEvent.GetLongValue("RecID", out intData);
						//pEvent.GetStringValue("Name", out strData);

						int index = listaAgents.FindIndex(listaAgents => listaAgents._agentId.Contains(Convert.ToString(intData)));
						if (index == -1)
						{
							//Console.WriteLine("Agente ignorado, fora da pilha:");
						}
						else
						{
							listaAgents.RemoveAt(index);
						}

						//if (sqlClient._connected is true)
						//{
						//	Task t = new Task(() => { sqlClient.DeleteAgent(intData); });
						//	t.Start();
						//}
						//else
						//{
						//	sqlClient.Open();
						//	Task t = new Task(() => { sqlClient.DeleteAgent(intData); });
						//	t.Start();
						//}
						//Log arquivo texto para conferência
						Send("===== " + intType.ToString() + " - Loggof =====", "id " + intData, "INFO");
						break;
					}
				case 3:
					{
						//Console.WriteLine("Disponivel Recebido ===== " + intType.ToString());
						pEvent.GetLongValue("RecID", out intData);
						//pEvent.GetStringValue("Name", out strData);
						int index = listaAgents.FindIndex(listaAgents => listaAgents._agentId.Contains(Convert.ToString(intData)));
						if (index == -1)
						{
							//Console.WriteLine("Agente ignorado, fora da pilha:");
						}
						else
						{
							listaAgents[index]._reason = "";
						}

						//if (sqlClient._connected is true)
						//{
						//	Task t = new Task(() => { sqlClient.UpdateAgent(intData, ""); });
						//	t.Start();
						//}
						//else
						//{
						//	sqlClient.Open();
						//	Task t = new Task(() => { sqlClient.UpdateAgent(intData, ""); });
						//	t.Start();
						//}
						//Log arquivo texto para conferência
						Send("===== " + intType.ToString() + " - Disponível =====", "id " + intData, "INFO");
						break;
					}
				case 4:
					{
						//Console.WriteLine("Indisponível Recebido ===== " + intType.ToString());
						pEvent.GetLongValue("RecID", out intData);
						pEvent.GetStringValue("ReasonString", out strData);

						int index = listaAgents.FindIndex(listaAgents => listaAgents._agentId.Contains(Convert.ToString(intData)));
						if (index == -1)
						{
							//Console.WriteLine("Agente ignorado, fora da pilha:");
						}
						else
						{
							listaAgents[index]._reason = strData;
						}

						//if (sqlClient._connected is true)
						//{
						//	Task t = new Task(() => { sqlClient.UpdateAgent(intData, strData); });
						//	t.Start();
						//
						//}
						//else
						//{
						//	sqlClient.Open();
						//	Task t = new Task(() => { sqlClient.UpdateAgent(intData, strData); });
						//	t.Start();
						//
						//}
						//Log arquivo texto para conferência
						Send("===== " + intType.ToString() + " - Indisponível =====", "id " + intData, "INFO");
						break;
					}
				case 7:
					{
						int callID;
						//Console.WriteLine("Originando Ligação ===== " + intType.ToString());
						pEvent.GetLongValue("RecID", out intData);
						pEvent.GetLongValue("CallID", out callID);

						int index = listaCalls.FindIndex(listaCalls => listaCalls._agentId.Contains(Convert.ToString(intData)));
						if (index == -1)
						{
							//Console.WriteLine("Chamada ignorada, fora da pilha:");
						}
						else
						{
							listaCalls[index]._callId = callID;
							int indexAgent = listaAgents.FindIndex(listaAgents => listaAgents._agentId.Contains(Convert.ToString(intData)));
							string nameAgent = listaAgents[indexAgent]._agentName;
							Task t = new Task(() => { sqlClient.CallStart(Convert.ToString(callID), Convert.ToString(intData), nameAgent, gmtNow, listaCalls[index]._phone, listaCalls[index]._card, listaCalls[index]._type, listaCalls[index]._mode); });
							t.Start();
						}
						//Log arquivo texto para conferência
						Send("===== " + intType.ToString() + " - Originando =====", "id " + intData + " callid " + callID, "INFO");
						break;
					}
				case 8:
					{
						int callID;
						//Console.WriteLine("Ligação Chamando ===== " + intType.ToString());
						pEvent.GetLongValue("RecID", out intData);
						pEvent.GetLongValue("CallID", out callID);
						pEvent.GetStringValue("OppositePartyNumber", out strData);

						int index = listaCalls.FindIndex(listaCalls => listaCalls._agentId.Contains(Convert.ToString(intData)));
						if (index == -1)
						{
							//Console.WriteLine("Chamada ignorada, fora da pilha:");
						}
						else
						{
							Task t = new Task(() => { sqlClient.CallRing(Convert.ToString(callID), intData, gmtNow); });
							t.Start();
							var objjson = new
							{
								RecID = Convert.ToString(intData),
								Status = "8",
								TimeStamp = gmtNow.ToString("dd/MM/yyyy' 'HH:mm:ss")
							};
							Task t2 = new Task(() => { httpClient.Send(objjson, _urlTotvsEvents); });
							t2.Start();
							Send("===== " + intType.ToString() + " - Originando =====", "id " + intData + " callid " + callID, "INFO");
						}
						//Log arquivo texto para conferência
						//Log("===== " + intType.ToString() + " - Chamando =====", "id " + intData + " callid " + callID, "INFO");
						break;
					}
				case 9:
					{
						int callID;
						//Console.WriteLine("Ligação Estabelecida ===== " + intType.ToString());
						pEvent.GetLongValue("RecID", out intData);
						pEvent.GetLongValue("CallID", out callID);
						pEvent.GetStringValue("OppositePartyNumber", out strData);

						int index = listaCalls.FindIndex(listaCalls => listaCalls._agentId.Contains(Convert.ToString(intData)));
						if (index == -1)
						{
							//Console.WriteLine("Chamada ignorada, fora da pilha:");
						}
						else
						{
							Task t = new Task(() => { sqlClient.CallAnswered(Convert.ToString(callID), intData, gmtNow); });
							t.Start();
							var objjson = new
							{
								RecID = Convert.ToString(intData),
								Status = "9",
								TimeStamp = gmtNow.ToString("dd/MM/yyyy' 'HH:mm:ss")
							};
							Task t2 = new Task(() => { httpClient.Send(objjson, _urlTotvsEvents); });
							t2.Start();
						}
						//Log arquivo texto para conferência
						Send("===== " + intType.ToString() + " - Estabelecida =====", "id " + intData + " callid " + callID, "INFO");
						break;
					}
				case 10:
					{
						int callID;
						//Console.WriteLine("Pausando Ligação ===== " + intType.ToString());
						pEvent.GetLongValue("RecID", out intData);
						pEvent.GetLongValue("CallID", out callID);

						int index = listaCalls.FindIndex(listaCalls => listaCalls._agentId.Contains(Convert.ToString(intData)));
						if (index == -1)
						{
							//Console.WriteLine("Chamada ignorada, fora da pilha:");
						}
						else
						{
							//Task t = new Task(() => { sqlClient.CallHold(Convert.ToString(callID), intData, gmtNow); });
							//t.Start();
							listaCalls[index]._holdStart = gmtNow;

						}
						//Log arquivo texto para conferência
						Send("===== " + intType.ToString() + " - Pausada =====", "id " + intData + " callid " + callID, "INFO");
						break;
					}
				case 11:
					{
						int callID;
						//Console.WriteLine("Recuperando Ligação ===== " + intType.ToString());
						pEvent.GetLongValue("RecID", out intData);
						pEvent.GetLongValue("CallID", out callID);

						int index = listaCalls.FindIndex(listaCalls => listaCalls._agentId.Contains(Convert.ToString(intData)));
						if (index == -1)
						{
							//Console.WriteLine("Chamada ignorada, fora da pilha:");
						}
						else
						{
							TimeSpan diff1 = gmtNow.Subtract(listaCalls[index]._holdStart);
							listaCalls[index]._holdTime = listaCalls[index]._holdTime + diff1;
							//Task t = new Task(() => { sqlClient.CallRetrieved(Convert.ToString(callID), intData, gmtNow); });
							//t.Start();
						}
						//Log arquivo texto para conferência
						Send("===== " + intType.ToString() + " - Recuperada =====", "id " + intData + " callid " + callID, "INFO");
						break;
					}
				case 14:
					{
						int callID;
						//Console.WriteLine("Encerrando Ligação ===== " + intType.ToString());
						pEvent.GetLongValue("RecID", out intData);
						pEvent.GetLongValue("CallID", out callID);

						int index = listaCalls.FindIndex(listaCalls => listaCalls._agentId.Contains(Convert.ToString(intData)));
						if (index == -1)
						{
							//Console.WriteLine("Chamada ignorada, fora da pilha:");
						}
						else
						{
							TimeSpan temp = listaCalls[index]._holdTime;
							Task t = new Task(() => { sqlClient.CallEnd(Convert.ToString(callID), intData, gmtNow, temp); });
							t.Start();
							listaCalls.RemoveAt(index);
							var objjson = new
							{
								RecID = Convert.ToString(intData),
								Status = "14",
								TimeStamp = gmtNow.ToString("dd/MM/yyyy' 'HH:mm:ss")
							};
							Task t2 = new Task(() => { httpClient.Send(objjson, _urlTotvsEvents); });
							t2.Start();
							Send("===== " + intType.ToString() + " - Finalizada =====", "id " + intData + " callid " + callID, "INFO");
						}
						break;
					}
				case 15:
					{
						int callID;
						//Console.WriteLine("Encerrando Clerical ===== " + intType.ToString());
						pEvent.GetLongValue("RecID", out intData);
						pEvent.GetLongValue("CallID", out callID);

						Task t = new Task(() => { sqlClient.ClericalEnd(Convert.ToString(callID), intData, gmtNow); });
						t.Start();
						//Log arquivo texto para conferência
						Send("===== " + intType.ToString() + " - Clerical Encerrado =====", "id " + intData + " callid " + callID, "INFO");


						break;
					}
				case 24:
					{
						if (_sendEvents is true)
						{
							string IVRLabel1, IVRLabel2, IVRLabel3;
							string IVRData1, IVRData2, IVRData3;
							string callID, recID;
							string CallingPartyNumber;
							pEvent.GetStringValue("CallID", out callID);
							pEvent.GetStringValue("RecID", out recID);
							pEvent.GetStringValue("CallingPartyNumber", out CallingPartyNumber);
							pEvent.GetStringValue("IVRLabel1", out IVRLabel1);
							pEvent.GetStringValue("IVRLabel2", out IVRLabel2);
							pEvent.GetStringValue("IVRLabel3", out IVRLabel3);
							pEvent.GetStringValue("IVRData1", out IVRData1);
							pEvent.GetStringValue("IVRData2", out IVRData2);
							pEvent.GetStringValue("IVRData3", out IVRData3);
							var objjson = new
							{
								CallID = callID,
								RecID = recID,
								Callingnumber = CallingPartyNumber,
								IVRLABEL = new
								{
									IVRLabel1,
									IVRLabel2,
									IVRLabel3
								},
								IVRDATA = new
								{
									IVRData1,
									IVRData2,
									IVRData3
								}
							};
							Task t = new Task(() => { httpClient.Send(objjson, _urlTotvsIncoming); });
							t.Start();
						};
						break;
					}
				case 33:
					{
						//Console.WriteLine("Grupos de Serviço ===== " + intType.ToString());
						//pEvent.GetLongValue("RecID", out intData);
						//pEvent.GetStringValue("Name", out strData);


						//Log arquivo texto para conferência
						//Log("===== " + intType.ToString() + " - Grupo de Serviço =====", "id " + intData + " nome " + strData, "INFO");
						break;
					}
				case 35:
					{
						//Log arquivo texto para conferência
						//Log("===== " + intType.ToString() + " - Fim =====", "", "INFO");
						break;
					}

			}
		}
	}
}

