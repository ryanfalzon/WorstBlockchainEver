using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using WorstBlockchainEver.Helper;
using WorstBlockchainEver.Interfaces;
using WorstBlockchainEver.Models;

namespace WorstBlockchainEver
{
    public class Peers : IPeers
    {
        public readonly Node Me;

        public readonly List<Node> Nodes;

        public Dictionary<int, bool> NodeSyncStatus;

        public Peers(Node me)
        {
            this.Me = me;
            this.Nodes = new List<Node>();
            this.NodeSyncStatus = new Dictionary<int, bool>();
        }

        public void InitPeers()
        {
            this.LoadSeedNodes();
        }

        public void LoadSeedNodes()
        {
            using(StringReader stringReader = new StringReader(Properties.Resources.SeedNodes))
            using (TextFieldParser parser = new TextFieldParser(stringReader))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                if (!parser.EndOfData)
                {
                    parser.ReadLine();
                }

                int idCounter = 0;
                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    var ipAddress = IPAddress.Parse(fields[0]);
                    var port = Convert.ToInt32(fields[1]);

                    if (!this.Me.IPAddress.Equals(ipAddress) || !this.Me.Port.Equals(port))
                    {
                        this.Nodes.Add(new Node()
                        {
                            Id = idCounter,
                            IPAddress = ipAddress,
                            Port = port
                        });
                        this.NodeSyncStatus.Add(idCounter, false);

                        idCounter++;
                    }
                }
            }
        }

        public void DiscoverOtherNodes()
        {
            throw new NotImplementedException();
        }

        public void BroadcastMessage(byte[] message)
        {
            using (var client = new UdpClient())
            {
                foreach (var node in this.Nodes)
                {
                    try
                    {
                        IPEndPoint endpoint = new IPEndPoint(node.IPAddress, node.Port);
                        client.Connect(endpoint);
                        client.Send(message, message.Length);
                    }
                    catch (Exception e)
                    {
                        Tools.Log($"Error occured while broadcasting message to {node.IPAddress}:{node.Port}...");
                        Tools.Log($"{e.Message}");
                    }
                }
            }
        }

        public void ProcessIncomingMessages()
        {
            using (UdpClient server = new UdpClient(this.Me.Port))
            {
                IPEndPoint listenEndPoint = new IPEndPoint(IPAddress.Any, this.Me.Port);
                while (true)
                {
                    byte[] receivedData = server.Receive(ref listenEndPoint);

                    // Try and process the message and if it fails log an error
                    if (!Protocol.ProcessMessage(receivedData))
                    {
                        Tools.Log("An error has occured while proessing received message");
                    }
                }
            }
        }
    }
}