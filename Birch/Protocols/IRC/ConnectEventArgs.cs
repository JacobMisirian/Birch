using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Birch.Protocols.IRC {

    public class ConnectEventArgs : EventArgs {
        public string Nickname {
            set;
            get;
        }

        public string Hostname {
            set;
            get;
        }

        public string Realname {
            set;
            get;
        }
    }
}
