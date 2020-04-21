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

            // Construct the command byte array by using the passed enum command
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

            // Return the whole message byte array
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
                // First byte of the message should be an STX byte representation
                if(Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("2"))
                {
                    index++;

                    var commandLength = (int)dataToProcess.GetRange(index, 1).FirstOrDefault();
                    index++;

                    var command = dataToProcess.GetRange(index, commandLength);
                    index += commandLength;

                    // Last byte of the message should be an ETX byte representation
                    if(Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("3"))
                    {
                        // Decode the first element of the command byte array to deduce what command it is
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
                        Tools.Log("Message does not end with an ETX byte representation!");
                        return false;
                    }
                }
                else
                {
                    Tools.Log("Message does not start with an STX byte representation!");
                    return false;
                }
            }
            catch(Exception e)
            {
                Tools.Log(e.Message);
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
                // Firt byte of the command should be a byte representation of an ASCII 'n'
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

                    // Add transaction to local transaction chain
                    Client.State.AddNetworkTransaction(new Transaction()
                    {
                        Number = number,
                        Time = time,
                        From = from,
                        To = to
                    });

                    // Broadcast an ok message
                    Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.Ok));
                    return true;
                }
                else
                {
                    Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.NotOk));
                    Tools.Log("Command does not start with an ASCII 'n'!");
                    return false;
                }
            }
            catch(Exception e)
            {
                Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.NotOk));
                Tools.Log(e.Message);
                return false;
            }
        }

        private static bool ProcessHighestTransactionMessage(byte[] data)
        {
            var dataToProcess = data.ToList();
            var index = 0;

            try
            {
                // Firt byte of the command should be a byte representation of an ASCII 'h'
                if (Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("h"))
                {
                    index++;

                    // If there are no transactions return 0, otherwise return the last transaction's id
                    Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.HighestTransactionResult, new string[] { Client.State.Transactions.Count > 0 ? Client.State.Transactions.Last().Number.ToString() : "0" }));
                    return true;
                }
                else
                {
                    Tools.Log("Command does not start with an ASCII 'h'!");
                    return false;
                }
            }
            catch(Exception e)
            {
                Tools.Log(e.Message);
                return false;
            }
        }

        private static bool ProcessHighestTransactionResultMessage(byte[] data)
        {
            var dataToProcess = data.ToList();
            var index = 0;

            try
            {
                if(Client.State.CurrentState == States.Syncing || (Client.State.SyncCounter == 0 && Client.State.CurrentState == States.InitializingSyncProcess))
                {
                    // Firt byte of the command should be a byte representation of an ASCII 'm'
                    if (Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("m"))
                    {
                        index++;
                        var highestTransaction = Tools.DecodeUInt16(dataToProcess.GetRange(index, 2).ToArray());

                        Client.State.SyncCounter += 1;
                        Client.State.NetworkTransactionCount = Client.State.NetworkTransactionCount < highestTransaction ? highestTransaction : Client.State.NetworkTransactionCount;

                        // First check if all other nodes has sent their response
                        if(Client.State.SyncCounter == Client.Peers.Nodes.Count)
                        {
                            Client.State.CurrentState = Client.State.IsSynced() ? States.Synchronized : States.NotSynchronized;
                        }
                        return true;
                    }
                    else
                    {
                        Tools.Log("Command does not start with an ASCII 'm'!");
                        return false;
                    }
                }

                return false;
            }
            catch(Exception e)
            {
                Tools.Log(e.Message);
                return false;
            }
        }

        private static bool ProcessGetTransactionMessage(byte[] data)
        {
            var dataToProcess = data.ToList();
            var index = 0;

            try
            {
                // Firt byte of the command should be a byte representation of an ASCII 'g'
                if (Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("g"))
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
                        Tools.Log($"Requested transaction with number {number} does not exist in local chain!");
                        return false;
                    }
                }
                else
                {
                    Tools.Log("Command does not start with an ASCII 'g'!");
                    return false;
                }
            }
            catch(Exception e)
            {
                Tools.Log(e.Message);
                return false;
            }
        }

        #endregion
    }
}