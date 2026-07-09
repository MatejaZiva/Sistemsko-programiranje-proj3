using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Configuration;
namespace Sistemsko_programiranje_proj_3.Conf
{
    internal class SystemConfig
    {
        public static Config getAkkaConfig() => ConfigurationFactory.ParseString(@"
            football-dispatcher {
                type = Dispatcher
                executor = ""fork-join-executor""
                fork-join-executor {
                    parallelism-min = 4
                    parallelism-factor = 2.0
                    parallelism-max = 16
                }
                throughput = 100
            }");
            
    }
}
