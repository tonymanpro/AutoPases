using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoPases.Models
{
    public class ModProyectosActivos
    {
        public int IdProyecto { get; set; }
        public string Proyecto { get; set; }
        public string Bucket { get; set; }
        public string PublishFile { get; set; }
        public string DownloadPath { get; set; }
        public string TargetPath { get; set; }
        public string BackUpPath { get; set; }
        public string SiteName { get; set; }
        public string Entidad { get; set; }
        public int IdEntidad { get; set; }
        public int IdCompilar { get; set; }
        public string TreeView { get; set; }

    }
}