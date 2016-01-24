using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Birch {
    public static class ProtocolRegistry {
        private static List<IChatProtocol> birchProtocols = new List<IChatProtocol> ();

        public static IEnumerable<IChatProtocol> Protocols {
            get {
                return birchProtocols;
            }
        }

        static ProtocolRegistry () {
            RegisterProtocol (new Birch.Protocols.Jacochat.JacoChatProtocol ());
            RegisterProtocol (new Birch.Protocols.IRC.IrcChatProtocol ());
        }

        public static void RegisterProtocol (IChatProtocol proto) {
            birchProtocols.Add (proto);
        }

        public static IChatProtocol GetProtocol (string name) {
            return birchProtocols.Where (p => p.Name == name).FirstOrDefault ();
        }
    }
}
