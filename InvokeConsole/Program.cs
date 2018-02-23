using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvokeConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Methods.ExecuteAsynchronously("ipconfig");
        }
    }
}
