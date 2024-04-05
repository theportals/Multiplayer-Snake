using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Shared.Messages;
using Type = Shared.Messages.Type;

namespace Client;

public class MessageQueueClient
{
    private static MessageQueueClient mInstance;
    private bool mKeepRunning = true;
    private Thread mThreadSender;
    private Thread mThreadReceiver;
    private Socket mSocketServer;

    private Mutex mMutexSendMessages = new();
    private ManualResetEvent mEventSendMessages = new(false);
    private ConcurrentQueue<Message> mSendMessages = new();
    private Queue<Message> mSendHistory = new();
    private uint mNextMessageId = 0;

    private Queue<Message> mReceivedMessages = new();
    private Mutex mMutexReceivedMessages = new();

    private delegate Message TypeToMessage();
    private Dictionary<Shared.Messages.Type, TypeToMessage> mMessageCommand = new();

    public static MessageQueueClient instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new MessageQueueClient();
            }
            return mInstance;
        }
    }

    public bool initialize(string address, ushort port)
    {
        mKeepRunning = true;
        var ipAddress = parseIPAddress(address);
        var endpoint = new IPEndPoint(ipAddress, port);

        mSocketServer = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        mMessageCommand[Type.ConnectAck] = () =>
        {
            Console.WriteLine("Connect message received");
            return new ConnectAck();
        };
        mMessageCommand[Type.NewEntity] = () =>
        {
            // Console.WriteLine("NewEntity message received");
            return new NewEntity();
        };
        mMessageCommand[Type.UpdateEntity] = () =>
        {
            Console.WriteLine("UpdateEntity message received");
            return new UpdateEntity();
        };
        mMessageCommand[Type.RemoveEntity] = () =>
        {
            Console.WriteLine("RemoveEntity message received");
            return new RemoveEntity();
        };
        mMessageCommand[Type.Join] = () => { 
            Console.WriteLine("Join message received");
            return new Join();
        };
        mMessageCommand[Type.Input] = () => {
            Console.WriteLine("Input message received");
            return new Shared.Messages.Input();
        };
        mMessageCommand[Type.Disconnect] = () => { 
            Console.WriteLine("Disconnect message received");
            return new Disconnect();
        };

        try
        {
            mSocketServer.Connect(endpoint);
            initializeSender();
            initializeReceiver();
        }
        catch (SocketException)
        {
            return false;
        }

        return true;
    }

    public void shutdown()
    {
        mKeepRunning = false;
        mEventSendMessages.Set();
        mSocketServer.Shutdown(SocketShutdown.Both);
        mSocketServer.Disconnect(false);
        mSocketServer.Close();
    }

    public void sendMessage(Message message)
    {
        mSendMessages.Enqueue(message);
        mEventSendMessages.Set();
    }

    public void sendMessageWidthId(Message message)
    {
        message.messageId = mNextMessageId++;
        sendMessage(message);
    }

    public Queue<Message>? getMessages()
    {
        if (mReceivedMessages.Count == 0)
        {
            return null;
        }

        var empty = new Queue<Message>();
        var previous = mReceivedMessages;

        lock (mMutexReceivedMessages)
        {
            mReceivedMessages = empty;
        }

        return previous;
    }

    public Queue<Message> getSendMessageHistory(uint lastMessageId)
    {
        lock (mSendHistory)
        {
            while (mSendHistory.Count > 0 && mSendHistory.Peek().messageId.Value <= lastMessageId)
            {
                mSendHistory.Dequeue();
            }

            return new Queue<Message>(mSendHistory);
        }
    }

    private void initializeSender()
    {
        mThreadSender = new Thread(() =>
        {
            while (mKeepRunning)
            {
                if (mSendMessages.TryDequeue(out var item))
                {
                    // Need to track messages with a sequence number for server reconciliation
                    if (item.messageId.HasValue)
                    {
                        lock (mSendHistory)
                        {
                            mSendHistory.Enqueue(item);
                        }
                    }
                    
                    // Three items are sent: type, size, message vody
                    var type = BitConverter.GetBytes((UInt16)item.type);
                    var body = item.serialize();
                    var size = BitConverter.GetBytes(body.Length);
                    
                    // Type
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(type);
                    }
                    mSocketServer.Send(type);
                    
                    // Size
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(size);
                    }
                    mSocketServer.Send(size);
                    
                    // Body
                    mSocketServer.Send(body);
                }
                else
                {
                    lock (mMutexSendMessages)
                    {
                        mEventSendMessages.WaitOne();
                        mEventSendMessages.Reset();
                    }
                }
            }
        });
        mThreadSender.Start();
    }

    private void initializeReceiver()
    {
        mThreadReceiver = new Thread(() =>
        {
            var type = new byte[sizeof(Shared.Messages.Type)];
            var size = new byte[sizeof(int)];

            mSocketServer.ReceiveTimeout = 100; // Milliseconds

            while (mKeepRunning)
            {
                try
                {
                    var bytesReceived = mSocketServer.Receive(type);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(type);
                    }

                    if (bytesReceived > 0)
                    {
                        mSocketServer.Receive(size);
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(size);
                        }

                        // Read body
                        var body = new byte[BitConverter.ToInt32(size)];
                        mSocketServer.Receive(body);
                        // Deserialize into actual message
                        Message message;
                        Shared.Messages.Type mtype;
                        mtype = (Shared.Messages.Type)BitConverter.ToUInt16(type);
                        try
                        {
                            message = mMessageCommand[(Shared.Messages.Type)BitConverter.ToUInt16(type)]();
                            message.parse(body);
                        }
                        catch (KeyNotFoundException)
                        {
                            Console.WriteLine($"Could not find command for message type {(Shared.Messages.Type)BitConverter.ToUInt16(type)}");
                            continue;
                        }

                        lock (mMutexReceivedMessages)
                        {
                            mReceivedMessages.Enqueue(message);
                        }
                    }
                }
                catch (SocketException)
                {
                    // Expected when a timeout occurs
                }
            }
        });
        mThreadReceiver.Start();
    }

    private static IPAddress parseIPAddress(string address)
    {
        IPAddress ipAddress;
        if (address == "localhost")
        {
            var ipHost = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHost.AddressList[0];
        }
        else
        {
            ipAddress = IPAddress.Parse(address);
        }

        return ipAddress;
    }
}