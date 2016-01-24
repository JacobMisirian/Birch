using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Birch.Frontend {
    public class NetworkForm : Form, INetworkView {
        
        private string networkName;
        private IChatProvider chatProvider;
        private TabControl tabControl = new TabControl ();
        private ChatControl statusControl;

        public IChannelBuffer StatusBuffer {
            get {
                return statusControl;
            }
        }

        public NetworkForm (string name) {
            TopLevel = false;
            networkName = name;
            InitializeComponent (); //AppendMessage
        }

        public void Connected (IChatProvider provider) {
            chatProvider = provider;
            Invoke (new MethodInvoker (() => {
                statusControl = new ChatControl (chatProvider, null);
                statusControl.Dock = DockStyle.Fill;
                TabPage statusPage = new TabPage ();
                statusPage.Text = "Status";
                statusPage.Dock = DockStyle.Fill;
                statusPage.Controls.Add (statusControl);
                tabControl.TabPages.Add (statusPage);
                statusControl.ShowNames = false;
                Controls.Add (tabControl);
            }));
        }

        protected override void OnLoad (EventArgs e) {
            base.OnLoad (e);
        }

        private void InitializeComponent () {
            tabControl.Dock = DockStyle.Fill;
            ShowIcon = false;
            WindowState = FormWindowState.Maximized;
            Text = "Network: " + networkName;
        }
        
        public void ChangeNick (string old, string newNick) {
        }

        public IChannelBuffer JoinChannel (string name) {
            ChatControl chatControl = new ChatControl (chatProvider, name);
            Invoke (new MethodInvoker (() => {
                TabPage chatPage = new TabPage ();
                chatPage.Text = name;
                chatControl.Dock = DockStyle.Fill;
                chatPage.Controls.Add (chatControl);
                tabControl.TabPages.Add (chatPage);
            }));
            return chatControl;
        }

        public void LeaveChannel (string channel) {
            throw new NotImplementedException ();
        }
    }
}
