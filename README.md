# Network-Packet-Sniffer
Packet sniffer in C# using SharpPcap - A Packet Capture Framework for .NET 

Packet capturing (or packet sniffing) is the process of collecting all packets of data that pass through a given network interface. Capturing network packets in our applications is a powerful capability which lets us write network monitoring, packet analyzers and security tools. The libpcap library for UNIX based systems and WinPcap for Windows are the most widely used packet capture drivers that provide API for low-level network monitoring. Among the applications that use libpcap/WinPcap as its packet capture subsystem are the famous tcpdump and Wireshark.

The purpose of SharpPcap(WinPcap wrapper) is to provide a framework for capturing, injecting and analyzing network packets for .NET applications.

This is a simple network packet sniffer application that demostrates:
  - Obtaining the device list
  - Opening an adapter
  - Capturing packets
  - Filtering the traffic
  - Writing packets to a capture file 
  - Interpreting the packets

![Screenshot_2](https://raw.githubusercontent.com/t3mpv4r/Network-Packet-Sniffer/master/screenshots/Screenshot_2.png)

![Screenshot_3](https://raw.githubusercontent.com/t3mpv4r/Network-Packet-Sniffer/master/screenshots/Screenshot_3.png)
