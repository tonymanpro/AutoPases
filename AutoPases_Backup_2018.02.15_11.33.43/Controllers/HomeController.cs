using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;

namespace AutoPases.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public void moveFiles()
        {
            string sourcePath = @"C:\Users\Public\TestFolder";
            string targetPath = @"C:\Users\Public\TestFolder\SubDir";

            // Use Path class to manipulate file and directory paths.
            string sourceFile = System.IO.Path.Combine(sourcePath);
            string destFile = System.IO.Path.Combine(targetPath);

            // To copy a file to another location and 
            // overwrite the destination file if it already exists.
            //System.IO.File.Copy(sourceFile, destFile, true);

            if (System.IO.Directory.Exists(sourcePath))
            {
                string[] files = System.IO.Directory.GetFiles(sourcePath);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    string fileName = System.IO.Path.GetFileName(s);
                    destFile = System.IO.Path.Combine(targetPath, fileName);
                    System.IO.File.Copy(s, destFile, true);
                }
            }
            else
            {
                Console.WriteLine("Source path does not exist!");
            }
        }

        public void StopIIS(string webSiteName)
        {
            var server = new ServerManager();
            var site = server.Sites.FirstOrDefault(s => s.Name == webSiteName);
            if (site != null)
            {
                //stop the site...
                site.Stop();
                if (site.State == ObjectState.Stopped)
                {
                    //do deployment tasks...
                }
                else
                {
                    throw new InvalidOperationException("Could not stop website!");
                }
                //restart the site...
                site.Start();
            }
            else
            {
                throw new InvalidOperationException("Could not find website!");
            }
        }

        public static void ExecuteAsynchronously(string txtInvoke)
        {
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                // this script has a sleep in it to simulate a long running script
                PowerShellInstance.AddScript(txtInvoke);

                // prepare a new collection to store output stream objects
                PSDataCollection<PSObject> outputCollection = new PSDataCollection<PSObject>();

                // begin invoke execution on the pipeline
                // use this overload to specify an output stream buffer
                IAsyncResult result = PowerShellInstance.BeginInvoke<PSObject, PSObject>(null, outputCollection);

                // do something else until execution has completed.
                // this could be sleep/wait, or perhaps some other work
                while (result.IsCompleted == false)
                {
                    Console.WriteLine("Waiting for pipeline to finish...");
                    Thread.Sleep(1000);                    
                }

                Console.WriteLine("Execution has stopped. The pipeline state: " + PowerShellInstance.InvocationStateInfo.State);

                foreach (PSObject outputItem in outputCollection)
                {
                    //TODO: handle/process the output items if required
                    Console.WriteLine(outputItem.BaseObject.ToString());
                }
            }
        }
        
    }
}