using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace KinectServer
{
    public class WebService
    {
        private ManualResetEvent resetEvent = new ManualResetEvent(false);

       

        public void Start()
        {
            Task.Run(() =>
            {
                try
                {
                    Debugger.Log(0, "WebServer", "Starting the web server.");
                    var config = new HttpSelfHostConfiguration("http://localhost:9999");
                    config.MapHttpAttributeRoutes();
                    config.Routes.MapHttpRoute(
                        "API Default",
                        "api/{controller}/{userId}",
                        new
                        {
                            userId = RouteParameter.Optional
                        });

                    using (HttpSelfHostServer server = new HttpSelfHostServer(config))
                    {
                        server.OpenAsync().Wait();
                        Console.WriteLine("Press Enter to quit.");
                        this.resetEvent.WaitOne();
                    }

                }
                catch (Exception ex)
                {
                    this.resetEvent.Set();
                }
            });
        }

        public void Stop()
        {
            this.resetEvent.Set();
        }
    }
}
