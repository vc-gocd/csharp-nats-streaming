using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STAN.Client
{
    public class StanConnectionFactory
    {
        /// <summary>
        /// Returns the default connection options.
        /// </summary>
        /// <returns></returns>
        public static Options GetDefaultOptions()
        {
            return new Options();
        }

        /// <summary>
        /// Creates a connection to the server.
        /// </summary>
        /// <param name="stanClusterID"></param>
        /// <param name="clientID"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IConnection CreateConnection(string clusterID, string clientID, Options options)
        {
            return new Connection(clusterID, clientID, options);
        }

        /// <summary>
        /// Creates a connection to the server.
        /// </summary>
        /// <param name="stanClusterID"></param>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public IConnection CreateConnection(string clusterID, string clientID)
        {
            return new Connection(clusterID, clientID, null);
        }

        /// <summary>
        /// Returns the default subscription options.
        /// </summary>
        /// <returns></returns>
        public static SubscriptionOptions GetDefaultSubscriptionOptions()
        {
            return new SubscriptionOptions();
        }
    }
}
