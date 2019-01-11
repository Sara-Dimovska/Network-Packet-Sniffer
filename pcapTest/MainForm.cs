using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using SharpPcap.LibPcap;
using SharpPcap;
using PacketDotNet;


namespace pcapTest
{
    public partial class MainForm : Form
    {
        List<LibPcapLiveDevice> interfaceList = new List<LibPcapLiveDevice>();
        int selectedIntIndex;
        LibPcapLiveDevice wifi_device;

        int packetNumber = 1;
        string time_str = "", sourceIP = "", destinationIP = "", protocol_type = "", length = "";


        public MainForm(List<LibPcapLiveDevice> interfaces, int selectedIndex)
        {
            InitializeComponent();
            this.interfaceList = interfaces;
            selectedIntIndex = selectedIndex;
            // Extract a device from the list
            wifi_device = interfaceList[selectedIntIndex];

            backgroundWorker1.WorkerSupportsCancellation = true;

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e) // START
        {
           
        }

        private void toolStripButton1_Click(object sender, EventArgs e)// Start sniffing
        {
            backgroundWorker1.RunWorkerAsync();
            toolStripButton1.Enabled = false;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)// Stop sniffing
        {
            // Stop the capturing process
            wifi_device.StopCapture();

            // Close the pcap device
            wifi_device.Close();
            //label1.Text = "Start sniffing";

            backgroundWorker1.CancelAsync();
            toolStripButton1.Enabled = true;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!backgroundWorker1.CancellationPending)
            {
                wifi_device.OnPacketArrival +=
                         new PacketArrivalEventHandler(Device_OnPacketArrival);

                // Open the device for capturing
                int readTimeoutMilliseconds = 1000;
                wifi_device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

                // Console.WriteLine("-- Listening on {0} -- ",
                //  wifi_device.Interface.FriendlyName);

                // Start the capturing process
                wifi_device.Capture();

            }

            else if (backgroundWorker1.CancellationPending)
            {           
                e.Cancel = true;
                Console.WriteLine("stopped");
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {

            }
            else if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                //
            }
        }

        public void Device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
               DateTime time = e.Packet.Timeval.Date;
                time_str = time.Hour + ":" + time.Minute + ":" + time.Second + ":" + time.Millisecond;
                length = e.Packet.Data.Length.ToString();

                // number time source dest protocol len

                var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);


                //var tcpPacket = (TcpPacket)packet.Extract(typeof(TcpPacket));
                var ipPacket = (IpPacket)packet.Extract(typeof(IpPacket));
                //var udpPacket = (UdpPacket)packet.Extract(typeof(UdpPacket));

                if (ipPacket != null)
                {
                    System.Net.IPAddress srcIp = ipPacket.SourceAddress;
                    System.Net.IPAddress dstIp = ipPacket.DestinationAddress;
                    protocol_type = ipPacket.Protocol.ToString();
                    sourceIP = srcIp.ToString();
                    destinationIP = dstIp.ToString();



                    var protocolPacket = ipPacket.PayloadPacket;

                    ListViewItem item = new ListViewItem(packetNumber.ToString());
                    item.SubItems.Add(time_str);
                    item.SubItems.Add(sourceIP);
                    item.SubItems.Add(destinationIP);
                    item.SubItems.Add(protocol_type);
                    item.SubItems.Add(length);

                    Action action = () => listView1.Items.Add(item);
                    listView1.Invoke(action);

                    Console.WriteLine("{0}  time = {1} Len={2} {3} -> {4}  Protocol = {5}",
                       packetNumber.ToString(), time_str, length,
                  sourceIP, destinationIP, protocol_type);

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
                    }*/
                    packetNumber++;
                }

            

        }
    }
}


