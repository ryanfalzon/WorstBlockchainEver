using Newtonsoft.Json;
using SlightlyBetterBlockchain.Helper;
using SlightlyBetterBlockchain.Models;
using SlightlyBetterBlockchain.Models.Enums;
using SlightlyBetterBlockchain.Models.ProtocolObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace SlightlyBetterBlockchain
{
    public class Protocol
    {
        public static byte[] CreateMessage(Commands command, object @object = null)
        {
            byte[] commandBytes = null;

            // Construct the command byte array by using the passed enum command
            switch (command)
            {
                case Commands.GetCount:
                    {
                        commandBytes = GetCountMessage();
                        break;
                    }

                case Commands.Count:
                    {
                        commandBytes = CountMessage();
                        break;
                    }

                case Commands.GetBlockHashes:
                    {
                        commandBytes = GetBlockHashesMessage();
                        break;
                    }

                case Commands.BlockHashes:
                    {
                        commandBytes = BlockHashesMessage();
                        break;
                    }

                case Commands.RequestBlock:
                    {
                        commandBytes = RequestBlockMessage(Convert.ToString(@object));
                        break;
                    }

                case Commands.Block:
                    {
                        commandBytes = BlockMessage((Block)@object);
                        break;
                    }

                case Commands.NewBlock:
                    {
                        commandBytes = NewBlockMessage((Block)@object);
                        break;
                    }
            }

            // Return the whole message byte array
            return new byte[] { Tools.EncodeChar('2') }
                .Concat(Tools.Encode(Convert.ToUInt16(commandBytes.Length)))
                .Concat(commandBytes)
                .Append(Tools.EncodeChar('3'))
                .ToArray();
        }

        public static void ProcessMessage(byte[] data)
        {
            var dataToProcess = data.ToList();
            var index = 0;

            try
            {
                // First byte of the message should be an STX byte representation
                if (Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("2"))
                {
                    index++;

                    var commandLength = Tools.DecodeUInt16(dataToProcess.GetRange(index, 2).ToArray());
                    index += 2;

                    var command = dataToProcess.GetRange(index, commandLength);
                    index += commandLength;

                    // Last byte of the message should be an ETX byte representation
                    if (Tools.DecodeString(dataToProcess.GetRange(index, 1).ToArray()).Equals("3"))
                    {
                        // Decode the first element of the command byte array to deduce what command it is
                        Task.Factory.StartNew(() =>
                        {
                            switch (Tools.DecodeString(command.GetRange(0, 1).ToArray()))
                            {
                                case "a": ProcessGetCountMessage(); break;

                                case "c": ProcessCountMessage(command.GetRange(1, commandLength - 1).ToArray()); break;

                                case "b": ProcessGetBlockHashesMessage(); break;

                                case "h": ProcessBlockHashesMessage(command.GetRange(1, commandLength - 1).ToArray()); break;

                                case "r": ProcessRequestBlockMessage(command.GetRange(1, commandLength - 1).ToArray()); break;

                                case "x": ProcessBlockMessage(command.GetRange(1, commandLength - 1).ToArray()); break;

                                case "z": ProcessNewBlockMessage(command.GetRange(1, commandLength - 1).ToArray()); break;

                                default: throw new InvalidOperationException("Invalid command received!");
                            }
                        });
                    }
                    else
                    {
                        Tools.Log("Message does not end with an ETX byte representation!");
                    }
                }
                else
                {
                    Tools.Log("Message does not start with an STX byte representation!");
                }
            }
            catch (Exception e)
            {
                Tools.Log(e.Message);
            }
        }

        #region Create Messages

        private static byte[] GetCountMessage()
        {
            return new byte[] { OperationIdentifiers.GetCount }
                .Concat(Tools.EncodeAscii(string.Empty))
                .ToArray();
        }

        private static byte[] CountMessage()
        {
            CountObject @object = new CountObject()
            {
                Blocks = Client.Chain.Blocks.Count
            };

            return new byte[] { OperationIdentifiers.Count }
                .Concat(Tools.EncodeAscii(JsonConvert.SerializeObject(@object)))
                .ToArray();
        }

        private static byte[] GetBlockHashesMessage()
        {
            return new byte[] { OperationIdentifiers.GetBlockHashes }
                .Concat(Tools.EncodeAscii(string.Empty))
                .ToArray();
        }

        private static byte[] BlockHashesMessage()
        {
            BlockHashesObject @object = new BlockHashesObject()
            {
                Hashes = Client.Chain.Blocks.Select(block => block.Hash).ToList()
            };

            return new byte[] { OperationIdentifiers.BlockHashes }
                .Concat(Tools.EncodeAscii(JsonConvert.SerializeObject(@object)))
                .ToArray();
        }

        private static byte[] RequestBlockMessage(string hash)
        {
            RequestBlockObject @object = new RequestBlockObject()
            {
                Hash = hash
            };

            return new byte[] { OperationIdentifiers.RequestBlock }
                .Concat(Tools.EncodeAscii(JsonConvert.SerializeObject(@object)))
                .ToArray();
        }

        private static byte[] BlockMessage(Block block)
        {
            BlockObject @object = new BlockObject()
            {
                Block = block
            };

            return new byte[] { OperationIdentifiers.Block }
                .Concat(Tools.EncodeAscii(JsonConvert.SerializeObject(@object)))
                .ToArray();
        }

        private static byte[] NewBlockMessage(Block block)
        {
            NewBlockObject @object = new NewBlockObject()
            {
                Block = block
            };

            return new byte[] { OperationIdentifiers.NewBlock }
                .Concat(Tools.EncodeAscii(JsonConvert.SerializeObject(@object)))
                .ToArray();
        }

        #endregion

        #region Process Messages

        private static void ProcessGetCountMessage()
        {
            StateAwaiter.Await(States.Mining);

            Tools.Log("Processing get count message...");

            try
            {
                Client.Peers.Messages.Enqueue(CreateMessage(Commands.Count));
            }
            catch(Exception e)
            {
                Tools.Log($"Error while processing get count message...\n{e.Message}");
            }
        }

        private static void ProcessCountMessage(byte[] data)
        {
            StateAwaiter.Await(States.Mining);

            Tools.Log("Processing count message...");

            try
            {
                CountObject @object = JsonConvert.DeserializeObject<CountObject>(Tools.DecodeString(data));

                // Check if block count is greater than local chain length
                if(Client.Chain.Blocks.Count < @object.Blocks)
                {
                    Client.State = States.GetBlockHashes;
                    Client.Peers.Messages.Enqueue(CreateMessage(Commands.GetBlockHashes));
                }
            }
            catch (Exception e)
            {
                Tools.Log($"Error while processing count message...\n{e.Message}");
            }
        }

        private static void ProcessGetBlockHashesMessage()
        {
            StateAwaiter.Await(States.Mining);

            Tools.Log("Processing get block hashes message...");

            try
            {
                Client.Peers.Messages.Enqueue(CreateMessage(Commands.BlockHashes));
            }
            catch (Exception e)
            {
                Tools.Log($"Error while processing get block hashes message...\n{e.Message}");
            }
        }

        private static void ProcessBlockHashesMessage(byte[] data)
        {
            StateAwaiter.Await(States.GetBlockHashes);

            Tools.Log("Processing block hashes message...");

            try
            {
                BlockHashesObject @object = JsonConvert.DeserializeObject<BlockHashesObject>(Tools.DecodeString(data));
                if (@object.Hashes.Count > Client.Chain.Blocks.Count)
                {
                    Client.State = States.GetBlocks;

                    // Enqueue all missing blocks in a concurrent queue
                    Client.Chain.Temporary = new List<Block>();
                    foreach (var hash in @object.Hashes)
                    {
                        if(!Client.Chain.Blocks.Exists(block => block.Hash.Equals(hash)))
                        {
                            Client.Chain.MissingBlocks.Enqueue(hash);
                        }
                    }

                    Client.Chain.MissingBlocks.TryDequeue(out string requestNextBlock);
                    if(requestNextBlock != null)
                    {
                        Client.Peers.Messages.Enqueue(CreateMessage(Commands.RequestBlock, requestNextBlock));
                    }
                }
            }
            catch (Exception e)
            {
                Tools.Log($"Error while processing block hashes message...\n{e.Message}");
            }
        }

        private static void ProcessRequestBlockMessage(byte[] data)
        {
            StateAwaiter.Await(States.Mining);

            Tools.Log("Processing request block message...");

            try
            {
                var block = Client.Chain.Blocks.Where(b => b.Hash.Equals(JsonConvert.DeserializeObject<RequestBlockObject>(Tools.DecodeString(data)).Hash)).FirstOrDefault();
                if(block != null)
                {
                    Client.Peers.Messages.Enqueue(CreateMessage(Commands.Block, block));
                }
            }
            catch (Exception e)
            {
                Tools.Log($"Error while processing request block message...\n{e.Message}");
            }
        }

        private static void ProcessBlockMessage(byte[] data)
        {
            StateAwaiter.Await(States.GetBlocks);

            Tools.Log("Processing block message...");

            try
            {
                BlockObject @object = JsonConvert.DeserializeObject<BlockObject>(Tools.DecodeString(data));

                // Validate the block
                bool isBlockValid = @object.Block.Hash.Equals(Tools.ToHex(Tools.GetSha256Hash(Tools.EncodeUtf8(JsonConvert.SerializeObject(@object.Block.HashedContent)))));
                if (isBlockValid)
                {
                    Client.Chain.TryAddBlock(@object.Block);
                }

                Client.Chain.MissingBlocks.TryDequeue(out string requestNextBlock);
                if (requestNextBlock != null)
                {
                    Client.Peers.Messages.Enqueue(CreateMessage(Commands.RequestBlock, requestNextBlock));
                }
                else
                {
                    // Merge temporary chain with main chain
                    Client.Chain.MergeTempChain();
                    Client.State = States.Mining;
                }
            }
            catch (Exception e)
            {
                Tools.Log($"Error while processing block message...\n{e.Message}");
            }
        }

        private static void ProcessNewBlockMessage(byte[] data)
        {
            StateAwaiter.Await(States.Mining);

            Tools.Log("Processing new block message...");

            try
            {
                NewBlockObject @object = JsonConvert.DeserializeObject<NewBlockObject>(Tools.DecodeString(data));
                if (Client.Chain.CurrentBlock == null || Client.Chain.CurrentBlock.Hash.Equals(@object.Block.HashedContent.PreviousBlockHash))
                {
                    Client.Chain.AddBlock(@object.Block);
                }
                else
                {
                    Tools.Log("Rejecting received block since previous block hash does not match current block hash!");
                }
            }
            catch (Exception e)
            {
                Tools.Log($"Error while processing new block message...\n{e.Message}");
            }
        }

        #endregion
    }
}