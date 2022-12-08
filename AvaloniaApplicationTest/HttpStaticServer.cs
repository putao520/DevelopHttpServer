using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplicationTest {
    public class HttpStaticServer {
        private string? currentFolder = Directory.GetCurrentDirectory();
        private IWebHost? server;
        public HttpStaticServer(string? currentFolder) {
            this.currentFolder = currentFolder;
        }
        public void listen(int port) {
            server = WebHost.CreateDefaultBuilder()
              .UseKestrel()
              .UseWebRoot(currentFolder)
              .UseIISIntegration()
              .UseUrls("http://*:" + Convert.ToString(port))
              .UseStartup<StaticFileStartup>()
              .Build();
            Task.Run(() => server.RunAsync());
        }

        public void stop() {
            server?.StopAsync();
        }
        
        public bool isRunning() {
            return server != null;
        }
    }
}
