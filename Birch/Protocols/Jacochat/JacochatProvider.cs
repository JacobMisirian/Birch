using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JacoChatClient;

namespace Birch.Protocols {
    public class JacochatProvider : IChatProvider {
        private string nickname;
        private INetworkView network;
        private JacoChatClient.JacoChatClient client;
        private Dictionary<string, IChannelBuffer> channels = new Dictionary<string, IChannelBuffer> ();

        public string Nickname {
            get {
                return nickname;
            }
            set {
                SetNick (value);
            }
        }

        public bool Connect (INetworkView net, string ipAddr, int port) {
            client = new JacoChatClient.JacoChatClient ();
            try {
                client.Connect (ipAddr, port);
            } catch {
                return false;
            }
            network = net;
            client.MessageRecieved += Client_MessageRecieved;
            net.Connected (this);
            return true;
        }

        private void Client_MessageRecieved (object sender, MessageRecievedEventArgs e) {
            JacoChatMessage msg = JacoChatMessage.Parse (e.Message);
            switch (msg.JacoChatMessageType) {
                case JacoChatMessageType.PRIVMSG:
                    string channel = msg.Channel == nickname ? msg.Sender : msg.Channel;
                    checkChannel (channel);
                    if (!channel.StartsWith ("#"))
                        channels[channel].ShowNames = false;
                    channels[channel].AppendMessage (msg.Sender, msg.Body);
                    break;
                case JacoChatMessageType.NAMES:
                    checkChannel (msg.Channel);
                    channels[msg.Channel].SetNamesList (msg.Body.Trim ().Split (' '));
                    break;
                case JacoChatMessageType.NICK:
                    checkChannel (msg.Channel);
                    channels[msg.Channel].OnChatEvent (ChatEventType.NickChange, msg.Sender, msg.Body);
                    break;
                case JacoChatMessageType.JOIN:
                    checkChannel (msg.Channel);
                    Console.WriteLine ("Join '{0}'", msg.Sender);
                    channels[msg.Channel].OnChatEvent (ChatEventType.UserJoin, msg.Sender);
                    break;
                case JacoChatMessageType.PART:
                    checkChannel (msg.Channel);
                    channels[msg.Channel].OnChatEvent (ChatEventType.UserPart, msg.Sender, msg.Body);
                    break;
                case JacoChatMessageType.ERROR:
                case JacoChatMessageType.UNKNOWN:
                    network.StatusBuffer.AppendRaw (msg.Body);
                    break;
            }
        }

        public void SendMessage (string channel, string message) {
            client.Send (String.Format ("PRIVMSG {0} {1}", channel, message));
        }

        public void SendRaw (string msg) {
            client.Send (msg);
        }

        public void JoinChannel (string name) {
            client.Send ("JOIN " + name);
            Console.WriteLine ("Attempted to join " + name);
        }

        public void PartChannel (string channel, string reason = "") {
            client.Send ("PART " + channel + " " + reason);
            Console.WriteLine ("Attempted to part " + channel);
        }
        
        private void SetNick (string name) {
            nickname = name;
            client.Send ("NICK " + name);
        }

        private void checkChannel (string channel) {
            if (!channels.ContainsKey(channel))
                channels.Add(channel, network.JoinChannel(channel));
        }
    }
}
