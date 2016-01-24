using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Birch.Frontend {
    public class BirchMainForm : Form {

        private static BirchMainForm _instance = null;

        public static BirchMainForm Instance {
            get {
                if (_instance == null) {
                    _instance = new BirchMainForm ();
                }
                return _instance;
            }
        }
        private MenuStrip menustrip = new MenuStrip ();
        private ToolStripMenuItem birchMenuItem = new ToolStripMenuItem ("Birch");
        private ToolStripMenuItem viewMenuItem = new ToolStripMenuItem ("View");

        public BirchMainForm () {
            InitializeComponent ();
        }

        protected override void OnLoad (EventArgs e) {
            ConnectionsForm connections = new ConnectionsForm ();
            connections.StartPosition = FormStartPosition.CenterParent;
            connections.Parent = this;
            connections.Location = new System.Drawing.Point ((Width / 2) - (connections.Width / 2),( Height / 2) - (connections.Height / 2));
            connections.Show ();
        }

        private void InitializeComponent () {
            menustrip.Items.Add (birchMenuItem);
            menustrip.Items.Add (viewMenuItem);
            WindowState = FormWindowState.Maximized;
            Controls.Add (menustrip);
            IsMdiContainer = true;
            
        }
    }
}
