using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Birch.Frontend {
    public class NewConnectionForm : Form {
        private Label userInfoLabel = new Label ();
        private Label nicknameLabel = new Label ();
        private Label usernameLabel = new Label ();
        private TextBox usernameTextBox = new TextBox ();
        private TextBox nicknameTextBox = new TextBox ();
        private CheckBox useGlobalInfoCheckBox = new CheckBox ();
        private Label serverInfoLabel = new Label ();
        private Label nameLabel = new Label ();
        private TextBox nameTextBox = new TextBox ();
        private Label ipLabel = new Label ();
        private TextBox ipTextBox = new TextBox ();
        private Label portLabel = new Label ();
        private TextBox portTextBox = new TextBox ();
        private Label encryptionLabel = new Label ();
        private CheckBox encryptionCheckBox = new CheckBox ();
        private Label protocolLabel = new Label ();
        private ComboBox protoComboBox = new ComboBox ();
        private Button saveButton = new Button ();
        private Button closeButton = new Button ();

        private NetworkInformation networkInformation;

        public NewConnectionForm (NetworkInformation netinfo = null) {
            networkInformation = netinfo;
            InitializeComponent ();
        }

        private void InitializeComponent () {
            FlowLayoutPanel mainPanel = new FlowLayoutPanel ();
            TableLayoutPanel table = new TableLayoutPanel ();
            Text = "Edit Network Information";
            ShowIcon = false;
            TopLevel = false;

            Width = 275;
            Height = 355;

            mainPanel.Dock = DockStyle.Fill;
            table.AutoSize = true;
            
            mainPanel.FlowDirection = FlowDirection.TopDown;
            userInfoLabel.Text = "User Information";
            nicknameLabel.Text = "Nickname:";
            usernameLabel.Text = "Username: ";
            nicknameTextBox.Enabled = false;
            usernameTextBox.Enabled = false;
            useGlobalInfoCheckBox.Text = "Use Defaults";
            useGlobalInfoCheckBox.Checked = true;
            serverInfoLabel.Text = "Server Information";
            nameLabel.Text = "Network Name: ";
            ipLabel.Text = "Host Name: ";
            portLabel.Text = "Port: ";
            encryptionCheckBox.Text = "Use Encryption";
            protocolLabel.Text = "Chat Protocol";
            protoComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            InitializeProtocolList ();
            saveButton.Text = saveButton == null ? "Add Network" : "Save Changes";
            saveButton.MouseClick += SaveButton_MouseClick;
            closeButton.Text = "Close";
            closeButton.MouseClick += CloseButton_MouseClick;
            table.Controls.Add (userInfoLabel, 0, 0);
            table.Controls.Add (nicknameLabel, 0, 1);
            table.Controls.Add (nicknameTextBox, 1, 1);
            table.Controls.Add (usernameLabel, 0, 2);
            table.Controls.Add (usernameTextBox, 1, 2);
            table.Controls.Add (useGlobalInfoCheckBox, 0, 3);
            table.Controls.Add (serverInfoLabel, 0, 4);
            table.Controls.Add (nameLabel, 0, 5);
            table.Controls.Add (nameTextBox, 1, 5);
            table.Controls.Add (ipLabel, 0, 6);
            table.Controls.Add (ipTextBox, 1, 6);
            table.Controls.Add (portLabel, 0, 7);
            table.Controls.Add (portTextBox, 1, 7);
            table.Controls.Add (protocolLabel, 0, 8);
            table.Controls.Add (protoComboBox, 1, 8);
            table.Controls.Add (encryptionCheckBox, 0, 9);
            table.Controls.Add (saveButton, 0, 10);
            mainPanel.Controls.Add (table);
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel ();
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.AutoSize = true;
            buttonPanel.Controls.Add (saveButton);
            buttonPanel.Controls.Add (closeButton);
            mainPanel.Controls.Add (buttonPanel);
            if (networkInformation != null) {
                nameTextBox.Text = networkInformation.Name;
                ipTextBox.Text = networkInformation.Address;
                portTextBox.Text = networkInformation.Port.ToString ();
                useGlobalInfoCheckBox.Checked = networkInformation.UseGlobalUserInformation;
            }
            Controls.Add (mainPanel);
        }

        private void InitializeProtocolList () {
            foreach (IChatProtocol proto in ProtocolRegistry.Protocols) {
                protoComboBox.Items.Add (proto);
            }
        }

        private void SaveButton_MouseClick (object sender, MouseEventArgs e) {
            bool modify = networkInformation != null;
            int port = Int32.Parse (portTextBox.Text);

            if (!modify) {
                networkInformation = new NetworkInformation (ProtocolRegistry.GetProtocol ("Jacochat"), 0, ipTextBox.Text, port, nameTextBox.Text);
                BirchSettings.Instance.AddNetworkInformation (networkInformation);
            } else {
                networkInformation.Name = nameTextBox.Text;
                networkInformation.Address = ipTextBox.Text;
                networkInformation.Port = port;
                networkInformation.UseGlobalUserInformation = useGlobalInfoCheckBox.Checked;
                networkInformation.Protocol = ProtocolRegistry.GetProtocol (protoComboBox.SelectedItem.ToString ());
            }
            Close ();
        }

        private void CloseButton_MouseClick (object sender, MouseEventArgs e) {
            Close ();
        }
    }
    
}
