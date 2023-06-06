using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using SimpleTcp;  //supersimpletcp
using SuperSimpleTcp;

namespace WWS_Epygi
{
    partial class EpygiClient : ServiceBase
    {
        public EpygiClient()
        {
            InitializeComponent();
        }
        #region Variables
        string server = ConfigurationManager.AppSettings["SERVIDOR"];
        int port = Convert.ToInt32(ConfigurationManager.AppSettings["PORTA"]);
        List<Agent> listaAgents = new List<Agent>();
        List<Call> listaCalls = new List<Call>();
        HttpClient httpClient = new HttpClient();
        bool _sendEvents = Convert.ToBoolean(ConfigurationManager.AppSettings["ENVIAREVENTOSRECEPTIVO"]);
        string _urlTotvsEvents = ConfigurationManager.AppSettings["URLTOTVSEVENTOS"];
        string _urlTotvsIncoming = ConfigurationManager.AppSettings["URLTOTVSRECEPTIVO"];
        string usuario = "4101";
        string senha = "68816719758561";//"86158117678575"; //
        const string CrLf = "\r\n\r\n";

        #endregion
        public void Start()
        {
            Thread t = new Thread(new ThreadStart(Connect));
            t.IsBackground = true;
            t.Start();
        }
        void Connect()
        {
            string host = server + ":" + port;
            ReferenciaClient.Client = new SimpleTcpClient(host);
            ReferenciaClient.Client.Events.Connected += Events_Connected;
            ReferenciaClient.Client.Events.DataReceived += Events_DataReceived;
            ReferenciaClient.Client.Events.Disconnected += Events_Disconected;
            ReferenciaClient.Client.Connect();


        }

        private void Events_Disconected(object sender, ConnectionEventArgs e)
        {
            //Console.WriteLine("Desconectado");
            //Console.WriteLine("Reconectando....");
            Connect();
        }

        private void Events_DataReceived(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Data);
            Console.WriteLine(data);

            try
            {
                string[] stringSeparators = new string[] { "\r\n\r\n" };
                string[] xmlStrings;
                xmlStrings = data.Split(stringSeparators, StringSplitOptions.None);

                for (int i = 0; i < xmlStrings.Length; i++)
                {
                    XmlDocument xmltest = new XmlDocument();
                    xmltest.LoadXml(xmlStrings[i].ToString());
                    XmlNodeList elemlist = xmltest.GetElementsByTagName("methodName");
                    string result = elemlist[0].InnerXml;
                    if (result == "Services.CallProcessingService.callstatechanged")
                    {
                        XmlNodeList elemlistCallId = xmltest.GetElementsByTagName("string");

                        int index = listaCalls.FindIndex(listaCalls => listaCalls._callId.Contains(elemlistCallId[0].InnerXml));
                        if (index != -1)
                        {

                            XmlNodeList elemlistStatus = xmltest.GetElementsByTagName("int");
                            switch (Convert.ToInt32(elemlistStatus[0].InnerXml))
                            {
                                case 1:
                                    {
                                        Console.WriteLine("CallID: " + elemlistCallId[0].InnerXml);
                                        Console.WriteLine("Chamando: " + elemlistStatus[0].InnerXml);
                                        Console.WriteLine("Erro: " + elemlistStatus[1].InnerXml);
                                        break;
                                    }
                                case 2:
                                    {
                                        Console.WriteLine("CallID: " + elemlistCallId[0].InnerXml);
                                        Console.WriteLine("Confirmado: " + elemlistStatus[0].InnerXml);
                                        Console.WriteLine("Erro: " + elemlistStatus[1].InnerXml);
                                        index = listaCalls.FindIndex(listaCalls => listaCalls._callId.Contains(elemlistCallId[0].InnerXml));
                                        if (index != -1)
                                        {
                                            TransferCall(listaCalls[index]._callId, listaCalls[index]._number);
                                        }

                                        break;
                                    }
                                case 3:
                                    {
                                        Console.WriteLine("CallID: " + elemlistCallId[0].InnerXml);
                                        Console.WriteLine("Encerrado: " + elemlistStatus[0].InnerXml);
                                        Console.WriteLine("Erro: " + elemlistStatus[1].InnerXml);
                                        listaCalls.RemoveAt(index);
                                        break;
                                    }
                                case 6:
                                    {
                                        Console.WriteLine("CallID: " + elemlistCallId[0].InnerXml);
                                        Console.WriteLine("Falha: " + elemlistStatus[0].InnerXml);
                                        Console.WriteLine("Erro: " + elemlistStatus[1].InnerXml);
                                        listaCalls.RemoveAt(index);
                                        break;
                                    }
                            }


                        }

                    }
                    if (result == "Services.CallProcessingService.prslistarrived")
                    {
                        string id = GetIDParamValue(xmltest);
                        string status = GetStatusValue(xmltest);

                        Console.WriteLine("ID " + id);
                        Console.WriteLine("STATUS " + status);
                        int index = listaAgents.FindIndex(listaAgents => listaAgents._agentId.Contains(id));
                        if (index != -1)
                        { 

                        }



                            ParsePresenceXml(Convert.ToString(xmltest));
                    }

                    xmltest.RemoveAll();
                }

            }
            catch
            {
                //Não é status de chamada
                // Verificar se a autenticação foi bem-sucedida
                if (data.Contains("<string>Authenticated.</string>"))
                {
                    Console.WriteLine("Autenticado, Subscrevendo...");
                    //Unsubscribe("1844674407");
                    Subscribe(usuario);
                }

            }

        }


        private void Events_Connected(object sender, ConnectionEventArgs e)
        {
            //Console.WriteLine("Conectado " + e);

            Authenticate();
        }

        public void Authenticate(string user, string password)
        {

            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("methodCall",
                                             new XElement("methodName", "Services.CallProcessingService.authenticate"),
                                             new XElement("params",
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", user))),
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", password))))), CrLf);
            var wr = new StringWriter();
            xml.Save(wr);
            Console.Write(wr.ToString());
            //SendText(wr.ToString());
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(xml.ToString());

            ReferenciaClient.Client.Send(data);



        }
        public int MakeCall(string recId, string number)
        {
            int status = 0;
            if (number.Length == 4)
            {
                number = "pbx:" + number;
            }
            else if (number.Length == 2)
            {
                number = "ar:" + number;
            }
            else
            {
                number = "pstn:0" + number;
            }
            Random randNum = new Random();
            int callId = randNum.Next();

            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("methodCall",
                                             new XElement("methodName", "Services.CallProcessingService.createcall"),
                                             new XElement("params",
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", callId))),
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", recId))),
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", number))))), CrLf);
            var wr = new StringWriter();
            xml.Save(wr);
            Console.Write(wr.ToString());
            //status = SendText(wr.ToString());
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(xml.ToString());

            ReferenciaClient.Client.Send(data);
            Call call = new Call();
            call._agentId = recId;
            call._callId = callId;
            listaCalls.Add(call);

            return status;

        }
        public void TransferCall(string callId, string number)
        {
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("methodCall",
                                             new XElement("methodName", "Services.CallProcessingService.transfercall"),
                                             new XElement("params",
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", callId))),
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", number))))), CrLf);
            var wr = new StringWriter();
            xml.Save(wr);
            Console.Write(wr.ToString());
            //status = SendText(wr.ToString());
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(xml.ToString());

            ReferenciaClient.Client.Send(data);


        }
        public int HangupCall(string recId)
        {
            int status = 0;
            int index = listaCalls.FindIndex(listaCalls => listaCalls._agentId.Contains(recId));
            if (index != -1)
            {
                var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("methodCall",
                                             new XElement("methodName", "Services.CallProcessingService.closecall"),
                                             new XElement("params",
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", listaCalls[index]._callId)))
                                             )), CrLf);
                var wr = new StringWriter();
                xml.Save(wr);
                Console.Write(wr.ToString());
                //status = SendText(wr.ToString());
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(xml.ToString());

                ReferenciaClient.Client.Send(data);
                return status;
            }


            return status;

        }
        public bool Subscribe(string recId)
        {
            // Cria uma instância da classe Random
            Random random = new Random();

            // Gera um número aleatório entre 0 e 100
            int randomNumber = random.Next(0, 9999999);
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("methodCall",
                                             new XElement("methodName", "Services.CallProcessingService.subscribe"),
                                             new XElement("params",
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", randomNumber))),
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("int", "9"))),
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("struct",
                                             new XElement("member",
                                             new XElement("name", "extension"),
                                             new XElement("value",
                                             new XElement("string", recId)))))))), CrLf);
            var wr = new StringWriter();
            xml.Save(wr);
            Console.Write(wr.ToString());
            //SendText(wr.ToString());
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(xml.ToString());
            ReferenciaClient.Client.Send(data);
            return true;
        }
        public bool Unsubscribe(string subId)
        {
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("methodCall",
                                             new XElement("methodName", "Services.CallProcessingService.unsubscribe"),
                                             new XElement("params",
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", subId))))), CrLf);
            var wr = new StringWriter();
            xml.Save(wr);
            Console.Write(wr.ToString());
            //SendText(wr.ToString());
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(xml.ToString());
            ReferenciaClient.Client.Send(data);
            return true;
        }
        public static void ParsePresenceXml(string xml)
        {
            XDocument xmlDoc = XDocument.Parse(xml);

            XElement statusElement = xmlDoc.Descendants("member")
                                        .FirstOrDefault(e => (string)e.Element("name") == "Status");

            if (statusElement != null)
            {
                string statusValue = (string)statusElement.Element("value").Element("string");
                Console.WriteLine("Status: " + statusValue);
            }

            XElement lastParamElement = xmlDoc.Descendants("param").LastOrDefault();

            if (lastParamElement != null)
            {
                string lastParamValue = (string)lastParamElement.Element("value").Element("string");
                Console.WriteLine("Param: " + lastParamValue);
            }
        }


        public static string GetStatusValue(XmlDocument xmlDoc)
        {
            XmlNode statusNode = xmlDoc.SelectSingleNode("//member[name='Status']/value/string");
            if (statusNode != null)
            {
                return statusNode.InnerText;
            }
            return null;
        }

        public static string GetIDParamValue(XmlDocument xmlDoc)
        {
            XmlNodeList paramNodes = xmlDoc.SelectNodes("//param/value/string");
            if (paramNodes.Count > 0)
            {
                XmlNode lastParamNode = paramNodes[paramNodes.Count - 1];
                return lastParamNode.InnerText;
            }
            return null;
        }

        protected override void OnStart(string[] args)
        {
            // TODO: Adicione aqui o código para iniciar seu serviço.
        }

        protected override void OnStop()
        {
            // TODO: Adicione aqui o código para realizar qualquer desmontagem necessária para interromper seu serviço.
        }
      
    }
}
