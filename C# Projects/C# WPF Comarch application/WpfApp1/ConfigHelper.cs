using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetupOptimaToolkit
{
    public class ConfigHelper
    {
        public static string GetConnectionString()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
            return connectionStringsSection.ConnectionStrings["CDN_Prezentacja_KHEntities"].ConnectionString;
                
        }
    }
}
