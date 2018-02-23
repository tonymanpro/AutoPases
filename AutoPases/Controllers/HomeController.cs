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

namespace AutoPases.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var connString = AppConfiguracion.Instance.ConnectionString;
            var proyectos = new List<Models.ModProyectosActivos>();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                var statement = ReadXml.GetSqlStatement("SqlProyectosActivos");

                var commd = new SqlCommand(statement, conn);
                SqlDataReader data = commd.ExecuteReader();
                while (data.Read())
                {
                    var archivo = data.CreateItemFromReader<Models.ModProyectosActivos>(); 
                    proyectos.Add(archivo);
                }
            }
            InfoSesion.IncluirSesion("ProyectosActivos", proyectos);
            return View(proyectos);
        }

        public ActionResult GetTreeView(string viewID)
        {
            return PartialView(string.Format("_treeview{0}", viewID));
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
        public async Task<JsonResult> CreateBackUpAsync(string sourcePath, string targetPath)
        {
            try
            {
                await moveFiles(sourcePath, targetPath);
                return Json(true);
            }
            catch (Exception ex)
            {
                return Json(new { result = false, response = ex.Message });
            }
        }
        public async Task<JsonResult> PublishSiteAsync(string publishFiles, string command,
                                      string sourcePath, string targetPath)
        {
            try
            {
                if (string.IsNullOrEmpty(publishFiles))
                {
                    await moveFiles(sourcePath, targetPath);
                    return Json(true);
                }
                else
                {
                    var response = await ExecuteAsynchronously(command);
                    return Json(new { result = true, response });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = false, response = ex.Message });
            }
        }
        public async Task<JsonResult> StartSiteAsync(string pageSite)
        {
            var result = await IISManager(pageSite, false);
            return Json(result);
        }

        public async Task moveFiles(string sourcePath, string targetPath)
        {
            // Use Path class to manipulate file and directory paths.
            string sourceFile = System.IO.Path.Combine(sourcePath);
            string destFile = System.IO.Path.Combine(targetPath);

            // To copy a file to another location and 
            // overwrite the destination file if it already exists.
            //System.IO.File.Copy(sourceFile, destFile, true);

            bool isExists = System.IO.Directory.Exists(targetPath);
            if (!isExists)
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }

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
        public async Task<bool> IISManager(string webSiteName, bool stop)
        {
            var server = new ServerManager();
            var site = server.Sites.FirstOrDefault(s => s.Name == webSiteName);
            if (site != null)
            {
                if (stop)
                {
                    site.Stop();
                    if (site.State == ObjectState.Stopped)
                    {
                        return true;
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
                var message = "";
                foreach (PSObject outputItem in outputCollection)
                {
                    //TODO: handle/process the output items if required
                    message += outputItem.BaseObject.ToString();
                }
                return message;
            }
        }

    }
}