using System;
using System.Linq;
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

            return Tools.Encode(0x02)
                .Append(Convert.ToByte(commandBytes.Length))
                .Concat(commandBytes)
                .Concat(Tools.Encode(0x03))
                .ToArray();
        }

        public static bool ProcessMessage(byte[] data)
        {
            var dataToProcess = data.ToList();
            var index = 0;

            try
            {
                if(Tools.DecodeUInt16(dataToProcess.GetRange(index, 1).ToArray()) == 0x02)
                {
                    index++;

                    var commandLength = Tools.DecodeUInt16(dataToProcess.GetRange(0, 1).ToArray());
                    index++;

                    var command = dataToProcess.GetRange(index, commandLength);
                    index += commandLength;

                    if(Tools.DecodeUInt16(dataToProcess.GetRange(index, 1).ToArray()) == 0x03)
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

                    var time = Tools.DecodeInt64(dataToProcess.GetRange(index, 4).ToArray());
                    index += 4;

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

        private static bool ProcessHighestTransactionMessage(byte[] data)
        {
            var dataToProcess = data.ToList();
            var index = 0;

            try
            {
                if (Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("h"))
                {
                    index++;

                    Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.HighestTransactionResult, new string[] { State.Transactions.Count.ToString() }));
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

                    var highestTransaction = Tools.DecodeUInt16(dataToProcess.GetRange(index, 2).ToArray());
                    if(highestTransaction > State.Transactions.Count)
                    {
                        for(int i = State.Transactions.Count; i <= highestTransaction; i++)
                        {
                            Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.GetTransaction, new string[] { i.ToString() }));
                        }
                    }

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

                    var number = Tools.DecodeUInt16(dataToProcess.GetRange(index, 2).ToArray());

                    var transaction = State.Transactions.Where(t => t.Number == number).FirstOrDefault();
                    if(transaction != null)
                    {
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