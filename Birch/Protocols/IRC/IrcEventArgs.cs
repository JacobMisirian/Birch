using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Birch.Protocols.IRC {

    public class IrcEventArgs : EventArgs {
        public readonly string Name;

        public IrcEventArgs (string name) {
            Name = name;
        }
    }
}
