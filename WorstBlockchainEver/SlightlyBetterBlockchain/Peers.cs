using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using SlightlyBetterBlockchain.Helper;
using SlightlyBetterBlockchain.Models;
using System.Collections.Concurrent;

namespace SlightlyBetterBlockchain
{
    public class Peers
    {
        public readonly Node Me;

        public readonly List<Node> Nodes;

        public ConcurrentQueue<byte[]> Messages;

        public Peers(Node me)
        {
            this.Me = me;
            this.Nodes = new List<Node>();
            this.Messages = new ConcurrentQueue<byte[]>();
        }

        public void InitPeers()
        {
            this.LoadSeedNodes();
        }

        public void LoadSeedNodes()
        {
            using (StringReader stringReader = new StringReader(Properties.Settings.Default.LocalDevelopment ? Properties.Resources.LocalSeedNodes : Properties.Resources.LiveSeedNodes))
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
                    var on = Convert.ToBoolean(fields[2]);

                    if (on && (!this.Me.IPAddress.Equals(ipAddress) || !this.Me.Port.Equals(port)))
                    {
                        this.Nodes.Add(new Node()
                        {
                            Id = idCounter,
                            IPAddress = ipAddress,
                            Port = port
                        });

                        Tools.Log($"Found node at {ipAddress}:{port}");

                        idCounter++;
                    }
                }
            }
        }

        public void DiscoverOtherNodes()
        {
            throw new NotImplementedException();
        }

        public void BroadcastMessages()
        {
            using (var client = new UdpClient())
            {
                while (true)
                {
                    this.Messages.TryDequeue(out byte[] message);

                    if(message != null)
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

                    Protocol.ProcessMessage(receivedData);
                }
            }
        }
    }
}