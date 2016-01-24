using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Birch {
    public interface IChannelBuffer {
        bool ShowNames {
            set;
            get;
        }

        void SetNamesList (IEnumerable<string> names);
        void AppendRaw (string raw);
        void AppendMessage (string author, string text);
        void OnChatEvent (ChatEventType type, params string[] args);
    }
}
