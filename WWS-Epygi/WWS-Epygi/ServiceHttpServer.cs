using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace WWS_Epygi
{
    partial class ServiceHttpServer : ServiceBase
    {
        
        public ServiceHttpServer()
        {
            InitializeComponent();
        }
        protected int _maxThreads =1;
        protected HttpListener _listener;
        protected Thread _listenerThread;
        protected ManualResetEvent _stop, _idle;
        protected Semaphore _busy;
        protected Log _log;
        //protected QXEpygiClient _qx;
        protected override void OnStart(string[] args)
        {

            // TODO: Adicione aqui o código para iniciar seu serviço.
            _log = new Log();
            _log.Send("OnStart HTTP", "Starting", "INFO");
            //_qx = new QXEpygiClient();
            _maxThreads = Convert.ToInt32(ConfigurationManager.AppSettings["MAXTHREADS"]);
            _listener = new HttpListener();
            _stop = new ManualResetEvent(false);
            _idle = new ManualResetEvent(false);
            _busy = new Semaphore(_maxThreads, _maxThreads);
            _listener = new HttpListener();
            _listenerThread = new Thread(HandleRequests);

            
            bool _startHttp = Convert.ToBoolean(ConfigurationManager.AppSettings["STARTHTTPSERVER"]);
            if (_startHttp)
            {
                int _portHttp = Convert.ToInt32(ConfigurationManager.AppSettings["PORTAHTTP"]);

                string _localIP = ConfigurationManager.AppSettings["IPLOCAL"];
                try
                {
                    //Console.WriteLine("Iniciando...1");
                    _listener.Prefixes.Add(String.Format(@"https://{1}:{0}/", _portHttp, _localIP));
                    //Console.WriteLine("Iniciando...2");
                    _listener.Start();
                    //Console.WriteLine("Iniciando...3");
                    _listenerThread.Start();
                    //Console.WriteLine("Iniciando...4");
                   
                    StartQX();
                }
                catch (Exception e)
                {
                    _log.Send("OnStart HTTP", e.Message, "ERRO");
                }
            }



        }

        protected override void OnStop()
        {
            // TODO: Adicione aqui o código para realizar qualquer desmontagem necessária para interromper seu serviço.
            timer.Dispose();
            Unsubscribe();
            _stop.Set();
            _listenerThread.Join();
            _idle.Reset();

            //aquire and release the semaphore to see if anyone is running, wait for idle if they are.
            _busy.WaitOne();
            if (_maxThreads != 1 + _busy.Release())
                _idle.WaitOne();

            _listener.Stop();

        }
        private void HandleRequests()
        {
            while (_listener.IsListening)
            {
                var context = _listener.BeginGetContext(ListenerCallback, null);

                if (0 == WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle }))
                    return;
            }
        }

        private void ListenerCallback(IAsyncResult ar)
        {
            //Console.WriteLine("ListenerCallbak chamando...");
            _busy.WaitOne();
            try
            {
                HttpListenerContext context;
                try
                {
                    context = _listener.EndGetContext(ar);
                }
                catch (HttpListenerException ex)
                {
                    _log.Send("ListenerCallback:HttpListenerContext", ex.Message, "ERRO");
                    return;
                }

                if (_stop.WaitOne(0, false))
                    return;

                //Console.WriteLine("{0} {1}", context.Request.HttpMethod, context.Request.RawUrl);
                context.Response.SendChunked = true;
                string url = context.Request.RawUrl;
                //var qry = context.Request.QueryString;

                string[] metodo = url.Split('?');
                string body;
                int recId = 0;
                int result = 0;
                switch (context.Request.HttpMethod)
                {
                    case "POST":
                        switch (metodo[0])
                        {
                            case "/api/pabx/prslistrequest":
                                try
                                {
                                    body = new StreamReader(context.Request.InputStream).ReadToEnd();
                                    // Converter o corpo da requisição para objeto dynamic
                                    dynamic requestBodyObject = JsonConvert.DeserializeObject(body);
                                    _log.Send("ListenerCallback:/api/pabx/prslistrequest body ", body, "INFO");
                                    // Criar uma lista para armazenar os objetos dynamic encontrados
                                    List<dynamic> resultPresence = new List<dynamic>();
                                    //Cria a lista para armazenaro status dos usuários do QX
                                    List<User> listUsers = GetUserStatus();
                                    // Verificar cada usuário recebido no corpo da requisição
                                    JArray usersArray = requestBodyObject.users;
                                    List<string> usernames = usersArray.ToObject<List<string>>();
                                    foreach (string username in usernames)
                                    {
                                        // Procurar um usuário com o mesmo valor de Sip na lista de objetos User
                                        User user = listUsers.Find(u => u.Sip == username);
                                        if (user != null)
                                        {
                                            // Adicionar um novo objeto dynamic com o par "Nome: Status" na lista de resultados
                                            //resultPresence.Add(new {[username] = user.Status });
                                            // Adicionar um novo objeto dynamic com o par "Nome: Status" na lista de resultados
                                            resultPresence.Add(new Dictionary<string, string> { { username, user.Status } });
                                        }
                                    }

                                    /*
                                    foreach (string username in requestBodyObject.users)
                                    {
                                        // Procurar um usuário com o mesmo valor de Sip na lista de objetos User
                                        User user = listUsers.Find(u => u.Sip == username);
                                        if (user != null)
                                        {
                                            // Adicionar um novo objeto dynamic com o par "Nome: Status" na lista de resultados
                                            //resultPresence.Add(new {[username] = user.Status });
                                            // Adicionar um novo objeto dynamic com o par "Nome: Status" na lista de resultados
                                            resultPresence.Add(new Dictionary<string, string> { { username, user.Status } });
                                        }
                                    }
                                    */
                                    // Converter a lista de resultados para JSON
                                    //string resultJson = JsonConvert.SerializeObject(new { users = result }, Formatting.Indented);
                                    //Console.WriteLine(resultJson);
                                    var serializer = new JavaScriptSerializer();
                                    var serializedResult = serializer.Serialize(resultPresence);
                                    context.Response.AddHeader("Access-Control-Allow-Origin", "*");// Especifique os cabeçalhos suportados
                                    byte[] byteArray = Encoding.UTF8.GetBytes(serializedResult);
                                    context.Response.StatusCode = 200;
                                    context.Response.ContentType = "application/json";
                                    context.Response.ContentLength64 = byteArray.Length;
                                    Stream dataStream = context.Response.OutputStream;
                                    dataStream.Write(byteArray, 0, byteArray.Length);
                                    dataStream.Close();
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    _log.Send("ListenerCallback:/api/pabx/prslistrequest", ex.Message, "ERRO");
                                }
                                break;
                            case "/api/pabx/makecall":
                                body = new StreamReader(context.Request.InputStream).ReadToEnd();
                                dynamic respostaMakecall = JsonConvert.DeserializeObject(body);
                                recId = Convert.ToInt32(respostaMakecall.RecID);
                                string callNumber = respostaMakecall.CallNumber;

                                //_micc.Log("ListenerCallbak", "/makecall " + qry[0] + "=" + qry[1], "INFO");
                                //Console.WriteLine("{0} {1}", qry[0], qry[1]);
                                //int resultMakecall = _micc.MakeCall(Convert.ToInt32(qry[0]), qry[1]);
                                //result = MakeCall(recId, callNumber);
                                if (result == 0)
                                {
                                    //reason = _sqlClient.AgentReason(recId);
                                    //reason = AgentReason(recId);
                                }
                                break;
                            case "/api/pabx/hangup":
                                body = new StreamReader(context.Request.InputStream).ReadToEnd();
                                dynamic respostaHangup = JsonConvert.DeserializeObject(body);
                                recId = Convert.ToInt32(respostaHangup.RecID);
                                //callId = Convert.ToInt32(respostaHangup.CallID);
                                //_micc.Log("ListenerCallbak", "/hangup "+ qry[0]+"="+qry[1], "INFO");
                                //Console.WriteLine("{0} {1}", qry[0], qry[1]);
                                //_micc.HangupCall(Convert.ToInt32(qry[0]), Convert.ToInt32(qry[1]));
                                //result = HangupCall(recId);
                                break;
                        }
                        break;
                    case "OPTIONS": // Lidar com a solicitação OPTIONS
                        context.Response.StatusCode = 200;
                        context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                        context.Response.AddHeader("Access-Control-Allow-Methods", "POST, OPTIONS"); // Especifique os métodos suportados
                        context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization"); // Especifique os cabeçalhos suportados
                        context.Response.AddHeader("Access-Control-Max-Age", "86400"); // Especifique o tempo máximo de cache para a resposta OPTIONS (em segundos)
                        context.Response.Close();
                        break;
                    default:
                        context.Response.StatusCode = 404;
                        context.Response.Close();
                        break;
                }
            }
            catch(Exception ex) 
            {
                _log.Send("ListenerCallback:", ex.Message, "ERRO"); 
            }
            finally
            {
                if (_maxThreads == 1 + _busy.Release())
                    _idle.Set();
            }
        }



        #region VariablesQX
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
        int _startRetries = 1;
        string filePath = ConfigurationManager.AppSettings["USERSJSONPATCH"];
        SimpleTcpClient client;
        Timer timer;
        //Log _log = new Log();

        #endregion
        public void StartQX()
        {
            // Leitura do conteúdo do arquivo
            string jsonContent = File.ReadAllText(filePath);
            // Desserialização do JSON
            listUsers = JsonConvert.DeserializeObject<List<User>>(jsonContent);

            Thread t = new Thread(new ThreadStart(ConnectQX));
            t.IsBackground = true;
            _log.Send("QXEpygiClient:Start", "StartThread", "INFO");
            t.Start();
        }
        void ConnectQX()
        {
            string host = server + ":" + port;
            _log.Send("QXEpygiClient:Connect", "TO " + host, "INFO");
            // instantiate
            client = new SimpleTcpClient(host);
            client.Events.Connected += Events_Connected;
            client.Events.DataReceived += Events_DataReceived;
            client.Events.Disconnected += Events_Disconected;
            try
            {
                client.ConnectWithRetries(10000);
                // Cria um temporizador com intervalo de 60 segundos (60000 milissegundos)
                timer = new Timer(SystemInfo, null, 0, 60000);

            }
            catch (System.TimeoutException ex)
            {
                _log.Send("QXEpygiClient:Connect", "ERRO " + ex.Message, "ERRO");
            }


        }

        private void Events_Disconected(object sender, ConnectionEventArgs e)
        {
            _log.Send("QXEpygiClient:Events_Disconected", "", "INFO");

            // Loop para imprimir os parâmetros Num e Password
            foreach (User user in listUsers)
            {
                if (user.ID != null && user.State == "auth")
                {
                    _log.Send("QXEpygiClient:Events_Disconected", "Delete Authenticate: Num: "+user.Num,"INFO");
                    user.ID = null;
                    user.State = null;
                    break;
                }
            }

            _startRetries += 1;
            if (_startRetries < 5)
            {
                _log.Send("QXEpygiClient:Events_Disconected", "Reconnectando...", "INFO");
                ConnectQX();
            }
            else
            {
                _log.Send("QXEpygiClient:Events_Disconected", "Todas as tentativas foram realizadas", "ERRO");
            }
        }

        private void Events_DataReceived(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Data.Array, 0, e.Data.Count);
            //Console.WriteLine(data);

            //try
            //{
            string[] stringSeparators = new string[] { "\r\n\r\n" };
            string[] xmlStrings;
            xmlStrings = data.Split(stringSeparators, StringSplitOptions.None);
            _log.Send("Events_DataReceived", data, "INFO");
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
                        //int index = listUsers.FindIndex(listUsers => listUsers.ID.Contains(id));
                        //if (index != -1)
                        //{
                        //    foreach (User user in listUsers)
                        //    {
                        //        if (user.ID == id)
                        //        {
                        //            user.Status = status;
                        //            _log.Send("Services.CallProcessingService.prslistarrived", "Services.CallProcessingService.prslistarrived: Found Num: " + user.Num + ", ID: " + user.ID + ", Status: " + user.Status, "INFO");

                        //        }


                        //    }

                        //}

                        foreach (User user in listUsers)
                        {
                            if (user.ID == id)
                            {
                                user.Status = status;
                                _log.Send("Services.CallProcessingService.prslistarrived", "Services.CallProcessingService.prslistarrived: Found Num: " + user.Num + ", ID: " + user.ID + ", Status: " + user.Status, "INFO");

                            }


                        }
                        //Authnticate users from list
                        foreach (User user in listUsers)
                        {
                            if (user.ID == null && user.State == null)
                            {
                                _log.Send("Services.CallProcessingService.prslistarrived", $"Authenticate New User: Num: {user.Num}, Password: {user.Password}","INFO");
                                Authenticate(user.Num, user.Password);
                                user.State = "sent";
                                break;
                            }

                        }
                        //ParsePresenceXml(Convert.ToString(xmltest));
                    }

                }
                else if (elemlistmethodResponse.Count > 0)
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
                                _log.Send("Services.CallProcessingService.AuthenticateReponse", "Authenticated: Found Num: " + user.Num + ", ID: " + user.ID, "INFO");
                                user.State = "auth";
                                //Console.WriteLine("Authenticated, Subscribe...");
                                //Unsubscribe("1844674407");
                                Subscribe(rand, user.Num);
                            }


                        }
                        // Loop para imprimir os parâmetros Num e Password
                        //foreach (User user in listUsers)
                        //{
                        //    if (user.ID == null && user.State == null)
                        //    {
                        //        _log.Send("Services.CallProcessingService.prslistarrived", "Authenticate New User: Num " + user.Num + ", Password: " + user.Password, "INFO");
                        //        //Console.WriteLine($"Authenticate New User: Num: {user.Num}, Password: {user.Password}");
                        //        Authenticate(user.Num, user.Password);
                        //        user.State = "sent";
                        //        break;
                        //    }

                        //}

                    }
                    else if (responseValue == "0")
                    {
                        foreach (User user in listUsers)
                        {
                            if (user.ID == null && user.State == "sent")
                            {
                                string responseString = GetResponseString(xmltest);
                                _log.Send("Services.CallProcessingService.AuthenticateReponse", "ERRO Authenticate: Found Num: " + user.Num + ", ERRO: " + responseString, "ERRO");
                                user.State = "faill";
                            }
                        }
                        //// Loop para imprimir os parâmetros Num e Password
                        //foreach (User user in listUsers)
                        //{
                        //    if (user.ID == null && user.State == null)
                        //    {
                        //        _log.Send("Services.CallProcessingService.prslistarrived", "Authenticate New User: Num: " + user.Num + ", Password: " + user.Password, "INFO");
                        //        Authenticate(user.Num, user.Password);
                        //        user.State = "sent";
                        //        break;
                        //    }

                        //}

                    }
                    else
                    {
                        string responseString = GetResponseString(xmltest);
                        _log.Send("Services.SystemService.getsysteminfo", "Info: " + responseString, "INFO");
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
            _log.Send("Events_Connected", "Connected ", "INFO");

            // Loop para imprimir os parâmetros Num e Password
            foreach (User user in listUsers)
            {
                if (user.ID == null && user.State == null)
                {
                    _log.Send("Events_Connected", "First Authenticate: Num: " + user.Num + ", Password: " + user.Password, "INFO");
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
            _log.Send("Services.Subscribe", "Subscribing.. Num: " + num + ", ID: " + id, "INFO");

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
        public void SystemInfo(object state)
        {
            // Cria uma instância da classe Random
            Random random = new Random();
            // Gera um número aleatório entre 0 e 100
            string rand = Convert.ToString(random.Next(0, 9999999));
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("methodCall",
                                             new XElement("methodName", "Services.SystemService.getsysteminfo"),
                                             new XElement("params",
                                             new XElement("param",
                                             new XElement("value",
                                             new XElement("string", rand))))), CrLf);
            var wr = new StringWriter();
            xml.Save(wr);
            //Console.Write(wr.ToString());
            //SendText(wr.ToString());
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(xml.ToString());
            client.Send(data);
        }
        public void Unsubscribe()
        {
            foreach (User user in listUsers)
            {
                if (user.ID != null && user.State == "auth")
                {
                    _log.Send("Unsubscribe", "Unsubscribe: Found Num: " + user.Num + ", Password: " + user.Password, "INFO");
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
    }
}
