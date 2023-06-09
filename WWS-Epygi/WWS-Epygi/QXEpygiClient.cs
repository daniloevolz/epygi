using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using SuperSimpleTcp;  //supersimpletcp

namespace WWS_Epygi
{
    class QXEpygiClient : Log
    {
        #region Variables
        string server = ConfigurationManager.AppSettings["SERVIDOR"];
        int port = Convert.ToInt32(ConfigurationManager.AppSettings["PORTA"]);
        List<Call> listaCalls = new List<Call>();
        List<User> listUsers = new List<User>();
        HttpClient httpClient = new HttpClient();
        bool _sendEvents = Convert.ToBoolean(ConfigurationManager.AppSettings["ENVIAREVENTOSRECEPTIVO"]);
        string _urlEvents = ConfigurationManager.AppSettings["URLTOTVSEVENTOS"];
        string _urlIncoming = ConfigurationManager.AppSettings["URLTOTVSRECEPTIVO"];
        string usuario = "4101";
        string senha = "68816719758561";//"86158117678575"; //
        const string CrLf = "\r\n\r\n";
        int _startRetries = 5;
        string filePath = ConfigurationManager.AppSettings["USERSJSONPATCH"];
        SimpleTcpClient client;
        //Log _log = new Log();

        #endregion
        public void Start()
        {
            Thread t = new Thread(new ThreadStart(Connect));
            t.IsBackground = true;
            Send("QXEpygiClient:Start", "StartThread", "INFO");
            t.Start();
        }
        void Connect()
        {
            string host = server + ":" + port;
            Send("QXEpygiClient:Connect", "TO " + host, "INFO");
            // instantiate
            client = new SimpleTcpClient(host);
            client.Events.Connected += Events_Connected;
            client.Events.DataReceived += Events_DataReceived;
            client.Events.Disconnected += Events_Disconected;
            try
            {
                client.ConnectWithRetries(10000);
            }
            catch (TimeoutException ex)
            {
                Send("QXEpygiClient:Connect", "ERRO "+ex.Message, "ERRO");
            }
            

        }

        private void Events_Disconected(object sender, ConnectionEventArgs e)
        {
            Send("QXEpygiClient:Events_Disconected", "", "INFO");
            _startRetries += 1;
            if (_startRetries < 5)
            {
                Start();
            }
            else
            {
                Send("QXEpygiClient:Events_Disconected", "Todas as tentativas foram realizadas","ERRO");
            }
        }

        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Data.Array, 0, e.Data.Count);
            //Console.WriteLine(data);

            //try
            //{
            string[] stringSeparators = new string[] { "\r\n\r\n" };
            string[] xmlStrings;
            xmlStrings = data.Split(stringSeparators, StringSplitOptions.None);

            for (int i = 0; i < xmlStrings.Length - 1; i++)
            {
                XmlDocument xmltest = new XmlDocument();
                xmltest.LoadXml(xmlStrings[i].ToString());
                XmlNodeList elemlistmethodName = xmltest.GetElementsByTagName("methodName");
                XmlNodeList elemlistmethodResponse = xmltest.GetElementsByTagName("methodResponse");
                if (elemlistmethodName.Count > 0)
                {
                    //Retorno Assíncrono method
                    string result = elemlistmethodName[0].InnerXml;
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
                                        //Console.WriteLine("CallID: " + elemlistCallId[0].InnerXml);
                                        //Console.WriteLine("Chamando: " + elemlistStatus[0].InnerXml);
                                        //Console.WriteLine("Erro: " + elemlistStatus[1].InnerXml);
                                        break;
                                    }
                                case 2:
                                    {
                                        //Console.WriteLine("CallID: " + elemlistCallId[0].InnerXml);
                                        //Console.WriteLine("Confirmado: " + elemlistStatus[0].InnerXml);
                                        //Console.WriteLine("Erro: " + elemlistStatus[1].InnerXml);
                                        index = listaCalls.FindIndex(listaCalls => listaCalls._callId.Contains(elemlistCallId[0].InnerXml));
                                        if (index != -1)
                                        {
                                            TransferCall(listaCalls[index]._callId, listaCalls[index]._number);
                                        }

                                        break;
                                    }
                                case 3:
                                    {
                                        //Console.WriteLine("CallID: " + elemlistCallId[0].InnerXml);
                                        //Console.WriteLine("Encerrado: " + elemlistStatus[0].InnerXml);
                                        //Console.WriteLine("Erro: " + elemlistStatus[1].InnerXml);
                                        listaCalls.RemoveAt(index);
                                        break;
                                    }
                                case 6:
                                    {
                                        //Console.WriteLine("CallID: " + elemlistCallId[0].InnerXml);
                                        //Console.WriteLine("Falha: " + elemlistStatus[0].InnerXml);
                                        //Console.WriteLine("Erro: " + elemlistStatus[1].InnerXml);
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
                        int index = listUsers.FindIndex(listUsers => listUsers.ID.Contains(id));
                        if (index != -1)
                        {
                            foreach (User user in listUsers)
                            {
                                if (user.ID == id)
                                {
                                    user.Status = status;
                                    Send("Services.CallProcessingService.prslistarrived", "Services.CallProcessingService.prslistarrived: Found Num: "+user.Num+", ID: "+user.ID+", Status: "+user.Status,"INFO");

                                }


                            }

                        }
                        //ParsePresenceXml(Convert.ToString(xmltest));
                    }

                }
                if (elemlistmethodResponse.Count > 0)
                {
                    //Retorno Síncrono
                    string responseValue = GetResponseValue(xmltest);
                    //Console.WriteLine($"Events_DataReceived: responseValue: {responseValue}");
                    if (responseValue == "1")
                    {
                        foreach (User user in listUsers)
                        {
                            if (user.ID == null && user.State == "sent")
                            {
                                // Cria uma instância da classe Random
                                Random random = new Random();
                                // Gera um número aleatório entre 0 e 100
                                string rand = Convert.ToString(random.Next(0, 9999999));
                                user.ID = rand;
                                Send("Services.CallProcessingService.prslistarrived", "Authenticated: Found Num: "+user.Num+", ID: "+user.ID, "INFO");
                                user.State = "auth";
                                //Console.WriteLine("Authenticated, Subscribe...");
                                //Unsubscribe("1844674407");
                                Subscribe(rand, user.Num);
                            }


                        }
                        // Loop para imprimir os parâmetros Num e Password
                        foreach (User user in listUsers)
                        {
                            if (user.ID == null && user.State == null)
                            {
                                Send("Services.CallProcessingService.prslistarrived", "Authenticate New User: Num " + user.Num + ", Password: " + user.Password, "INFO");
                                //Console.WriteLine($"Authenticate New User: Num: {user.Num}, Password: {user.Password}");
                                Authenticate(user.Num, user.Password);
                                user.State = "sent";
                                break;
                            }

                        }

                    }
                    if (responseValue == "0")
                    {
                        foreach (User user in listUsers)
                        {
                            if (user.ID == null && user.State == "sent")
                            {
                                string responseString = GetResponseString(xmltest);
                                Send("Services.CallProcessingService.prslistarrived", "ERRO Authenticate: Found Num: "+user.Num+", ERRO: "+responseString,"ERRO");
                                user.State = "faill";
                            }
                        }
                        // Loop para imprimir os parâmetros Num e Password
                        foreach (User user in listUsers)
                        {
                            if (user.ID == null && user.State == null)
                            {
                                Send("Services.CallProcessingService.prslistarrived", "Authenticate New User: Num: "+user.Num+", Password: "+user.Password,"INFO");
                                Authenticate(user.Num, user.Password);
                                user.State = "sent";
                                break;
                            }

                        }

                    }

                }
                xmltest.RemoveAll();
            }
            //}
            //catch(Exception ex)
            //{
            //Não é status
            //.WriteLine($"Events_DataReceived: ERRO: {Convert.ToString(ex.Message)}");

            //}

        }


        private void Events_Connected(object sender, ConnectionEventArgs e)
        {
            Send("Events_Connected", "Connected ","INFO");

            // Leitura do conteúdo do arquivo
            string jsonContent = File.ReadAllText(filePath);

            // Desserialização do JSON
            listUsers = JsonConvert.DeserializeObject<User>(jsonContent).Users;

            // Loop para imprimir os parâmetros Num e Password
            foreach (User user in listUsers)
            {
                if (user.ID == null && user.State == null)
                {
                    Send("Events_Connected","First Authenticate: Num: "+user.Num+", Password: "+user.Password,"INFO");
                    Authenticate(user.Num, user.Password);
                    user.State = "sent";
                    break;
                }

            }

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
            //Console.Write(wr.ToString());
            //SendText(wr.ToString());
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(xml.ToString());

            client.Send(data);



        }
        public int MakeCall(string recID, string number)
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
                                             new XElement("string", recID))),
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", number))))), CrLf);
            var wr = new StringWriter();
            xml.Save(wr);
            //Console.Write(wr.ToString());
            //status = SendText(wr.ToString());
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(xml.ToString());

            client.Send(data);
            Call call = new Call();
            call._agentId = recID;
            call._callId = Convert.ToString(callId);
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
            //Console.Write(wr.ToString());
            //status = SendText(wr.ToString());
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(xml.ToString());

            client.Send(data);


        }
        public int HangupCall(string recID)
        {
            int status = 0;
            int index = listaCalls.FindIndex(listaCalls => listaCalls._agentId.Contains(recID));
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
                //Console.Write(wr.ToString());
                //status = SendText(wr.ToString());
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(xml.ToString());

                client.Send(data);
                return status;
            }


            return status;

        }
        public bool Subscribe(string id, string num)
        {
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("methodCall",
                                             new XElement("methodName", "Services.CallProcessingService.subscribe"),
                                             new XElement("params",
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", id))),
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("int", "9"))),
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("struct",
                                             new XElement("member",
                                             new XElement("name", "extension"),
                                             new XElement("value",
                                             new XElement("string", num)))))))), CrLf);
            var wr = new StringWriter();
            xml.Save(wr);
            //Console.Write(wr.ToString());
            //SendText(wr.ToString());
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(xml.ToString());
            client.Send(data);
            return true;
        }
        public void Unsubscribe()
        {
            foreach (User user in listUsers)
            {
                if (user.ID != null && user.State == "auth")
                {
                    Send("Unsubscribe", "Unsubscribe: Found Num: "+user.Num+", Password: "+user.Password,"INFO");
                    user.State = "unsubscribe";
                    //Unsubscribe("1844674407");
                    var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("methodCall",
                                             new XElement("methodName", "Services.CallProcessingService.unsubscribe"),
                                             new XElement("params",
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", user.ID))))), CrLf);
                    var wr = new StringWriter();
                    xml.Save(wr);
                    Console.Write(wr.ToString());
                    //SendText(wr.ToString());
                    Byte[] data = System.Text.Encoding.ASCII.GetBytes(xml.ToString());
                    client.Send(data);

                }
            }
            client.Disconnect();
        }
        public string GetStatusValue(XmlDocument xmlDoc)
        {
            XmlNode statusNode = xmlDoc.SelectSingleNode("//member[name='Status']/value/string");
            if (statusNode != null)
            {
                return statusNode.InnerText;
            }
            return null;
        }

        public string GetIDParamValue(XmlDocument xmlDoc)
        {
            XmlNodeList paramNodes = xmlDoc.SelectNodes("//param/value/string");
            if (paramNodes.Count > 0)
            {
                XmlNode lastParamNode = paramNodes[paramNodes.Count - 1];
                return lastParamNode.InnerText;
            }
            return null;
        }
        public string GetResponseValue(XmlDocument xmlDoc)
        {
            XmlNodeList paramNodes = xmlDoc.SelectNodes("//param/value/boolean");
            if (paramNodes.Count > 0)
            {
                XmlNode lastParamNode = paramNodes[paramNodes.Count - 1];
                return lastParamNode.InnerText;
            }
            return null;
        }
        public string GetResponseString(XmlDocument xmlDoc)
        {
            XmlNodeList paramNodes = xmlDoc.SelectNodes("//param/value/string");
            if (paramNodes.Count > 0)
            {
                XmlNode lastParamNode = paramNodes[paramNodes.Count - 1];
                return lastParamNode.InnerText;
            }
            return null;
        }

        public List<User> GetUserStatus()
        {
            return listUsers;

        }
        //private class UsersWrapper
        //{
        //    public List<User> Users { get; set; }
        //}

        public void Dispose()
        {
            Unsubscribe();
        }
    }
}
