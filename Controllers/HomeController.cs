using AutomateAppHost.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Text;
using System.Net.Http;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace AutomateAppHost.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        static HttpClient client = new HttpClient();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        [HttpPost]
        public async void AutoMigrate(IFormCollection form)
        {
          //  ProcessStartInfo startInfo = new ProcessStartInfo();

            string _repository = Convert.ToString(form["txtRepo"]);
            string _repoToken = Convert.ToString(form["txtRepoToken"]);
            string _appSvcName = Convert.ToString(form["txtAppName"]);
            string _resGroupName = Convert.ToString(form["txtRGName"]);
            string _sourceBranch = Convert.ToString(form["txtSourceName"]);

            string _resGroupName1 = Convert.ToString(form["txtRGName"]);
            string _sourceBranch2 = Convert.ToString(form["txtSourceName"]);

            string _repo = Convert.ToString(form["txtRepo"]);
            string _repTok = Convert.ToString(form["txtRepoToken"]);
            string _orgName = Convert.ToString(form["txtOrgName"]);
            string _orgPAT = Convert.ToString(form["txtPAT"]);
            string _orgProj = Convert.ToString(form["txtAzProject"]);
            string _repoName = Convert.ToString(form["txtAzRepo"]);
            var scriptfile="";
            if(string.IsNullOrEmpty(_sourceBranch))
            {
                scriptfile = @"C:\Users\sivasaisambela\Automate_CICDAzure.ps1";
            }
            else
            {
                scriptfile = @"C:\Users\sivasaisambela\Automate_deployinAzure1.ps1";
            }

         

            PowerShell powershell = PowerShell.Create();
            //RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
            //Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration)
            try
            {
                using (Runspace runspace = RunspaceFactory.CreateRunspace())
                {
                    runspace.Open();
                    //RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);
                    //scriptInvoker.Invoke("Set-ExecutionPolicy Unrestricted");
                    powershell.Runspace = runspace;
                    //powershell.Commands.AddScript("Add-PsSnapin Microsoft.SharePoint.PowerShell");
                    System.IO.StreamReader sr = new System.IO.StreamReader(scriptfile);
                    StringBuilder st = new StringBuilder();
                    st.Append(sr.ReadToEnd());
                    st.Replace("stry", _repository);
                    st.Replace("isat", _repoToken);
                    st.Replace("svcname", _appSvcName);
                    st.Replace("resgroup", _resGroupName);
                    st.Replace("sourcebrnch", _sourceBranch);
                 
                    st.Replace("azorg", string.Concat("https://dev.azure.com/" + _orgName));
                    st.Replace("aztoken", _orgPAT);
                    st.Replace("azrepon", _repoName);
                    st.Replace("azprjc", _orgProj);

                    powershell.AddScript(st.ToString());

                    string extendCommand = "az repos create --name "+ _repoName + " --org "+ string.Concat("https://dev.azure.com/" + _orgName)+ " --project "+ _orgProj;

                    ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c" + extendCommand)
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    Process proc = new Process
                    {
                        StartInfo = procStartInfo
                    };

                    proc.Start();
                    string cliResult = await proc.StandardOutput.ReadToEndAsync().ConfigureAwait(false);

                    //powershell.AddParameter("gitrepo", _repository);
                    //powershell.AddParameter("gittoken", _repoToken);
                    //powershell.AddParameter("webappname", _appSvcName);
                    //powershell.AddParameter("rgName", _resGroupName);
                    //powershell.AddParameter("branch", _sourceBranch);
                    //    powershell.Commands = command;
                    //powershell.AddCommand("Out-String");
                    var results = powershell.Invoke();
                    if (powershell.Streams.Error.Count > 0)
                    {
                        ViewBag.Result = "Failed to Host your application in Azure.";
                    }
                    else
                    {
                        ViewBag.Result = "Successfully your application is hosted in Azure App Service.";
                    }

                }
            }
            catch(Exception ex)
            {

            }
            
               // return View();
            //}
        }

      


        public IActionResult Index()
        {
            return View("Automate");
        }
        private static void RunPostAsync()
        {

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            Inputs inputs = new Inputs();

            inputs.Password = "tp7qrowvyro2yubzzgartfpgg4q3w53jbthwism5a7cxaxm5bpqq";
            //inputs.AppVersion = "apv";
            //inputs.AppComments = "apc";
            inputs.UserName = "SivaSaiSambela@godevsuite149.onmicrosoft.com";
            //inputs.AppKey = "apk";

            Root rt = new Root();
            rt.folder = "";
            rt.name = "pipeline" + Convert.ToString(DateTime.Now.ToString("yyyy-MM-ddTHH:mm"));
            rt.configuration = new Configuration();
            rt.configuration.type = "yaml";
            rt.configuration.path = "/azure-pipelines.yml";
            rt.configuration.repository = new Repository();

         
         //   rt.configuration.repository.name=""

            var res = client.PostAsync("https://baseuriplus", new StringContent(JsonConvert.SerializeObject(inputs)));

            try
            {
                res.Result.EnsureSuccessStatusCode();

                Console.WriteLine("Response " + res.Result.Content.ReadAsStringAsync().Result + Environment.NewLine);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + res + " Error " +
                ex.ToString());
            }

            Console.WriteLine("Response: {0}", res);
        }

        public string GetRepositoryId()
        {
            string url = "https://dev.azure.com/$ORGANIZATION/$PROJECT/_apis/git/repositories?api-version=6.0";
            HttpMessageHandler handler = new HttpClientHandler()
            {
            };

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(url),
                Timeout = new TimeSpan(0, 2, 0)
            };

            httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

            //This is the key section you were missing    
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("SivaSaiSambela@godevsuite149.onmicrosoft.com:tp7qrowvyro2yubzzgartfpgg4q3w53jbthwism5a7cxaxm5bpqq");
            string val = System.Convert.ToBase64String(plainTextBytes);
          
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);

            HttpResponseMessage response = httpClient.GetAsync(url).Result;
            string content = string.Empty;

            using (StreamReader stream = new StreamReader(response.Content.ReadAsStreamAsync().Result, System.Text.Encoding.GetEncoding(val)))
            {
                content = stream.ReadToEnd();
            }
            return url;
        }
        public class Inputs
        {
            public string Password;
          //  public string name;
         //   public string AppComments;
            public string UserName;
          //  public string AppKey;
        }
        public class Configuration
        {
            public string type { get; set; }
            public string path { get; set; }
            public Repository repository { get; set; }
        }

        public class Repository
        {
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

        public class Root
        {
            public string folder { get; set; }
            public string name { get; set; }
            public Configuration configuration { get; set; }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
