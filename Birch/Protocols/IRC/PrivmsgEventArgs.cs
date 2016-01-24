using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Birch.Protocols.IRC {

    public class PrivmsgEventArgs : ChannelEventArgs {
        public readonly string Message;

        public PrivmsgEventArgs (string name, string channel, string message) 
            : base (name, channel) {
                Message = message;
        }
    }
}
