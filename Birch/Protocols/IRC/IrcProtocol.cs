using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Birch.Protocols.IRC {
    public class IrcChatProtocol : IChatProtocol {
        public int Port {
            get {
                return 6667;
            }
        }

        public string Name {
            get {
                return "IRC";
            }
        }

        public IChatProvider Connect (NetworkInformation information, INetworkView networkView) {
            IrcClient client = new IrcClient (networkView);
            Thread th = new Thread (() => {
                client.Connect (information.Address, 6697, information.UseEncryption);
            });
            th.Start ();
            return client;
        }

        public override string ToString () {
            return Name;
        }
    }
}
