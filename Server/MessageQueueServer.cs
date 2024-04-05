using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Shared.Messages;
using Type = Shared.Messages.Type;

namespace Server;

public class MessageQueueServer
{
    private static MessageQueueServer mInstance;
    private bool mKeepRunning = true;
    private Thread mThreadListener;
    private Thread mThreadSender;
    private Thread mThreadReceiver;
    private Socket mListener;
    private Dictionary<int, Socket> mClients = new();
    private Mutex mMutexSockets = new();

    private ConcurrentQueue<Tuple<int, uint?, Message>> mSendMessages = new();
    private ManualResetEvent mEventSendMessages = new(false);
    private Mutex mMutexSendMessages = new();

    private Queue<Tuple<int, Message>> mReceivedMessages = new();
    private Mutex mMutexReceivedMessages = new();
    private Dictionary<int, uint> mClientLastMessageId = new();

    private delegate Message TypeToMessage();

    private Dictionary<Shared.Messages.Type, TypeToMessage> mMessageCommand = new();

    public delegate void ConnectHandler(int clientId);
    public delegate void DisconnectHandler(int clientId);

    private ConnectHandler mConnectHandler;
    private DisconnectHandler mDisconnectHandler;

    public static MessageQueueServer instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new MessageQueueServer();
            }

            return mInstance;
        }
    }

    /// <summary>
    /// Create three threads for the message queue:
    ///  1. Listen for incoming client connections
    ///  2. Listen for incoming messages
    ///  3. Sending of messages
    /// </summary>
    public bool initialize(ushort port)
    {
        mMessageCommand[Type.Join] = () => { return new Join(); };
        mMessageCommand[Type.Input] = () => { return new Input(); };
        mMessageCommand[Type.Disconnect] = () => { return new Disconnect(); };

        initializeListener(port);
        initializeSender();
        initializeReceiver();

        return true;
    }

    /// <summary>
    /// Gracefully shut things down
    /// </summary>
    public void shutdown()
    {
        mKeepRunning = false;
        mEventSendMessages.Set();
        foreach (var socket in mClients.Values)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Disconnect(false);
            socket.Close();
        }
        mListener.Close();
    }

    /// <summary>
    /// Two steps in sending a message:
    ///  1. Add the message the the message queue
    ///  2. Signal the thread that performs the sending that a new message is available
    /// </summary>
    public void sendMessage(int clientId, Message message, uint? messageId = null)
    {
        // The reason messageId is part of the tuple rather than directly setting it on
        // the message at this point is that broadcast messages are not copies, but we
        // want each message that goes out to a client to have the unique per-client
        // last message sequence number attached to it.  Right before the message
        // is sent in the sender thread, the messageId is set on the message, ensure
        // the correct sequence number is sent to the client.
        mSendMessages.Enqueue(Tuple.Create(clientId, messageId, message));
        mEventSendMessages.Set();
    }

    /// <summary>
    /// Some messages go back to the client with the id of the last message
    /// processed by the server.  This is for use in client-side server
    /// reconciliation.
    /// </summary>
    public void sendMessageWithLastId(int clientId, Message message)
    {
        sendMessage(clientId, message, mClientLastMessageId[clientId]);
    }
    
    /// <summary>
    /// Send the message to all connected clients.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="ignore"></param>
    public void broadcastMessage(Message message)
    {
        lock (mMutexSockets)
        {
            foreach (var clientId in mClients.Keys)
            {
               sendMessage(clientId, message);
            }
        }
    }

    /// <summary>
    /// Send the message to all connected clients, but also including the
    /// last message sequence number processed by the server.
    /// </summary>
    public void broadcastMessageWithLastId(Message message)
    {
        lock (mMutexSockets)
        {
            foreach (var clientId in mClients.Keys)
            {
                sendMessageWithLastId(clientId, message);
            }
        }
    }

    /// <summary>
    /// Returns the queue of all messages received since the last time
    /// this method was called.
    /// </summary>
    public Queue<Tuple<int, Message>>? getMessages() {
        if (mReceivedMessages.Count == 0)
        {
            return null;
        }

        var empty = new Queue<Tuple<int, Message>>();
        var previous = mReceivedMessages;

        lock (mMutexReceivedMessages)
        {
            mReceivedMessages = empty;
        }

        return previous;
    }

    public void registerConnectHandler(ConnectHandler handler)
    {
        mConnectHandler = handler;
    }

    /// <summary>
    /// Listen for incoming client connections.  As a connection is made
    /// remember it and begin listening for messages over that socket.
    /// </summary>
    private void initializeListener(ushort port)
    {
        // Get the ip address of the host (the machine running this program)
        var ipHost = Dns.GetHostEntry(Dns.GetHostName());
        var ipAddress = IPAddress.IPv6Any;
        var endPoint = new IPEndPoint(ipAddress, port);

        // Make a listener socket on this host and bind it to the endpoint
        // SocketType.Stream is the type we use for TCP
        mListener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        mListener.Bind(endPoint);
        
        // Set the socket to listen, with a waiting queue of up to 10 connections
        mListener.Listen(10);

        mThreadListener = new Thread(() =>
        {
            while (mKeepRunning)
            {
                var client = mListener.Accept();
                lock (mListener)
                {
                    lock (mMutexSockets)
                    {
                        mClients.Add(client.GetHashCode(), client);
                    }

                    Console.WriteLine($"Client connected from {client.RemoteEndPoint}");
                }

                mConnectHandler(client.GetHashCode());
            }
        });
        mThreadListener.Start();
    }

    /// <summary>
    /// Prepares the message queue for sending of messages.  As messages
    /// are added to the queue of messages to send, the thread created
    /// in this method sends them as soon as it can.
    /// </summary>
    private void initializeSender()
    {
        mThreadSender = new Thread(() =>
        {
            var remove = new List<int>();
            while (mKeepRunning)
            {
                if (mSendMessages.TryDequeue(out var item))
                {
                    // Some messages have a sequence number associated with them, if they do,
                    // then set it on the message.
                    if (item.Item2.HasValue)
                    {
                        item.Item3.messageId = item.Item2.Value;
                    }

                    lock (mMutexSockets)
                    {
                        if (mClients.ContainsKey(item.Item1))
                        {
                            try
                            {
                                if (item.Item3.type != Type.NewEntity) Console.WriteLine($"Sending message {item.Item3.type} to {item.Item1}");
                                // Three items are sent; type, size, and body
                                var type = BitConverter.GetBytes((UInt16)item.Item3.type);
                                var body = item.Item3.serialize();
                                var size = BitConverter.GetBytes(body.Length);

                                // Type
                                if (BitConverter.IsLittleEndian)
                                {
                                    Array.Reverse(type);
                                }

                                mClients[item.Item1].Send(type);

                                // Size
                                if (BitConverter.IsLittleEndian)
                                {
                                    Array.Reverse(size);
                                }

                                mClients[item.Item1].Send(size);

                                // Message body
                                mClients[item.Item1].Send(body);
                            }
                            catch (SocketException)
                            {
                                Console.WriteLine($"Client {item.Item1.GetHashCode()} disconnected");
                                mClients[item.Item1].Disconnect(false);
                                mClients[item.Item1].Close();
                                remove.Add(item.Item1);
                            }
                        }
                    }

                    lock (mMutexSockets)
                    {
                        foreach (var clientId in remove)
                        {
                            mClients.Remove(clientId);
                            mClientLastMessageId.Remove(clientId);
                        }
                        remove.Clear();
                    }
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

    /// <summary>
    /// Sets up a thread that listens for incoming messages on all
    /// known client sockets.  If there is something to receive on a
    /// socket, the message is read, parsed, and added to the queue
    /// of received messages.
    /// </summary>
    private void initializeReceiver()
    {
        mThreadReceiver = new Thread(() =>
        {
            var type = new byte[sizeof(Type)];
            var size = new byte[sizeof(int)];

            var remove = new List<int>();
            // This is a busy loop, would be nice to efficiently wait for an incoming message
            //       from a client
            while (mKeepRunning)
            {
                lock (mListener)
                {
                    lock (mMutexSockets)
                    {
                        foreach (var client in mClients)
                        {
                            try
                            {
                                if (client.Value.Connected && client.Value.Available > 0)
                                {
                                    // Read the type first
                                    client.Value.Receive(type);
                                    if (BitConverter.IsLittleEndian)
                                    {
                                        Array.Reverse(type);
                                    }

                                    // Read the size of the message body
                                    client.Value.Receive(size);
                                    if (BitConverter.IsLittleEndian)
                                    {
                                        Array.Reverse(size);
                                    }

                                    // Read the message body
                                    var body = new byte[BitConverter.ToInt32(size)];
                                    client.Value.Receive(body);

                                    // Deserialize the bytes into the actual message
                                    var message = mMessageCommand[(Type)BitConverter.ToUInt16(type)]();
                                    message.parse(body);
                                    if (message.messageId.HasValue)
                                    {
                                        mClientLastMessageId[client.Key] = message.messageId.Value;
                                    }
                                    lock (mMutexReceivedMessages)
                                    {
                                        mReceivedMessages.Enqueue(new Tuple<int, Message>(client.Key, message));
                                    }

                                    // Console.WriteLine($"Received message {message.type} from {client.Key}");
                                }

                            }
                            catch (SocketException)
                            {
                                Console.WriteLine($"Client {client.Key} disconnected - here");
                                remove.Add(client.Key);
                            }
                        }
                    }

                    lock (mMutexSockets)
                    {
                        foreach (var clientId in remove)
                        {
                            mClients.Remove(clientId);
                        }
                    }

                    // Have to do this in a different scope from the mutex lock above
                    // because the disconnect handler calls back into the message queue
                    // to broadcast messages.  This is bad, bad, because it means this code
                    // knows something about the implementation of the disconnect handler.
                    // I'll keep thinking about this to find a better overall solution.
                    foreach (var clientId in remove)
                    {
                        mDisconnectHandler(clientId);
                    }
                    remove.Clear();
                }
            }
        });
        mThreadReceiver.Start();
    }
}