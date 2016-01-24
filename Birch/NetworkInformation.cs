using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Birch {
    public class NetworkInformation : IXmlSerializable {
        public readonly int Identifier;

        private string name;
        private string nickname;
        private string username;
        private string address;
        private int port;
        private bool useGlobalInformation;
        private bool useEncryption;
        private IChatProtocol protocol;

        public string Address {
            set {
                address = value;
                BirchSettings.Instance.Dirty = true;
            }
            get {
                return address;
            }
        }

        public int Port {
            set {
                port = value;
                BirchSettings.Instance.Dirty = true;
            }
            get {
                return port;
            }
        }

        public string Name {
            set {
                name = value;
                BirchSettings.Instance.Dirty = true;
            }
            get {
                return name;
            }
        }

        public bool UseGlobalUserInformation {
            set {
                useGlobalInformation = value;
                BirchSettings.Instance.Dirty = true;
            }
            get {
                return useGlobalInformation;
            }
        }

        public bool UseEncryption {
            set {
                useEncryption = value;
                BirchSettings.Instance.Dirty = true;
            }
            get {
                return useEncryption;
            }
        }

        public string Nickname {
            get {
                if (!UseGlobalUserInformation)
                    return nickname;
                return BirchSettings.Instance.Nickname;
            }
            set {
                nickname = value;
                BirchSettings.Instance.Dirty = true;
            }
        }

        public string Username {
            get {
                if (!UseGlobalUserInformation)
                    return username;
                return BirchSettings.Instance.Username;
            }
            set {
                username = value;
                BirchSettings.Instance.Dirty = true;
            }
        }

        public IChatProtocol Protocol {
            get {
                return protocol;
            }
            set {
                protocol = value;
                BirchSettings.Instance.Dirty = true;
            }
        }

        public NetworkInformation () {
            useGlobalInformation = true;
        }

        public NetworkInformation (IChatProtocol protocol, int id, string address, int port, string name, bool useEncryption = false)
            : this () {
            Protocol = protocol;
            Address = address;
            Port = port;
            Name = name;
            Identifier = id;
            UseEncryption = useEncryption;
        }

        public override string ToString () {
            return Name;
        }

        public IChatProvider DoConnect (INetworkView network) {
            return Protocol.Connect (this, network);
        }

        public XmlSchema GetSchema () {
            return null;
        }

        public void ReadXml (XmlReader reader) {
            int od = reader.Depth;
            reader.Read ();
            while (reader.Depth != od) {
                switch (reader.Name) {
                    case "Protocol":
                        protocol = ProtocolRegistry.GetProtocol (reader.ReadInnerXml ());
                        break;
                    case "Address":
                        address = reader.ReadInnerXml ();
                        break;
                    case "Name":
                        name = reader.ReadInnerXml ();
                        break;
                    case "Port":
                        port = Int32.Parse (reader.ReadInnerXml ());
                        break;
                    default:
                        reader.ReadInnerXml ();
                        break;
                }
            }
            reader.Read ();
        }

        public void WriteXml (XmlWriter writer) {
            writer.WriteElementString ("Protocol", protocol.Name);
            writer.WriteElementString ("Address", address);
            writer.WriteElementString ("Name", name);
            writer.WriteElementString ("Port", port.ToString ());
            writer.WriteElementString ("UseGlobalInformation", useGlobalInformation.ToString ());
            writer.WriteElementString ("UseEncryption", useEncryption.ToString ());
        }
    }
}
