using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Birch.Protocols.Jacochat {
    public class JacoChatProtocol : IChatProtocol {
        public int Port {
            get {
                return 1337;
            }
        }

        public string Name {
            get {
                return "Jacochat";
            }
        }

        public IChatProvider Connect (NetworkInformation information, INetworkView networkView) {
            JacochatProvider provider = new JacochatProvider ();
            provider.Connect (networkView, information.Address, information.Port);
            Console.WriteLine (information.Nickname + " is nick");
            provider.Nickname = information.Nickname;
            return provider;
        }

        public override string ToString () {
            return Name;
        }
    }
}
