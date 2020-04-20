using System;
using System.Linq;
using System.Threading;
using WorstBlockchainEver.Helper;
using WorstBlockchainEver.Models;

namespace WorstBlockchainEver
{
    public class Protocol
    {
        public static byte[] CreateMessage(Commands command, string[] args = null)
        {
            byte[] commandBytes = null;

            switch (command)
            {
                case Commands.NewTransaction:
                    {
                        commandBytes = CreateNewTransactionMessage(new Transaction()
                        {
                            Number = Convert.ToUInt16(args[0]),
                            Time = Convert.ToInt64(args[1]),
                            From = args[2],
                            To = args[3]
                        });
                        break;
                    }

                case Commands.HighestTransaction:
                    {
                        commandBytes = CreateHighestTransactionMessage();
                        break;
                    }

                case Commands.HighestTransactionResult:
                    {
                        commandBytes = CreateHighestTransactionResultMessage(Convert.ToUInt16(args[0]));
                        break;
                    }

                case Commands.GetTransaction:
                    {
                        commandBytes = CreateGetTransactionMessage(Convert.ToUInt16(args[0]));
                        break;
                    }

                case Commands.Ok:
                    {
                        commandBytes = CreateOkMessage();
                        break;
                    }

                case Commands.NotOk:
                    {
                        commandBytes = CreateNotOkMessage();
                        break;
                    }
            }

            return Tools.Encode("2")
                .Append((byte)commandBytes.Length)
                .Concat(commandBytes)
                .Concat(Tools.Encode("3"))
                .ToArray();
        }

        public static bool ProcessMessage(byte[] data)
        {
            var dataToProcess = data.ToList();
            var index = 0;

            try
            {
                if(Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("2"))
                {
                    index++;

                    var commandLength = (int)dataToProcess.GetRange(index, 1).FirstOrDefault();
                    index++;

                    var command = dataToProcess.GetRange(index, commandLength);
                    index += commandLength;

                    if(Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("3"))
                    {

                        switch(Tools.DecodeString(command.GetRange(0, 1).ToArray()))
                        {
                            case "n": return ProcessNewTransactionMessage(command.ToArray());

                            case "h": return ProcessHighestTransactionMessage(command.ToArray());

                            case "m": return ProcessHighestTransactionResultMessage(command.ToArray());

                            case "g": return ProcessGetTransactionMessage(command.ToArray());

                            case "o": return true;

                            case "f": return true;

                            default: return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        #region Create Messages

        private static byte[] CreateNewTransactionMessage(Transaction transaction)
        {
            return Tools.Encode("n")
                .Concat(Tools.Encode(transaction.Number))
                .Concat(Tools.Encode(transaction.Time))
                .Concat(Tools.Encode(transaction.From))
                .Concat(Tools.Encode(transaction.To))
                .ToArray();
        }

        private static byte[] CreateHighestTransactionMessage()
        {
            return Tools.Encode("h");
        }

        private static byte[] CreateHighestTransactionResultMessage(ushort highestTransaction)
        {
            return Tools.Encode("m")
                .Concat(Tools.Encode(highestTransaction))
                .ToArray();
        }

        private static byte[] CreateGetTransactionMessage(ushort transaction)
        {
            return Tools.Encode("g")
                .Concat(Tools.Encode(transaction))
                .ToArray();
        }

        private static byte[] CreateOkMessage()
        {
            return Tools.Encode("o");
        }

        private static byte[] CreateNotOkMessage()
        {
            return Tools.Encode("f");
        }

        #endregion

        #region Process Messages

        private static bool ProcessNewTransactionMessage(byte[] data)
        {
            var dataToProcess = data.ToList();
            var index = 0;

            try
            {
                if (Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("n"))
                {
                    index++;

                    var number = Tools.DecodeUInt16(dataToProcess.GetRange(index, 2).ToArray());
                    index += 2;

                    var time = Tools.DecodeInt64(dataToProcess.GetRange(index, 8).ToArray());
                    index += 8;

                    var from = Tools.DecodeString(dataToProcess.GetRange(index, 2).ToArray());
                    index += 2;

                    var to = Tools.DecodeString(dataToProcess.GetRange(index, 2).ToArray());
                    index += 2;

                    Client.State.AddNetworkTransaction(new Transaction()
                    {
                        Number = number,
                        Time = time,
                        From = from,
                        To = to
                    });

                    Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.Ok));
                    return true;
                }
                else
                {
                    Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.NotOk));
                    return false;
                }
            }
            catch
            {
                Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.NotOk));
                return false;
            }
        }

        private static bool ProcessHighestTransactionMessage(byte[] data)
        {
            var dataToProcess = data.ToList();
            var index = 0;

            try
            {
                if (Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("h"))
                {
                    index++;

                    Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.HighestTransactionResult, new string[] { Client.State.Transactions.Count.ToString() }));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool ProcessHighestTransactionResultMessage(byte[] data)
        {
            var dataToProcess = data.ToList();
            var index = 0;

            try
            {
                if (Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("m"))
                {
                    index++;

                    // Assume that current node is not synced
                    Client.State.Synchronized = false;

                    // Check if highest transaction number that node sent is bigger than the number we have stored in memory
                    var highestTransaction = Tools.DecodeUInt16(dataToProcess.GetRange(index, 2).ToArray());
                    if(highestTransaction > Client.State.Transactions.Count)
                    {
                        // Set the syncing state to true
                        Client.State.Syncing = true;

                        // Get all transactions that are not present locally by asking other nodes for them
                        for (int i = Client.State.Transactions.Count; i <= highestTransaction; i++)
                        {
                            Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.GetTransaction, new string[] { i.ToString() }));
                        }

                        // Set the syncing state to false
                        Client.State.Syncing = false;
                    }

                    // At this point node is synced
                    Client.State.Synchronized = true;

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool ProcessGetTransactionMessage(byte[] data)
        {
            var dataToProcess = data.ToList();
            var index = 0;

            try
            {
                if (Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("m"))
                {
                    index++;

                    // Extract requested transaction number from the message
                    var number = Tools.DecodeUInt16(dataToProcess.GetRange(index, 2).ToArray());

                    // Check if we have the transaction being requested
                    var transaction = Client.State.Transactions.Where(t => t.Number == number).FirstOrDefault();
                    if(transaction != null)
                    {
                        // If the transaction exists, then broadcast it to the network
                        Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.NewTransaction, new string[]
                        {
                            transaction.Number.ToString(),
                            transaction.Time.ToString(),
                            transaction.From,
                            transaction.To
                        }));

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}