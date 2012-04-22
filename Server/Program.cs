using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main()
        {
            string dir = Environment.CurrentDirectory;
            dir = dir.Replace("\\Server\\bin", "");
            dir += "\\WebApplication1\\";
            WebServer server = new WebServer(new[] {"http://localhost:8000/test/"}, dir);
            server.Start();
            Console.ReadLine();
        }
    }

    internal class WebServer
    {
        readonly HttpListener _listener;
        readonly string _baseFolder;

        public WebServer(IEnumerable<string> uriPrefixs, string baseFolder)
        {
            _listener = new HttpListener();
            foreach (string prefix in uriPrefixs)
            {
                _listener.Prefixes.Add(prefix);
            }
            _baseFolder = baseFolder;
        }

        public void Start()
        {
            _listener.Start();
            while (true)
                try
                {
                    HttpListenerContext request = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(ProcessRequest, request);
                }
                catch (HttpListenerException) { break; }
                catch (InvalidOperationException) { break; }
        }

        public void Stop() { _listener.Stop(); }

        void ProcessRequest(object listenerContext)
        {
            try
            {
                HttpListenerContext context = (HttpListenerContext)listenerContext;
                string filename = Path.GetFileName(context.Request.RawUrl);
                string path = Path.Combine(_baseFolder, filename);
                byte[] msg;
                if (!File.Exists(path))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    msg = Encoding.UTF8.GetBytes("Sorry, that page does not exist");
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    msg = File.ReadAllBytes(path);
                }
                context.Response.ContentLength64 = msg.Length;
                using (Stream s = context.Response.OutputStream)
                    s.Write(msg, 0, msg.Length);
            }
            catch (Exception ex) { Console.WriteLine("Request error: " + ex); }
        }
	
    }
}
