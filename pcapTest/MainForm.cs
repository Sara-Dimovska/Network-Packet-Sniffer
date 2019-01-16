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
        Dictionary<int, Packet> capturedPackets_list = new Dictionary<int, Packet>();

        int packetNumber = 1;
        string time_str = "", sourceIP = "", destinationIP = "", protocol_type = "", length = "";

        bool startCapturingAgain = false;
        bool bgProcess = false;

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
            if(startCapturingAgain == false) //first time 
            {
                System.IO.File.Delete(Environment.CurrentDirectory + "capture.pcap");
                bgProcess = true;
                backgroundWorker1.RunWorkerAsync();
                toolStripButton1.Enabled = false;
                toolStripButton2.Enabled = true;
                textBox1.Enabled = false;

            }
           else if (startCapturingAgain)
            {
                if (MessageBox.Show("Your packets are captured in a file. Starting a new capture will override existing ones.", "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    // user clicked ok
                    System.IO.File.Delete(Environment.CurrentDirectory + "capture.pcap");
                    listView1.Items.Clear();
                    capturedPackets_list.Clear();
                    packetNumber = 1;
                    textBox2.Text = "";
                    bgProcess = true;
                    backgroundWorker1.RunWorkerAsync();
                    toolStripButton1.Enabled = false;
                    toolStripButton2.Enabled = true;
                    textBox1.Enabled = false;

                }
            }
            startCapturingAgain = true;
        }

        // TODO paket information
        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            string protocol = e.Item.SubItems[4].Text;
            int key = Int32.Parse(e.Item.SubItems[0].Text);
            Packet packet;
            bool getPacket  = capturedPackets_list.TryGetValue(key, out packet);

                switch (protocol) {
                case "TCP":
                    if(getPacket)
                    {
                        var tcpPacket = (TcpPacket)packet.Extract(typeof(TcpPacket));
                        if (tcpPacket != null)
                        {
                            int srcPort = tcpPacket.SourcePort;
                            int dstPort = tcpPacket.DestinationPort;
                            var checksum = tcpPacket.Checksum;

                            textBox2.Text = "";
                            textBox2.Text = "Packet number: " + key +
                                            " Type: TCP" +
                                            "\r\nSource port:" + srcPort +
                                            "\r\nDestination port: " + dstPort +
                                            "\r\nTCP header size: " + tcpPacket.DataOffset +
                                            "\r\nChecksum:" + checksum.ToString() + (tcpPacket.ValidChecksum ? ",valid" : ",invalid") +
                                            "\r\nTCP checksum: " + (tcpPacket.ValidTCPChecksum ? ",valid" : ",invalid") +
                                            "\r\nSequence number: " + tcpPacket.SequenceNumber.ToString() +
                                            "\r\nAcknowledgment number: " + tcpPacket.AcknowledgmentNumber + (tcpPacket.Ack ? ",valid" : ",invalid") +
                                            "\r\nUrgent pointer: " + (tcpPacket.Urg ? "valid" : "invalid") +
                                            "\r\nPSH flag: " + (tcpPacket.Psh ? "1" : "0") +
                                            "\r\nRST flag: " + (tcpPacket.Rst ? "1" : "0") +
                                            "\r\nSYN flag: " + (tcpPacket.Syn ? "1" : "0") +
                                            "\r\nFIN flag: " + (tcpPacket.Fin ? "1" : "0") +
                                            "\r\nECN flag: " + (tcpPacket.ECN ? "1" : "0") +
                                            "\r\nCWR flag: " + (tcpPacket.CWR ? "1" : "0") +
                                            "\r\nNS flag: " + (tcpPacket.NS ? "1" : "0");
                        }
                    }
                    break;
                case "UDP":
                    if (getPacket)
                    {
                        var udpPacket = (UdpPacket)packet.Extract(typeof(UdpPacket));
                        if (udpPacket != null)
                        {
                            int srcPort = udpPacket.SourcePort;
                            int dstPort = udpPacket.DestinationPort;
                            var checksum = udpPacket.Checksum;

                            textBox2.Text = "";
                            textBox2.Text = "Packet number: " + key +
                                            " Type: UDP" +
                                            "\r\nSource port:" + srcPort +
                                            "\r\nDestination port: " + dstPort +
                                            "\r\nChecksum:" + checksum.ToString() + " valid: " + udpPacket.ValidChecksum +
                                            "\r\nValid UDP checksum: " + udpPacket.ValidUDPChecksum;
                        }
                    }
                    break;
                case "ARP":
                    if (getPacket)
                    {
                        var arpPacket = (ARPPacket)packet.Extract(typeof(ARPPacket));
                        if (arpPacket != null)
                        {
                            System.Net.IPAddress senderAddress = arpPacket.SenderProtocolAddress;
                            System.Net.IPAddress targerAddress = arpPacket.TargetProtocolAddress;
                            System.Net.NetworkInformation.PhysicalAddress senderHardwareAddress = arpPacket.SenderHardwareAddress;
                            System.Net.NetworkInformation.PhysicalAddress targerHardwareAddress = arpPacket.TargetHardwareAddress;

                            textBox2.Text = "";
                            textBox2.Text = "Packet number: " + key +
                                            " Type: ARP" +
                                            "\r\nHardware address length:" + arpPacket.HardwareAddressLength +
                                            "\r\nProtocol address length: " + arpPacket.ProtocolAddressLength +
                                            "\r\nOperation: " + arpPacket.Operation.ToString() +
                                            "\r\nSender protocol address: " + senderAddress +
                                            "\r\nTarget protocol address: " + targerAddress +
                                            "\r\nSender hardware address: " + senderHardwareAddress +
                                            "\r\nTarget hardware address: " + targerHardwareAddress;
                        }
                    }
                    break;
                default:
                    textBox2.Text = "";
                    break;
                }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)// last packet
        {
            var items = listView1.Items;
            var last = items[items.Count - 1];
            last.EnsureVisible();
            last.Selected = true;
        }

        private void toolStripButton5_Click(object sender, EventArgs e)// fist packet
        {
            var first = listView1.Items[0];
            first.EnsureVisible();
            first.Selected = true;
        }

        private void toolStripButton4_Click(object sender, EventArgs e)//next
        {
            if(listView1.SelectedItems.Count == 1)
            {
                int index = listView1.SelectedItems[0].Index;
                listView1.Items[index + 1].Selected = true;
                listView1.Items[index + 1].EnsureVisible();
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)// prev
        {
            if (listView1.SelectedItems.Count == 1)
            {
                int index = listView1.SelectedItems[0].Index;
                listView1.Items[index - 1].Selected = true;
                listView1.Items[index - 1].EnsureVisible();
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)// Stop sniffing
        {
            bgProcess = false;
            backgroundWorker1.CancelAsync();
            wifi_device.StopCapture();
            wifi_device.Close();
            captureFileWriter.Close();

            toolStripButton1.Enabled = true;
            textBox1.Enabled = true;
            toolStripButton2.Enabled = false;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!backgroundWorker1.CancellationPending)
            {
                wifi_device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);

                // Open the device for capturing
                int readTimeoutMilliseconds = 1000;
                wifi_device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

                // Start the capturing process
                if(wifi_device.Opened && bgProcess)
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
                   // wifi_device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
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
            

            // start extracting properties for the listview q
            DateTime time = e.Packet.Timeval.Date;
                time_str = (time.Hour + 1 ) + ":" + time.Minute + ":" + time.Second + ":" + time.Millisecond;
                length = e.Packet.Data.Length.ToString();


                var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

                // add to the list
                capturedPackets_list.Add(packetNumber, packet);

      
            var ipPacket = (IpPacket)packet.Extract(typeof(IpPacket));
               

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

                    /*
                    switch (protocol_type)
                    {
                        case "TCP":
                        break;

                        case "UDP":
                            break;

                        case "TCP":
                        break;
                    }*/

                Action action = () => listView1.Items.Add(item);
                    listView1.Invoke(action);
            
                    ++packetNumber;
                }
        }
    }
}


