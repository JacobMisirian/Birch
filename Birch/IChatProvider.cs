using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Birch {
    public interface IChatProvider {
        string Nickname {
            get;
            set;
        }
        
        void SendMessage (string channel, string message);
        void SendRaw (string msg);
        void JoinChannel (string channel);
    }
}
