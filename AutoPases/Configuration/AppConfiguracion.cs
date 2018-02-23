using System.Configuration;

namespace AutoPases.Controllers
{
    public class AppConfiguracion
    {
        private static AppConfiguracion instance;
        public static AppConfiguracion Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AppConfiguracion();
                }
                return instance;
            }
        }
        private AppConfiguracion()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["MonibyteConn"].ConnectionString;
        }
        private static string ObtenerValor(string llave)
        {
            return ConfigurationManager.AppSettings[llave];
        }
        public string ConnectionString { get; private set; }
    }

}