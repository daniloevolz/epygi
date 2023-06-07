using Newtonsoft.Json;
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
        protected QXEpygiClient _qx;
        protected override void OnStart(string[] args)
        {

            // TODO: Adicione aqui o código para iniciar seu serviço.
            _log = new Log();
            _log.Send("OnStart HTTP", "Starting", "INFO");
            _qx = new QXEpygiClient();
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
                    _listener.Prefixes.Add(String.Format(@"http://{1}:{0}/", _portHttp, _localIP));
                    //Console.WriteLine("Iniciando...2");
                    _listener.Start();
                    //Console.WriteLine("Iniciando...3");
                    _listenerThread.Start();
                    //Console.WriteLine("Iniciando...4");
                   
                    _qx.Start();
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
            _qx.Dispose();
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
                catch (HttpListenerException e)
                {
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
                string reason = "";
                switch (metodo[0])
                {
                    case "/api/pabx/prslistrequest":
                        body = new StreamReader(context.Request.InputStream).ReadToEnd();
                        // Converter o corpo da requisição para objeto dynamic
                        dynamic requestBodyObject = JsonConvert.DeserializeObject(body);

                        // Criar uma lista para armazenar os objetos dynamic encontrados
                        List<dynamic> resultPresence = new List<dynamic>();

                        //Cria a lista para armazenaro status dos usuários do QX
                        List<User> listUsers = _qx.GetUserStatus();
                        // Verificar cada usuário recebido no corpo da requisição
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
                        // Converter a lista de resultados para JSON
                        string resultJson = JsonConvert.SerializeObject(new { users = result }, Formatting.Indented);
                        Console.WriteLine(resultJson);
                        var serializer = new JavaScriptSerializer();
                        var serializedResult = serializer.Serialize(resultPresence);

                        byte[] byteArray = Encoding.UTF8.GetBytes(serializedResult);
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "application/json";
                        context.Response.ContentLength64 = byteArray.Length;
                        Stream dataStream = context.Response.OutputStream;
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        dataStream.Close();
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
            }
            finally
            {
                if (_maxThreads == 1 + _busy.Release())
                    _idle.Set();
            }
        }
    }
}
