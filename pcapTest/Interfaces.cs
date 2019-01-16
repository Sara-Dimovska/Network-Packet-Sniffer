using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PcapDotNet.Core;
using PcapDotNet.Packets;

using SharpPcap.LibPcap;
using SharpPcap;
using PacketDotNet;

namespace pcapTest
{
    public partial class Interfaces : Form
    {
        List<LibPcapLiveDevice> interfaceList =  new List<LibPcapLiveDevice>();

        public Interfaces()
        {
            InitializeComponent();
        }

        private void Interfaces_Load(object sender, EventArgs e)
        {
            LibPcapLiveDeviceList devices = LibPcapLiveDeviceList.Instance;

            foreach (LibPcapLiveDevice device in devices)
            {
                if (!device.Interface.Addresses.Exists(a => a != null && a.Addr != null && a.Addr.ipAddress != null)) continue;
                var devInterface = device.Interface;
                var friendlyName = devInterface.FriendlyName;
                var description = devInterface.Description;

                interfaceList.Add(device);
                mInterfaceCombo.Items.Add(friendlyName);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(mInterfaceCombo.SelectedIndex >=0 && mInterfaceCombo.SelectedIndex < interfaceList.Count)
            {
                MainForm openMainForm = new MainForm(interfaceList, mInterfaceCombo.SelectedIndex);
                this.Hide();
                openMainForm.Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /*
private static void Device_OnPacketArrival(object sender, CaptureEventArgs e)
{
DateTime time = e.Packet.Timeval.Date;
int len = e.Packet.Data.Length;
//var ipPacket = IpPacket.GetEncapsulated(packet);

//Console.WriteLine(" TIME = {0}:{1}:{2}:{3} Len = {4} DATA= ",
//  time.Hour, time.Minute, time.Second, time.Millisecond, len);

var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
// var ip = (PacketDotNet.IpPacket)packet.Extract(typeof(PacketDotNet.IpPacket));


var tcpPacket = (TcpPacket)packet.Extract(typeof(TcpPacket));
var ipPacket = (IpPacket)packet.Extract(typeof(IpPacket));
var udpPacket = (UdpPacket)packet.Extract(typeof(UdpPacket));

if(ipPacket != null)
{
System.Net.IPAddress srcIp = ipPacket.SourceAddress;
System.Net.IPAddress dstIp = ipPacket.DestinationAddress;
var protocolPacket = ipPacket.PayloadPacket;


Console.WriteLine(protocolPacket.ToString());

/*
if (tcpPacket != null)
{
int srcPort = tcpPacket.SourcePort;
int dstPort = tcpPacket.DestinationPort;

Console.WriteLine("{0}:{1}:{2},{3} Len={4} {5}:{6} -> {7}:{8}  TCP = {9}",
time.Hour, time.Minute, time.Second, time.Millisecond, len,
srcIp, srcPort, dstIp, dstPort, tcpPacket.ToString());
}

if (udpPacket != null)
{
int udp_srcPort = udpPacket.SourcePort;
int udp_dstPort = udpPacket.DestinationPort;

Console.WriteLine("{0}:{1}:{2},{3} Len={4} {5}:{6} -> {7}:{8}  UDP = {9}",
time.Hour, time.Minute, time.Second, time.Millisecond, len,
srcIp, udp_srcPort, dstIp, udp_dstPort, udpPacket.ToString());
}

}*/
    }
    }

