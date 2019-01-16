using System;
using System.IO;
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
        CaptureFileWriterDevice captureFileWriter;

        int packetNumber = 1;
        string time_str = "", sourceIP = "", destinationIP = "", protocol_type = "", length = "";

        bool startCapturingAgain = false;

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

        private void toolStripButton1_Click(object sender, EventArgs e)// Start sniffing
        {
            if(startCapturingAgain == false)
            {
                System.IO.File.Delete(Environment.CurrentDirectory + "capture.pcap");
                backgroundWorker1.RunWorkerAsync();
                toolStripButton1.Enabled = false;
                toolStripButton2.Enabled = true;
                textBox1.Enabled = false;

            }

            if (startCapturingAgain)
            {
                if (MessageBox.Show("Your packets are captured in a file. Starting a new capture will override existing ones.", "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    // user clicked ok
                    System.IO.File.Delete(Environment.CurrentDirectory + "capture.pcap");
                    listView1.Items.Clear();
                    packetNumber = 1;

                    backgroundWorker1.RunWorkerAsync();
                    toolStripButton1.Enabled = false;
                    toolStripButton2.Enabled = true;
                    textBox1.Enabled = false;

                }
            }

            startCapturingAgain = true;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
                //var protocol = listView1.SelectedIndices[0].SubItems[4].Text;
               // Console.WriteLine(protocol.ToString());

            
        }
        // TODO paket information
        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            string protocol = e.Item.SubItems[4].Text;
            switch (protocol)
            {
                case "TCP":
                    break;
                
            }
        }


        private void toolStripButton2_Click(object sender, EventArgs e)// Stop sniffing
        {
            wifi_device.StopCapture();
            wifi_device.Close();
            captureFileWriter.Close();

            backgroundWorker1.CancelAsync();
            toolStripButton1.Enabled = true;
            textBox1.Enabled = true;
            toolStripButton2.Enabled = false;
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
                if(wifi_device.Opened == true)
                {
                    if(textBox1.Text != "")
                    {
                        wifi_device.Filter = textBox1.Text;
                    }
                    captureFileWriter = new CaptureFileWriterDevice(wifi_device, Environment.CurrentDirectory + "capture.pcap");
                    wifi_device.Capture();
                }
                else
                {
                    wifi_device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
                }

            }

            else if (backgroundWorker1.CancellationPending)
            {           
                e.Cancel = true;
                Console.WriteLine("STOPPED");
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                Console.WriteLine("stopped");
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
            // dump to a file
            captureFileWriter.Write(e.Packet);

            DateTime time = e.Packet.Timeval.Date;
                time_str = (time.Hour + 1 ) + ":" + time.Minute + ":" + time.Second + ":" + time.Millisecond;
                length = e.Packet.Data.Length.ToString();


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

                    //Console.WriteLine("{0}  time = {1} Len={2} {3} -> {4}  Protocol = {5}",
                      // packetNumber.ToString(), time_str, length,
                  // sourceIP, destinationIP, protocol_type);

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


