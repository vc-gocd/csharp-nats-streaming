using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STAN.Client
{
    /// <summary>
    /// This class is used with an acknowleggement event handler indicating 
    /// a message has been acknoweledged by the STAN server.
    /// </summary>
    public class StanAckHandlerArgs : EventArgs
    {
        private string stanGuid;
        private string stanError;

        private StanAckHandlerArgs() { }

        internal StanAckHandlerArgs(string guid, string error)
        {
            this.stanGuid = guid;
            this.stanError = error;
        }

        /// <summary>
        /// Contains the GUID of the acknowledged message.
        /// </summary>
        public string GUID
        {
            get
            {
                return stanGuid;
            }
        }

        /// <summary>
        /// Returns an error if applicable.
        /// </summary>
        public string Error
        {
            get
            {
                return this.stanError;
            }
        }
    }
}
