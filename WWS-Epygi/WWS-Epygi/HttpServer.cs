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
    partial class HttpServer : ServiceBase
    {
        private int _maxThreads;
        private HttpListener _listener;
        private Thread _listenerThread;
        private ManualResetEvent _stop, _idle;
        private Semaphore _busy;
        private Log _log;

        public HttpServer()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            // TODO: Adicione aqui o código para iniciar seu serviço.
            _maxThreads = Convert.ToInt32(ConfigurationManager.AppSettings["MAXTHREADS"]);
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
                        dynamic respostaPresence = JsonConvert.DeserializeObject(body);
                        recId = Convert.ToInt32(respostaPresence.RecID);

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
                var objjson = new
                {
                    RecID = Convert.ToString(recId),
                    Status = result,
                    Reason = reason
                };
                var serializer = new JavaScriptSerializer();
                var serializedResult = serializer.Serialize(objjson);

                byte[] byteArray = Encoding.UTF8.GetBytes(serializedResult);
                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = byteArray.Length;
                Stream dataStream = context.Response.OutputStream;
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                //using (TextWriter tw = new StreamWriter(context.Response.OutputStream))
                //{


                //tw.WriteLine("<html><body><h1>Multithread do Danilo</h1>");
                //    for (int i = 0; i < qry.Count; i++)
                //    {
                //        tw.WriteLine("<p>{0} @ {1}</p>", qry[i], DateTime.Now);
                //        tw.Flush();
                //Thread.Sleep(1000);
                //    }
                //    tw.WriteLine("</body></html>");
                //}
            }
            finally
            {
                if (_maxThreads == 1 + _busy.Release())
                    _idle.Set();
            }
        }
    }
}
