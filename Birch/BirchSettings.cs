using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace Birch {
    public class BirchSettings : IXmlSerializable {
        private static BirchSettings _instance;

        public static BirchSettings Instance {
            get {
                if (_instance == null) {
                    if (File.Exists ("settings.xml")) {
                        _instance = LoadSettings ("settings.xml");
                    } else {
                        _instance = new BirchSettings ();
                        _instance.DefaultSettings ();
                    }
                }
                return _instance;
            }
        }

        private bool isDirty;
        private string username;
        private string nickname;
        private FontFamily messageFont;
        private FontFamily uiFont;
        private List<NetworkInformation> networkInformation = new List<NetworkInformation> ();

        public bool Dirty {
            get {
                return isDirty;
            }
            set {
                isDirty = value;
            }
        }

        public string Nickname {
            get {
                return nickname;
            }
            set {
                nickname = value;
                isDirty = true;
            }
        }
        
        public string Username {
            get {
                return username;
            }
            set {
                username = value;
                isDirty = true;
            }
        }

        public FontFamily MessageFont {
            get {
                return messageFont;
            }
            set {
                messageFont = value;
                isDirty = true;
            }
        }

        public FontFamily UiFont {
            get {
                return uiFont;
            }
            set {
                uiFont = value;
                isDirty = true;
            }
        }
        public IEnumerable<NetworkInformation> Networks {
            get {
                return networkInformation;
            }
        }

        public BirchSettings () {
            InitializeWatchdogThread ();
        }

        public void DefaultSettings () {
            Nickname = "Birch";
            Username = "Birch";
            uiFont = new FontFamily ("Arial");
            messageFont = new FontFamily ("Consolas");
            AddNetworkInformation (new NetworkInformation (ProtocolRegistry.GetProtocol ("IRC"), 0, "int0x10.com", 6697, "int0x10", true));
        }

        private void InitializeWatchdogThread () {
            Thread th = new Thread (() => {
                for (;;) {
                    if (isDirty) {
                        WriteSettings ("settings.xml");
                    }
                    Thread.Sleep (10000);
                }
            });
            th.Start ();
        }

        public void AddNetworkInformation (NetworkInformation information) {
            networkInformation.Add (information);
            isDirty = true;
        }

        public void RemoveNetworkInformation (NetworkInformation information) {
            networkInformation.Remove (information);
            isDirty = true;
        }

        private void WriteSettings (string output) {
            lock (networkInformation) {
                XmlSerializer serializer = new XmlSerializer (typeof (BirchSettings));
                using (StreamWriter sw = new StreamWriter (output)) {
                    serializer.Serialize (sw, this);
                }
                isDirty = false;
            }
        }

        private static BirchSettings LoadSettings (string path) {
            XmlSerializer serializer = new XmlSerializer (typeof (BirchSettings));
            using (StreamReader sr = new StreamReader (path)) {
                return (BirchSettings)serializer.Deserialize (sr);
            }
        }

        public XmlSchema GetSchema () {
            return null;
        }

        public void ReadXml (XmlReader reader) { 
            while (!reader.EOF) {
                switch (reader.Name) {
                    case "UiFont":
                        uiFont = new FontFamily (reader.ReadInnerXml ());
                        break;
                    case "MessageFont":
                        messageFont = new FontFamily (reader.ReadInnerXml ());
                        break;
                    case "Username":
                        username = reader.ReadInnerXml ();
                        break;
                    case "Nickname":
                        nickname = reader.ReadInnerXml ();
                        break;
                    case "NetworkList":
                        ReadNetworkList (reader);
                        break;
                    default:
                        reader.Read ();
                        break;
                }
            }
        }

        private void ReadNetworkList (XmlReader reader) {
            XmlSerializer serializer = new XmlSerializer (typeof (NetworkInformation));
            int startDepth = reader.Depth;
            reader.Read ();
            while (reader.Depth != startDepth) { 
                NetworkInformation info = (NetworkInformation)serializer.Deserialize (reader);
                networkInformation.Add (info);
            }
            reader.Read ();
        }
        public void WriteXml (XmlWriter writer) {
            writer.WriteElementString ("Username", username);
            writer.WriteElementString ("Nickname", nickname);
            writer.WriteElementString ("UiFont", uiFont.Name);
            writer.WriteElementString ("MessageFont", messageFont.Name);
            XmlSerializer serializer = new XmlSerializer (typeof (NetworkInformation));
            writer.WriteStartElement ("NetworkList");
            foreach (NetworkInformation info in networkInformation) {
                serializer.Serialize (writer, info);
            }
            writer.WriteEndElement ();

        }
    }
}
