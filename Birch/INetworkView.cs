using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Birch {
    public interface INetworkView {
        IChannelBuffer StatusBuffer {
            get;
        }
        IChannelBuffer JoinChannel (string name);
        void Connected (IChatProvider provider);
        void LeaveChannel (string channel);
        void ChangeNick (string old, string newNick);
    }
}
