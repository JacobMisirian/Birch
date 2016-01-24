using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Birch.Protocols.IRC {
    public class ChannelEventArgs : IrcEventArgs {
        public readonly string Channel;

        public ChannelEventArgs (string nick, string channel)
            : base (nick) {
                Channel = channel;
        }
    }
}
