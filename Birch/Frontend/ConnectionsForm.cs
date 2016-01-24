using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Birch.Frontend {
    public class ConnectionsForm : Form {
        private Label userInfoLabel = new Label ();
        private Label nicknameLabel = new Label ();
        private Label usernameLabel = new Label ();
        private Label networksLabel = new Label ();
        private TextBox nicknameTextbox = new TextBox ();
        private TextBox usernameTextbox = new TextBox ();
        private ListBox networkListbox = new ListBox ();

        private Button connectButton = new Button ();
        private Button deleteButtonButton = new Button ();
        private Button editButton = new Button ();
        private Button addButtonButton = new Button ();

        private TableLayoutPanel tabel = new TableLayoutPanel ();

        public ConnectionsForm () {
            InitializeComponent ();
            TopLevel = false;
        }

        private void InitializeComponent () {
            Width = 275;
            Height = 275;
            Text = "Network List";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            userInfoLabel.Text = "User Information";
            nicknameLabel.Text = "Nickname: ";
            nicknameTextbox.TextChanged += NicknameTextbox_TextChanged;
            usernameLabel.Text = "Username: ";
            usernameTextbox.TextChanged += UsernameTextbox_TextChanged;
            networksLabel.Text = "Networks";
            nicknameTextbox.Text = BirchSettings.Instance.Nickname;
            usernameTextbox.Text = BirchSettings.Instance.Username;
            connectButton.Text = "Connect";
            connectButton.MouseClick += ConnectButton_MouseClick;
            deleteButtonButton.Text = "Delete";
            editButton.Text = "Edit";
            editButton.MouseClick += EditButton_MouseClick;
            addButtonButton.Text = "Add Network";
            addButtonButton.MouseClick += AddButtonButton_MouseClick;
            tabel.AutoSize = true;
            tabel.RowCount = 5;
            tabel.ColumnCount = 2;

            tabel.Controls.Add (userInfoLabel, 0, 0);
            tabel.Controls.Add (nicknameLabel, 0, 1);
            tabel.Controls.Add (nicknameTextbox, 1, 1);
            tabel.Controls.Add (usernameLabel, 0, 2);
            tabel.Controls.Add (usernameTextbox, 1, 2);
            tabel.Controls.Add (networksLabel, 0, 3);
            tabel.Controls.Add (networkListbox, 0, 4);
            networkListbox.Dock = DockStyle.Fill;
            FlowLayoutPanel panel = new FlowLayoutPanel ();
            panel.FlowDirection = FlowDirection.TopDown;
            panel.AutoSize = true;
            panel.Controls.Add (connectButton);
            panel.Controls.Add (deleteButtonButton);
            panel.Controls.Add (editButton);
            panel.Controls.Add (addButtonButton);
            tabel.Controls.Add (panel, 1, 4);


            BirchSettings.Instance.Networks.ToList ().ForEach (p => networkListbox.Items.Add (p));
            Controls.Add (tabel);
        }

        private void UsernameTextbox_TextChanged (object sender, EventArgs e) {
            BirchSettings.Instance.Username = usernameTextbox.Text;
        }

        private void NicknameTextbox_TextChanged (object sender, EventArgs e) {
            BirchSettings.Instance.Nickname = nicknameTextbox.Text;
        }

        private void EditButton_MouseClick (object sender, MouseEventArgs e) {
            if (networkListbox.SelectedItem != null) {
                NewConnectionForm form = new NewConnectionForm ((NetworkInformation)networkListbox.SelectedItem);
                form.Parent = BirchMainForm.Instance;
                form.Location = Location;
                form.FormClosed += Form_FormClosed;
                form.Show ();
                Hide ();
            }
        }

        private void AddButtonButton_MouseClick (object sender, MouseEventArgs e) {
            NewConnectionForm form = new NewConnectionForm ();
            form.Parent = BirchMainForm.Instance;
            form.Location = Location;
            form.FormClosed += Form_FormClosed;
            form.Show ();
            Hide ();
        }

        private void Form_FormClosed (object sender, FormClosedEventArgs e) {
            networkListbox.Items.Clear ();
            BirchSettings.Instance.Networks.ToList ().ForEach (p => networkListbox.Items.Add (p));
            Show ();
        }

        private void ConnectButton_MouseClick (object sender, MouseEventArgs e) {
            if (networkListbox.SelectedItem != null) {
                NetworkInformation information = (NetworkInformation)networkListbox.SelectedItem;
                NetworkForm networkForm = new NetworkForm (information.Name);
                networkForm.MdiParent = BirchMainForm.Instance;
                networkForm.Show ();
                IChatProvider proto = information.DoConnect (networkForm);
                Close ();
            }
        }
    }
}
