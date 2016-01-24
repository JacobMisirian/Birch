using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Birch {
    /// <summary>
    /// Provides an interface for interacting with an abstract TCP IRC-like chat protocol
    /// </summary>
    public interface IChatProtocol {
        /// <summary>
        /// Name of the protocol
        /// </summary>
        string Name {
            get;
        }

        /// <summary>
        /// Default port the protocol listens on
        /// </summary>
        int Port {
            get;
        }

        /// <summary>
        /// Connects to a server, returning an IChatProvider object
        /// </summary>
        /// <param name="information">Information regarding the server to connect to</param>
        /// <param name="networkView">INetworkView which the IChatProvider will use as its output</param>
        /// <returns></returns>
       IChatProvider Connect (NetworkInformation information, INetworkView networkView);
       string ToString ();
    }
}
