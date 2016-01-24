using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Birch {
    public enum ChatEventType {
        Message,
        NickChange,
        UserJoin,
        UserPart,
        TopicChange
    }
}
