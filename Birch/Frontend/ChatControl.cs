using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Birch.Frontend {
    public class ChatControl : UserControl, IChannelBuffer {
        const int LEFT_PADDING = 10;
        const int TOP_PADDING = 10;

        private string channel;
        private IChatProvider chatProvider;

        private RichTextBox chatTextBox;
        private ListBox namesListBox;
        private TextBox messageTextBox;
        private bool _showNames = true;

        private Color[] pallete = new Color[] { Color.Black, Color.Red };

        public bool ShowNames {
            set {
                _showNames = value;
                Invoke (new MethodInvoker (() => {
                    Size = new Size (Width, Height);
                    namesListBox.Visible = false;
                }));
            } get {
                return _showNames;
            }
        }

        public ChatControl (IChatProvider provider, string chan) {
            channel = chan;
            chatProvider = provider;
            InitializeComponent ();
        }

        private void InitializeComponent () {
            namesListBox = new ListBox ();
            chatTextBox = new RichTextBox ();
            messageTextBox = new TextBox ();
            messageTextBox.KeyPress += MessageTextBox_KeyPress;
            chatTextBox.Font = new Font (BirchSettings.Instance.MessageFont, 10);
            messageTextBox.Font = new Font (BirchSettings.Instance.MessageFont, 10);
            namesListBox.Font = new Font (BirchSettings.Instance.MessageFont, 10);
            chatTextBox.BackColor = Color.White;
            chatTextBox.ReadOnly = true;
            Controls.Add (namesListBox);
            Controls.Add (chatTextBox);
            Controls.Add (messageTextBox);
            pallete[0] = chatTextBox.ForeColor;
        }

        private void MessageTextBox_KeyPress (object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r') {
                if (channel == "" || channel == null && !messageTextBox.Text.StartsWith ("/")) {
                    AppendRaw ("You cannot send messages in this window!");
                } else {
                    if (messageTextBox.Text.StartsWith ("/")) {
                        InterpretCommand (messageTextBox.Text);
                    } else {
                        chatProvider.SendMessage (channel, messageTextBox.Text);
                        AppendMessage (chatProvider.Nickname, messageTextBox.Text);
                    }
                }
                messageTextBox.Text = "";
            }
        }

        protected override void OnResize (EventArgs e) {
            base.OnResize (e);

            if (ShowNames) {
                namesListBox.Width = (Width / 7) - LEFT_PADDING;
                namesListBox.Height = Height;
            }
            chatTextBox.Width = ShowNames ? Width - namesListBox.Width + LEFT_PADDING : Width;
            chatTextBox.Height = Height - (2 * TOP_PADDING + 20);
            chatTextBox.Location = new Point (ShowNames ? namesListBox.Width + LEFT_PADDING : 0, 0);
            
            messageTextBox.SetBounds (chatTextBox.Location.X, chatTextBox.Height + TOP_PADDING, chatTextBox.Width, 20);
      
        }

        public void SetNamesList (IEnumerable<string> names) {
            Invoke (new MethodInvoker (() => {
                namesListBox.Items.Clear ();
                foreach (string name in names) {
                    namesListBox.Items.Add (name);
                }
            }));
        }

        private void AppendText (string text) {
            bool escapeSequence = false;
            StringBuilder buffer = new StringBuilder ();
            for (int i = 0; i < text.Length; i++) {
                if (text [i] == '\u001B') {
                    escapeSequence = true;
                } else if (!escapeSequence) {
                    chatTextBox.AppendText (text[i].ToString ());
                } else {
                    if (text[i] != ';') {
                        buffer.Append (text[i]);
                    } else {
                        InterpretEscapeSequence (buffer.ToString ());
                        buffer.Clear ();
                        escapeSequence = false;
                    }
                }
            }
        }

        private void InterpretEscapeSequence (string seq) {
            if (seq.Length > 0) {
                char cmd = seq[0];
                string arguments = seq.Substring (1);
                switch (cmd) {
                    case 'c':
                        int code = 0;
                        if (!Int32.TryParse (arguments, out code)) {
                            return;
                        }
                        chatTextBox.Select (chatTextBox.Text.Length, 0);
                        chatTextBox.SelectionColor = pallete[code % pallete.Length];
                        break;
                }
            }
        }

        public void AppendRaw (string text) {
            Invoke (new MethodInvoker (() => {
                chatTextBox.AppendText (String.Format ("[{0:D2}:{1:D2}]", DateTime.Now.Hour, DateTime.Now.Minute));
                chatTextBox.AppendText ("".PadLeft (20) + "     " + text + "\r\n");
            }));
        }

        public void AppendMessage (string author, string text) {
            Invoke (new MethodInvoker (() => {
                chatTextBox.AppendText (String.Format ("[{0:D2}:{1:D2}]", DateTime.Now.Hour, DateTime.Now.Minute));
                int oldPos = chatTextBox.Text.Length;
                chatTextBox.AppendText (author.PadLeft (20));
                chatTextBox.Select (oldPos, chatTextBox.Text.Length - chatTextBox.SelectionLength);
                chatTextBox.SelectionColor = Color.Red;
                chatTextBox.Select (chatTextBox.Text.Length, 0);
                chatTextBox.SelectionColor = chatTextBox.ForeColor;
                chatTextBox.AppendText ("     " + text + "\r\n");
            }));
        }


        public void OnChatEvent (ChatEventType type, params string[] args) {
            Invoke (new MethodInvoker (() => {
                switch (type) {
                    case ChatEventType.Message:
                        AppendText (String.Format ("[{0:D2}:{1:D2}]{3}     {4}", DateTime.Now.Hour, DateTime.Now.Minute, args[0], args[1]));
                        break;
                    case ChatEventType.UserJoin:
                        AppendText (String.Format ("[{0:D2}:{1:D2}]{2}     \u001Bc1;{3}\u001Bc0; has joined!\n", DateTime.Now.Hour, DateTime.Now.Minute, "".PadLeft (20), args[0]));
                        namesListBox.Items.Add (args [0]);
                        break;
                    case ChatEventType.UserPart:
                        AppendText (String.Format ("[{0:D2}:{1:D2}]{2}     \u001Bc1;{3}\u001Bc0; has left!\n", DateTime.Now.Hour, DateTime.Now.Minute, "".PadLeft (20), args [0]));
                        namesListBox.Items.Remove (args[0]);
                        break;
                    case ChatEventType.NickChange:
                        AppendText (String.Format ("[{0:D2}:{1:D2}]{2}     \u001Bc1;{3}\u001Bc0; has changed their nickname to \u001Bc1;{4}\u001Bc0;!\n",
                            DateTime.Now.Hour,
                            DateTime.Now.Minute,
                            "".PadLeft (20),
                            args[0],
                            args[1]));
                        namesListBox.Items.Remove (args[0]);
                        namesListBox.Items.Add (args[1]);
                        break;
                }
            }));
        }


        public void UserJoin (string name) {
            Invoke (new MethodInvoker (() => {
                chatTextBox.AppendText (String.Format ("[{0:D2}:{1:D2}]{2}     {3} has joined!\r\n", DateTime.Now.Hour, DateTime.Now.Minute, "".PadLeft (20), name));
            }));
        }


        public void UserLeave (string name, string reason) {
            Invoke (new MethodInvoker (() => {
                chatTextBox.AppendText (String.Format ("[{0:D2}:{1:D2}]{2}     {3} has left\r\n", DateTime.Now.Hour, DateTime.Now.Minute, "".PadLeft (20), name));
            }));
        }


        private bool InterpretCommand (string cmd) {
            string[] args = cmd.Substring (1).Split (' ');
            switch (args [0].ToUpper ()) {
                case "JOIN":
                    if (args.Length >= 2) {
                        chatProvider.JoinChannel (args[1]);
                    } else {
                        AppendRaw ("Not enough arguments!");
                    }
                    return true;
                case "NICK":
                    if (args.Length >= 2) {
                        chatProvider.Nickname = args[1];
                    } else {
                        AppendRaw ("Not enough arguments!");
                    }
                    return true;

            }
            return false;
        }
    }
}
