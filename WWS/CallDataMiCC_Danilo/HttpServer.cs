using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CallDataMiCC_Danilo
{
    class HttpServer : Log
    {
        private readonly int _maxThreads;
        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly ManualResetEvent _stop, _idle;
        private readonly Semaphore _busy;
        private Micc _micc;
        //private SqlClient _sqlClient;
        //private Log _log;

        public HttpServer(int maxThreads)
        {
            _maxThreads = maxThreads;
            _stop = new ManualResetEvent(false);
            _idle = new ManualResetEvent(false);
            _busy = new Semaphore(maxThreads, maxThreads);
            _listener = new HttpListener();
            _listenerThread = new Thread(HandleRequests);
            _micc = new Micc();
            //_sqlClient = new SqlClient();
            //_log = new Log();
        }

        public void Start(int port, string iplocal)
        {
            //Console.WriteLine("Iniciando...1");
            _listener.Prefixes.Add(String.Format(@"http://{1}:{0}/", port, iplocal));
            //Console.WriteLine("Iniciando...2");
            _listener.Start();
            //Console.WriteLine("Iniciando...3");
            _listenerThread.Start();
            //Console.WriteLine("Iniciando...4");
            _micc.Connect();
            //_sqlClient.Open();
            //Console.WriteLine("Iniciando...");
        }

        public void Dispose()
        { Stop(); }

        public void Stop()
        {
            _stop.Set();
            _listenerThread.Join();
            _idle.Reset();

            //aquire and release the semaphore to see if anyone is running, wait for idle if they are.
            _busy.WaitOne();
            if (_maxThreads != 1 + _busy.Release())
                _idle.WaitOne();

            _listener.Stop();
            _micc.Disconnect();
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
                string type = "";
                string card = "";
                string mode = "";
                switch (metodo[0])
                {
                    case "/api/pabx/makecall":
                        body = new StreamReader(context.Request.InputStream).ReadToEnd();
                        dynamic respostaMakecall = JsonConvert.DeserializeObject(body);
                        recId = Convert.ToInt32(respostaMakecall.RecID);
                        string callNumber = respostaMakecall.CallNumber;
                        try
                        {
                            type = Convert.ToString(respostaMakecall.FoneType);
                        }
                        catch
                        {
                            type = "";
                        }
                        try
                        {
                            card = respostaMakecall.Card;
                        }
                        catch
                        {
                            card = "";
                        }
                        try
                        {
                            mode = respostaMakecall.Mode;
                        }
                        catch
                        {
                            mode = "";
                        }
                        //_micc.Log("ListenerCallbak", "/makecall " + qry[0] + "=" + qry[1], "INFO");
                        //Console.WriteLine("{0} {1}", qry[0], qry[1]);
                        //int resultMakecall = _micc.MakeCall(Convert.ToInt32(qry[0]), qry[1]);
                        result = _micc.MakeCall(recId, callNumber, card, type, mode);
                        if (result == 0)
                        {
                            //reason = _sqlClient.AgentReason(recId);
                            reason = _micc.AgentReason(recId);
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
                        result = _micc.HangupCall(recId);
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

