using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;

namespace Birch.Protocols.IRC {

    public delegate void ConnectEvent (IrcClient sender, ConnectEventArgs args);
    public delegate void IrcEvent (IrcClient sender, IrcEventArgs args);
    public delegate void PrivmsgEvent (IrcClient sender, PrivmsgEventArgs args);
    public delegate void ChannelEvent (IrcClient sender, ChannelEventArgs args);

    public class IrcClient : IChatProvider {

        public string Nickname {
            private set;
            get;
        }

        public string Hostname {
            private set;
            get;
        }

        public string Username {
            private set;
            get;
        }

        string IChatProvider.Nickname {
            get {
                return Nickname;
            }
            set {

            }
        }

        public event ConnectEvent ConnectionMade;
        public event PrivmsgEvent Privmsg;
        public event ChannelEvent Joined;
        public event ChannelEvent UserJoined;

        public IrcClient (INetworkView networkView) {
            Nickname = "Foobar";
            Hostname = "Fooserv";
            Username = "Foo";
            this.networkView = networkView;
        }

        private Mutex clientLock = new Mutex ();
        private TcpClient client = new TcpClient ();
        private StreamWriter streamWriter;
        private StreamReader streamReader;
        private ManualResetEvent connectDone = new ManualResetEvent (false);
        private Dictionary<string, IChannelBuffer> channels = new Dictionary<string, IChannelBuffer> ();
        private INetworkView networkView;

        public bool Connect (string server, int port, bool ssl = false, int timeout = 1800) {
            client.BeginConnect (server, port, result => {
                connectDone.Set ();
            }, this);

            connectDone.WaitOne (1800, false);

            if (!client.Connected) {
                return false;
            }

            Stream baseStream = client.GetStream ();

            if (ssl) {
                baseStream = new SslStream (baseStream, false,
                    new RemoteCertificateValidationCallback ((sender, certificate, chain, sslPolicyErrors) => true),
                    null);
                ((SslStream)baseStream).AuthenticateAsClient (server);
            }

            streamWriter = new StreamWriter (baseStream);
            streamReader = new StreamReader (baseStream);

            ConnectEventArgs args = new ConnectEventArgs ();
            networkView.Connected (this);
            Thread.Sleep (100);
            OnConnectionMade (args);

            Nickname = args.Nickname ?? BirchSettings.Instance.Nickname;
            Hostname = args.Hostname ?? "BirchClient";
            Username = args.Realname ?? BirchSettings.Instance.Username;

            SendRaw ("USER {0} {1} {2} :{3}", Nickname, Hostname, Hostname, Username);
            SendRaw ("NICK {0}", Nickname);
            EnterMessageLoop ();

            return true;
        }

        public void SendPrivmsg (string channel, string message) {
            SendRaw ("PRIVMSG {0} :{1}", channel, message);
        }

        public void JoinChannel (string channel) {
            SendRaw ("JOIN {0}", channel);
        }

        public void PartChannel (string channel, string reason = "") {
            SendRaw ("PART {0} :{1}", channel, reason);
        }

        protected virtual void OnConnectionMade (ConnectEventArgs args) {
            if (ConnectionMade != null) {
                ConnectionMade (this, args);
            }
        }

        protected virtual void OnPrivmsg (PrivmsgEventArgs args) {
            if (Privmsg != null) {
                Privmsg (this, args);
            }
        }

        protected virtual void OnJoinedChannel (ChannelEventArgs args) {
            if (Joined != null) {
                Joined (this, args);
            }
        }

        protected virtual void OnUserJoinedChannel (ChannelEventArgs args) {
            if (UserJoined != null) {
                UserJoined (this, args);
            }
        }

        public void SendRaw (string msg) {
            clientLock.WaitOne ();
            streamWriter.Write ("{0}\r\n", msg);
            streamWriter.Flush ();
            clientLock.ReleaseMutex ();
        }

        public void SendRaw (string format, params object[] args) {
            SendRaw (String.Format (format, args));
        }

        private void EnterMessageLoop () {
            while (client.Connected) {
                string msg = streamReader.ReadLine ().Trim ();
                DecodeMessage (msg);
            }
        }

        private void DecodeMessage (string ircMessage) {
            string prefix;
            string command;
            string parameters;
            string name;
            string user;
            string host;
            ParseMessage (ircMessage, out prefix, out command, out parameters);
            ParsePrefix (prefix, out name, out user, out host);
            string[] splitParams = parameters.Split (' ');
            switch (command) {
                case "NOTICE":
                case "371":
                case "372": {
                        networkView.StatusBuffer.AppendRaw (parameters.Substring (parameters.IndexOf (":") + 1).Trim ());
                        break;
                    }
                case "467":
                case "471":
                case "472":
                case "473":
                case "474":
                case "475": {
                        networkView.StatusBuffer.AppendRaw (parameters.Substring (parameters.IndexOf (":") + 1).Trim ());
                        break;
                    }
                case "PRIVMSG": {
                        string channel = parameters.Substring (0, parameters.IndexOf (":")).Trim ();
                        if (!channels.ContainsKey (channel)) {
                            channels.Add (channel, networkView.JoinChannel (channel));
                        }
                        channels[channel].AppendMessage (name, parameters.Substring (parameters.IndexOf (':') + 1));
                        break;
                    }
                case "JOIN": {
                        string channel = parameters.Substring (parameters.IndexOf (":") + 1).Trim ();
                        if (!channels.ContainsKey (channel)) {
                            channels.Add (channel, networkView.JoinChannel (channel));
                        }
                        channels[channel].OnChatEvent (ChatEventType.UserJoin, name, channel);
                        SendRaw ("NAMES {0}", channel);
                        break;
                    }
                case "PART": {
                        string channel = parameters.Substring (parameters.IndexOf (":") + 1).Trim ();
                        if (channels.ContainsKey (channel)) {
                            channels[channel].OnChatEvent (ChatEventType.UserPart, name, channel);
                            channels.Remove (channel);
                        }
                        break;
                    }
                case "353": {
                        string channel = parameters.Substring (parameters.IndexOf ("=") + 1).Trim ();
                        channel = channel.Substring (0, channel.IndexOf (":")).Trim ();
                        string names = parameters.Substring (parameters.IndexOf (":") + 1).Trim ();
                        channels[channel].SetNamesList (names.Split (' '));
                        break;
                    }
                case "PING":
                    SendRaw ("PONG {0}", parameters);
                    break;
            }
        }

        private void ParsePrefix (string prefix, out string name, out string user, out string host) {
            if (prefix.Contains ("!")) {
                name = prefix.Substring (0, prefix.IndexOf ('!'));
                string suffix = prefix.Substring (prefix.IndexOf ('!') + 1);
                user = suffix.Substring (0, suffix.IndexOf ("@"));
                host = suffix.Substring (suffix.IndexOf ("@") + 1);
            } else {
                name = prefix;
                user = "";
                host = "";
            }
        }

        private void ParseMessage (string ircMessage,
            out string prefix,
            out string command,
            out string parameters) {
            if (ircMessage.StartsWith (":"))
                prefix = ircMessage.Substring (1, ircMessage.IndexOf (' '));
            else
                prefix = "";
            ParseCommandAndParameters (ircMessage, out command, out parameters);
        }

        private void ParseCommandAndParameters (string ircMessage, out string command, out string parameters) {
            string part = ircMessage;
            if (part.StartsWith (":")) {
                part = ircMessage.Substring (ircMessage.IndexOf (' ')).Trim ();
            }

            int lastLetter = 3;
            if (part.Length >= 3 && char.IsDigit (part[0]) &&
                char.IsDigit (part[1]) && char.IsDigit (part[2])) {
                command = part.Substring (0, 3);
            } else {
                lastLetter = 0;
                for (int i = 0; i < part.Length; i++) {
                    if (!char.IsLetter (part[i])) {
                        lastLetter = i;
                        break;
                    }
                }
                command = part.Substring (0, lastLetter);
            }
            parameters = part.Substring (lastLetter).Trim ();
        }

        public bool Connect (INetworkView net, string ipAddr, int port) {
            throw new NotImplementedException ();
        }

        public void SendMessage (string channel, string message) {
            SendPrivmsg (channel, message);
        }
    }
}
