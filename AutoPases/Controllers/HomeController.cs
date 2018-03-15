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
using System.Data.SqlClient;
using AutoPases.Integracion;
using Monibyte.Arquitectura.Comun.Nucleo.Sesion;
using AutoPases.Models;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using System.Security;
using Monibyte.Arquitectura.Comun.Nucleo.Utilitarios;
using Monibyte.Arquitectura.Web.Nucleo.Controlador;
using System.Security.Permissions;

namespace AutoPases.Controllers
{
    public class HomeController : ControladorBase
    {
        public ActionResult Index()
        {
            var connString = AppConfiguracion.Instance.ConnectionString;
            var proyectos = new List<ModProyectosActivos>();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                var statement = ReadXml.GetSqlStatement("SqlProyectosActivos");

                var commd = new SqlCommand(statement, conn);
                SqlDataReader data = commd.ExecuteReader();
                while (data.Read())
                {
                    var archivo = data.CreateItemFromReader<ModProyectosActivos>();
                    proyectos.Add(archivo);
                }
            }
            InfoSesion.IncluirSesion("ProyectosActivos", proyectos);
            return View(proyectos);
        }

        public ActionResult GetMobileSwitch(string projectName)
        {
            var projects = InfoSesion.ObtenerSesion<List<ModProyectosActivos>>("ProyectosActivos");
            var model = projects.Where(x => x.Entidad == projectName);
            return PartialView("_MobileButton", model);
        }

        public ActionResult GetTreeView(string viewID)
        {
            var projects = InfoSesion.ObtenerSesion<List<ModProyectosActivos>>("ProyectosActivos");
            var model = projects.Where(x => x.Proyecto == viewID);
            return PartialView("_Treeview", model);
        }

        public async Task<JsonResult> GCloudDownloadAsync(string command)
        {
            var response = await ExecuteAsynchronously(command);
            return Json(new { result = true, response });
        }
        public async Task<JsonResult> StopSiteAsync(string pageSite)
        {
            var result = await IISManager(pageSite, true);
            return Json(result);
        }
        public async Task<JsonResult> CreateBackUpAsync(string sourcePath, string targetPath, string projectName)
        {
            try
            {
                await moveFiles(sourcePath, targetPath, projectName);
                return Json(new { result = true });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, response = ex.Message });
            }
        }
        public async Task<JsonResult> PublishSiteAsync(string publishFiles, string solution,
                                      string sourcePath, string targetPath, string backUp, string siteName)
        {
            try
            {
                if (string.IsNullOrEmpty(publishFiles))
                {
                    await SimpleMovFiles(sourcePath, targetPath);
                    return Json(new { result = true });
                }
                else
                {
                    var response = await PublishProject(solution, sourcePath);
                    await moveFilesBase(backUp, targetPath, siteName);
                    return Json(new { result = true, response });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = false, response = ex.Message });
            }
        }
        public async Task<JsonResult> RollBackAsync(string sourcePath, string targetPath, string siteName)
        {
            try
            {
                await moveFiles(sourcePath, targetPath, siteName, true);
                await StartSiteAsync();
                return Json(new { result = true });

            }
            catch (Exception ex)
            {
                return Json(new { result = false, response = ex.Message });
            }
        }
        public async Task<JsonResult> StartSiteAsync()
        {
            try
            {
                var result = await ExecuteAsynchronously("iisreset");
                //var result = await InvokeCMD(null, true);
                return Json(new { result = true, response = result });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, response = ex.Message + ex.StackTrace });
            }
        }


        public async Task SimpleMovFiles(string sourcePath,
            string targetPath)
        {
            // Use Path class to manipulate file and directory paths.
            string sourceFile = Path.Combine(sourcePath);
            string routePath = Path.Combine(targetPath);

            // To copy a file to another location and 
            // overwrite the destination file if it already exists.
            //File.Copy(sourceFile, destFile, true);

            bool isExists = Directory.Exists(routePath);
            if (!isExists)
            {
                Directory.CreateDirectory(routePath);
            }

            if (Directory.Exists(sourcePath))
            {
                string[] files = Directory.GetFiles(sourceFile);

                // Copy the files and overwrite destination files if they already exist.
                //foreach (string s in files)
                //{
                //    // Use static Path methods to extract only the file name from the path.
                //    string fileName = Path.GetFileName(s);
                //    targetPath = Path.Combine(routePath, fileName);
                //    System.IO.File.Copy(s, targetPath, true);
                //}
                foreach (string dirPath in Directory.GetDirectories(sourceFile, "*",
                        SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(sourceFile, routePath));

                foreach (string newPath in Directory.GetFiles(sourceFile, "*.*",
                        SearchOption.AllDirectories))
                    System.IO.File.Copy(newPath, newPath.Replace(sourceFile, routePath), true);
            }
            else
            {
                Console.WriteLine("Source path does not exist!");
            }
        }

        public async Task moveFiles(string sourcePath,
            string targetPath, string projectName = "", bool isRollBack = false)
        {
            // Use Path class to manipulate file and directory paths.
            string sourceFile = Path.Combine(isRollBack ? string.Format("{0}{1}{2}",
                sourcePath, projectName, DateTime.Now.Date.ToString("yyyyMMdd")) : sourcePath);
            string routePath = Path.Combine(isRollBack ? targetPath : string.Format("{0}{1}{2}\\",
                targetPath, projectName, DateTime.Now.Date.ToString("yyyyMMdd")));

            // To copy a file to another location and 
            // overwrite the destination file if it already exists.
            //File.Copy(sourceFile, destFile, true);

            bool isExists = Directory.Exists(routePath);
            if (!isExists)
            {
                Directory.CreateDirectory(routePath);
            }

            if (Directory.Exists(sourcePath))
            {
                string[] files = Directory.GetFiles(sourceFile);

                // Copy the files and overwrite destination files if they already exist.
                //foreach (string s in files)
                //{
                //    // Use static Path methods to extract only the file name from the path.
                //    string fileName = Path.GetFileName(s);
                //    targetPath = Path.Combine(routePath, fileName);
                //    System.IO.File.Copy(s, targetPath, true);
                //}
                foreach (string dirPath in Directory.GetDirectories(sourceFile, "*",
                        SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(sourceFile, routePath));

                foreach (string newPath in Directory.GetFiles(sourceFile, "*.*",
                        SearchOption.AllDirectories))
                    System.IO.File.Copy(newPath, newPath.Replace(sourceFile, routePath), true);
            }
            else
            {
                Console.WriteLine("Source path does not exist!");
            }
        }

        public async Task moveFilesBase(string sourcePath, string targetPath, string projectName = "")
        {
            // Use Path class to manipulate file and directory paths.
            string sourceFile = Path.Combine(string.Format("{0}{1}{2}",
                sourcePath, projectName, DateTime.Now.Date.ToString("yyyyMMdd")));
            string routePath = Path.Combine(targetPath);

            // To copy a file to another location and 
            // overwrite the destination file if it already exists.
            //File.Copy(sourceFile, destFile, true);

            if (Directory.Exists(sourcePath))
            {
                string[] files = { "configuration.json", "Web.config" };

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    string filepath = Path.Combine(sourceFile, s);
                    routePath = Path.Combine(targetPath, s);
                    if (System.IO.File.Exists(filepath) ? true : false)
                    {
                        System.IO.File.Copy(filepath, routePath, true);
                    }
                }
            }
            else
            {
                Console.WriteLine("Source path does not exist!");
            }
        }
        public async Task<bool> IISManager(string webSiteName, bool stop)
        {
            var server = new ServerManager();
            var site = server.Sites.FirstOrDefault(s => s.Name == webSiteName);
            ApplicationPoolCollection appPools = server.ApplicationPools;

            if (site != null)
            {
                if (stop)
                {
                    try
                    {
                        foreach (ApplicationPool ap in appPools)
                        {
                            ap.Recycle();
                        }
                        site.Stop();
                        if (site.State == ObjectState.Stopped)
                        {
                            return true;
                        }
                    }
                    catch (Exception es)
                    {

                    }
                }
                else
                {
                    site.Start();
                    if (site.State == ObjectState.Starting ||
                        site.State == ObjectState.Started)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public async Task<string> ExecuteAsynchronously(string txtInvoke)
        {
            string wanted_path = Directory.GetCurrentDirectory();

            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                // this script has a sleep in it to simulate a long running script
                PowerShellInstance.AddScript(txtInvoke);
                // this script has a sleep in it to simulate a long running script
                //InvokeCMD(txtInvoke);

                // prepare a new collection to store output stream objects
                PSDataCollection<PSObject> outputCollection = new PSDataCollection<PSObject>();

                // begin invoke execution on the pipeline
                // use this overload to specify an output stream buffer
                IAsyncResult result = PowerShellInstance.BeginInvoke<PSObject, PSObject>(null, outputCollection);

                // do something else until execution has completed.
                // this could be sleep/wait, or perhaps some other work
                while (result.IsCompleted == false)
                {
                    Thread.Sleep(1000);
                }

                var message = "";
                foreach (PSObject outputItem in outputCollection)
                {
                    //TODO: handle/process the output items if required
                    message += outputItem.BaseObject.ToString();
                }
                return message;
            }
        }

        //[PrincipalPermission(SecurityAction.Demand, Role = @"IMPESAGROUP\ACHAVARRIA")]
        public async Task<string> InvokeCMD(string scriptText, bool isCMD = false)
        {
            try
            {
                if (isCMD)
                {
                    //return
                    //await Process.Start(@"C:\WINDOWS\system32\iisreset.exe", "/noforce")
                    //.StandardOutput.ReadToEndAsync();

                    var process = new Process();
                    Process.Start(@"C:\WINDOWS\system32\iisreset.exe", "/noforce");
                    ProcessStartInfo startInfo = new ProcessStartInfo("C:\\Windows\\System32\\iisreset.exe");

                    var argumentos = string.Format("\"{0}\"", "iisreset");

                    startInfo.Arguments = argumentos;
                    startInfo.Verb = "runas";

                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardInput = true;
                    startInfo.RedirectStandardOutput = true;

                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    process.StartInfo = startInfo;
                    process.Start();

                    process.StandardInput.Flush();
                    process.StandardInput.Close();
                    return process.StandardOutput.ReadToEnd();
                }
                else
                {
                    // create Powershell runspace
                    Runspace runspace = RunspaceFactory.CreateRunspace();

                    // open it
                    runspace.Open();

                    // create a pipeline and feed it the script text
                    Pipeline pipeline = runspace.CreatePipeline();
                    //pipeline.Commands.AddScript("'" + "C:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\MSBuild.exe" + "' " + scriptText);
                    pipeline.Commands.AddScript(scriptText);

                    // "Get-Process" returns a collection
                    // of System.Diagnostics.Process instances.
                    pipeline.Commands.Add("Out-String");

                    // execute the script
                    Collection<PSObject> results = pipeline.Invoke();

                    if (pipeline.Error.Count > 0)
                    {
                        // error records were written to the error stream.
                        // do something with the items found.
                    }

                    // close the runspace
                    runspace.Close();

                    // convert the script result into a single string
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (PSObject obj in results)
                    {
                        stringBuilder.AppendLine(obj.ToString());
                    }
                    return stringBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                return ex.Message + ex.StackTrace;
            }
        }

        public async Task<string> PublishProject(string solution, string directory)
        {
            Process process = null;
            try
            {
                process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo("C:\\TEMP\\Publisher.bat");

                var argumentos = string.Format("\"{0}\" \"{1}\"",
                    directory, solution);

                startInfo.Arguments = argumentos;

                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;

                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                process.StartInfo = startInfo;
                process.Start();

                process.StandardInput.Flush();
                process.StandardInput.Close();
                return process.StandardOutput.ReadToEnd();
                //return "Build and Publish " + scriptText + "Successful";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}