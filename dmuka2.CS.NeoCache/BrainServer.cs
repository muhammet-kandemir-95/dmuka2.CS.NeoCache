using dmuka.Semaphore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace dmuka2.CS.NeoCache
{
    public enum BrainPackageType
    {
        Exit = 0,
        AddANeuron = 1,
        RemoveANeuron = 2,
        RefreshANeuron = 3,
        SetNeuronValue = 4,
        GetNeuronValue = 5,
        SetNeuronValueToNeuron = 6,
        AddANeuronToList = 7,
        RemoveANeuronFromList = 8,
        ClearNeuronList = 9,
        NeuronExists = 10,
        SetOrAddNeuronValue = 11,
        SetOrAddNeuronValueToNeuron = 12,
        AddNeuronValue = 13,
        SubtractNeuronValue = 14,
        MultiplyNeuronValue = 15,
        DivideNeuronValue = 16,
        ModuloNeuronValue = 17,
        AddNeuronValueWithAdd = 18,
        SubtractNeuronValueWithAdd = 19,
        MultiplyNeuronValueWithAdd = 20,
        DivideNeuronValueWithAdd = 21,
        ModuloNeuronValueWithAdd = 22,
        GetOrAddNeuronValue = 23,
        GetNullableNeuronValue = 24
    }

    /// <summary>
    /// Tcp brain server.
    /// </summary>
    public class BrainServer
    {
        #region Variables
        TcpListener tcpListener = null;
        string password = null;
        bool enable = false;
        int readTimeout = 0;
        int coreCount = 0;
        ActionQueue actionQueue = null;
        #endregion

        #region Constructors
        public BrainServer(int coreCount, string password, int port, int readTimeout = 1000)
        {
            this.coreCount = coreCount;
            this.password = password;
            this.tcpListener = new TcpListener(IPAddress.Any, port);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Read buffer from Stream.
        /// </summary>
        /// <param name="arr">Which arr will be filled?</param>
        /// <param name="offset">Data index.</param>
        /// <param name="check">Check argument that will be used on while loop.</param>
        internal static void ReadBuffer(TcpClient tcpClient, Stream stream, byte[] arr, Func<bool> check = null)
        {
            int offset = 0;
            check = check ?? (() => true);
            while (stream.CanRead && ConnectionEnable(tcpClient) && offset < arr.Length && check())
            {
                if (tcpClient.ReceiveBufferSize > 0)
                {
                    int read = stream.Read(arr, offset, arr.Length - offset);
                    offset += read;
                }
            }
        }

        /// <summary>
        /// Tcp connection still is enable?
        /// </summary>
        /// <param name="tcpClient">Connection.</param>
        /// <returns></returns>
        internal static bool ConnectionEnable(TcpClient tcpClient)
        {
            if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
            {
                byte[] buff = new byte[1];
                if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Open the server to listen clients.
        /// </summary>
        public void Open()
        {
            this.OpenWithSSL(null);
        }

        /// <summary>
        /// Open the server with ssl to listen clients.
        /// </summary>
        /// <param name="certificate"></param>
        public void OpenWithSSL(X509Certificate certificate)
        {
            this.enable = true;
            this.actionQueue = new ActionQueue(coreCount);
            this.actionQueue.Start();

            Func<TcpClient, Stream> getStream = (c) => c.GetStream();
            if (certificate != null)
            {
                getStream = (c) =>
                {
                    var ssl = new SslStream(c.GetStream());
                    ssl.AuthenticateAsServer(certificate, false, false);
                    return ssl;
                };
            }

            this.tcpListener.Start();
#if DEBUG
            Console.WriteLine("Server opened...");
#endif

            while (this.enable == true)
            {
                try
                {
                    TcpClient tcpClient = this.tcpListener.AcceptTcpClient();
                    this.actionQueue.AddAction(() =>
                    {
                        runAClient(getStream, tcpClient);
                    });
                }
                catch { }
            }
        }

        private void runAClient(Func<TcpClient, Stream> getStream, TcpClient tcpClient)
        {
            Stream stream = null;
            try
            {
                DateTime startDateTime = DateTime.Now;
                Func<bool> timeoutCheck = () =>
                {
                    var diff = DateTime.Now - startDateTime;
                    return diff.TotalMilliseconds < 1000;
                };
#if DEBUG

                Console.WriteLine("New connection...");
#endif
                startDateTime = DateTime.Now;
                tcpClient.ReceiveTimeout = this.readTimeout;
                stream = getStream(tcpClient);

                var helloMessage = Encoding.UTF8.GetBytes("HELLO");
                stream.Write(helloMessage, 0, helloMessage.Length);

                #region Read Password
                byte[] passwordLen = new byte[1];
                ReadBuffer(tcpClient, stream, passwordLen, timeoutCheck);
                if (timeoutCheck() == false)
                {
                    tcpClient.Close();
                    return;
                }

                byte[] password = new byte[passwordLen[0]];
                ReadBuffer(tcpClient, stream, password, timeoutCheck);
                if (timeoutCheck() == false)
                {
                    tcpClient.Close();
                    return;
                }

                var passwordStr = password.ToStringDC();

                if (passwordStr != this.password)
                {
                    // ER_AUTH
                    stream.Write(new byte[] { 69, 82, 95, 65, 85, 84, 72 }, 0, 7);
#if DEBUG
                    Console.WriteLine("Wrong password! {0}", password);
#endif
                }
#if DEBUG
                Console.WriteLine("Login success...");
#endif
                // OK_AUTH
                stream.Write(new byte[] { 79, 75, 95, 65, 85, 84, 72 }, 0, 7);
                #endregion

                while (stream.CanRead && ConnectionEnable(tcpClient))
                {
                    byte[] package = new byte[1];
                    ReadBuffer(tcpClient, stream, package);

                    // FIRST_BYTE = Package Type
                    var processType = (BrainPackageType)package[0];
                    if (processType == BrainPackageType.Exit)
                        break;
#if DEBUG
                    Console.WriteLine("Processing {0}...", processType.ToString());
#endif
                    byte[] response = new byte[2];
                    switch (processType)
                    {
                        case BrainPackageType.AddANeuron:
                            {
                                // [1,2] BYTE = Key Length
                                // [3] BYTE = Type Length
                                // [<key_length>] = Key Text
                                // [<type_length>] = Type Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);

                                var keyLength = keyLengthArr.ToInt16DC();

                                var typeLengthArr = new byte[1];
                                ReadBuffer(tcpClient, stream, typeLengthArr);
                                var typeLength = typeLengthArr[0];

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var type = new byte[typeLength];
                                ReadBuffer(tcpClient, stream, type);

                                var keyStr = key.ToStringDC();
                                var typeStr = type.ToStringDC();

                                lock (Brain.Neurons)
                                {
                                    Brain.AddANeuron(keyStr, typeStr);
                                }
                            }
                            break;
                        case BrainPackageType.RemoveANeuron:
                            {
                                // [1,2] BYTE = Key Length
                                // [<key_length>] = Key Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var keyStr = key.ToStringDC();

                                lock (Brain.Neurons)
                                {
                                    Brain.RemoveANeuron(keyStr);
                                }
                            }
                            break;
                        case BrainPackageType.RefreshANeuron:
                            {
                                // [1,2] BYTE = Key Length
                                // [3] BYTE = Type Length
                                // [<key_length>] = Key Text
                                // [<type_length>] = Type Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var typeLengthArr = new byte[1];
                                ReadBuffer(tcpClient, stream, typeLengthArr);
                                var typeLength = typeLengthArr[0];

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var type = new byte[typeLength];
                                ReadBuffer(tcpClient, stream, type);

                                var keyStr = key.ToStringDC();
                                var typeStr = type.ToStringDC();

                                lock (Brain.Neurons)
                                {
                                    Brain.RefreshANeuron(keyStr, typeStr);
                                }
                            }
                            break;
                        case BrainPackageType.SetNeuronValue:
                            {
                                // [1,2] BYTE = Key Length
                                // [3,4] BYTE = Path Length
                                // [5,6,7,8] BYTE = Data Length
                                // [<key_length>] = Key Text
                                // [<path_length>] = Path Text
                                // [<data_length>] = Data Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var dataLengthArr = new byte[4];
                                ReadBuffer(tcpClient, stream, dataLengthArr);
                                var dataLength = dataLengthArr.ToInt32DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var data = new byte[dataLength];
                                ReadBuffer(tcpClient, stream, data);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();

                                lock (Brain.Neurons)
                                {
                                    Brain.SetNeuronValue(keyStr, pathStr, data);
                                }
                            }
                            break;
                        case BrainPackageType.AddNeuronValue:
                            {
                                // [1,2] BYTE = Key Length
                                // [3,4] BYTE = Path Length
                                // [5,6,7,8] BYTE = Data Length
                                // [<key_length>] = Key Text
                                // [<path_length>] = Path Text
                                // [<data_length>] = Data Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var dataLengthArr = new byte[4];
                                ReadBuffer(tcpClient, stream, dataLengthArr);
                                var dataLength = dataLengthArr.ToInt32DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var data = new byte[dataLength];
                                ReadBuffer(tcpClient, stream, data);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();
                                byte[] value = null;
                                lock (Brain.Neurons)
                                {
                                    value = Brain.AddNeuronValue(keyStr, pathStr, data);
                                }

                                var valueLen = value.Length.ToByteArrayDC();
                                response = new byte[valueLen.Length + value.Length + 2];
                                Array.Copy(valueLen, 0, response, 2, valueLen.Length);
                                Array.Copy(value, 0, response, 2 + valueLen.Length, value.Length);
                            }
                            break;
                        case BrainPackageType.SubtractNeuronValue:
                            {
                                // [1,2] BYTE = Key Length
                                // [3,4] BYTE = Path Length
                                // [5,6,7,8] BYTE = Data Length
                                // [<key_length>] = Key Text
                                // [<path_length>] = Path Text
                                // [<data_length>] = Data Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var dataLengthArr = new byte[4];
                                ReadBuffer(tcpClient, stream, dataLengthArr);
                                var dataLength = dataLengthArr.ToInt32DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var data = new byte[dataLength];
                                ReadBuffer(tcpClient, stream, data);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();
                                byte[] value = null;
                                lock (Brain.Neurons)
                                {
                                    value = Brain.SubtractNeuronValue(keyStr, pathStr, data);
                                }

                                var valueLen = value.Length.ToByteArrayDC();
                                response = new byte[valueLen.Length + value.Length + 2];
                                Array.Copy(valueLen, 0, response, 2, valueLen.Length);
                                Array.Copy(value, 0, response, 2 + valueLen.Length, value.Length);
                            }
                            break;
                        case BrainPackageType.MultiplyNeuronValue:
                            {
                                // [1,2] BYTE = Key Length
                                // [3,4] BYTE = Path Length
                                // [5,6,7,8] BYTE = Data Length
                                // [<key_length>] = Key Text
                                // [<path_length>] = Path Text
                                // [<data_length>] = Data Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var dataLengthArr = new byte[4];
                                ReadBuffer(tcpClient, stream, dataLengthArr);
                                var dataLength = dataLengthArr.ToInt32DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var data = new byte[dataLength];
                                ReadBuffer(tcpClient, stream, data);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();
                                byte[] value = null;
                                lock (Brain.Neurons)
                                {
                                    value = Brain.MultiplyNeuronValue(keyStr, pathStr, data);
                                }

                                var valueLen = value.Length.ToByteArrayDC();
                                response = new byte[valueLen.Length + value.Length + 2];
                                Array.Copy(valueLen, 0, response, 2, valueLen.Length);
                                Array.Copy(value, 0, response, 2 + valueLen.Length, value.Length);
                            }
                            break;
                        case BrainPackageType.DivideNeuronValue:
                            {
                                // [1,2] BYTE = Key Length
                                // [3,4] BYTE = Path Length
                                // [5,6,7,8] BYTE = Data Length
                                // [<key_length>] = Key Text
                                // [<path_length>] = Path Text
                                // [<data_length>] = Data Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var dataLengthArr = new byte[4];
                                ReadBuffer(tcpClient, stream, dataLengthArr);
                                var dataLength = dataLengthArr.ToInt32DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var data = new byte[dataLength];
                                ReadBuffer(tcpClient, stream, data);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();
                                byte[] value = null;
                                lock (Brain.Neurons)
                                {
                                    value = Brain.DivideNeuronValue(keyStr, pathStr, data);
                                }

                                var valueLen = value.Length.ToByteArrayDC();
                                response = new byte[valueLen.Length + value.Length + 2];
                                Array.Copy(valueLen, 0, response, 2, valueLen.Length);
                                Array.Copy(value, 0, response, 2 + valueLen.Length, value.Length);
                            }
                            break;
                        case BrainPackageType.ModuloNeuronValue:
                            {
                                // [1,2] BYTE = Key Length
                                // [3,4] BYTE = Path Length
                                // [5,6,7,8] BYTE = Data Length
                                // [<key_length>] = Key Text
                                // [<path_length>] = Path Text
                                // [<data_length>] = Data Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var dataLengthArr = new byte[4];
                                ReadBuffer(tcpClient, stream, dataLengthArr);
                                var dataLength = dataLengthArr.ToInt32DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var data = new byte[dataLength];
                                ReadBuffer(tcpClient, stream, data);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();
                                byte[] value = null;
                                lock (Brain.Neurons)
                                {
                                    value = Brain.ModuloNeuronValue(keyStr, pathStr, data);
                                }

                                var valueLen = value.Length.ToByteArrayDC();
                                response = new byte[valueLen.Length + value.Length + 2];
                                Array.Copy(valueLen, 0, response, 2, valueLen.Length);
                                Array.Copy(value, 0, response, 2 + valueLen.Length, value.Length);
                            }
                            break;
                        case BrainPackageType.SetOrAddNeuronValue:
                            {
                                // [1,2] BYTE = Key Length
                                // [3] BYTE = Type Length
                                // [4,5] BYTE = Path Length
                                // [6,7,8,9] BYTE = Data Length
                                // [<key_length>] = Key Text
                                // [<type_length>] = Type Text
                                // [<path_length>] = Path Text
                                // [<data_length>] = Data Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var typeLengthArr = new byte[1];
                                ReadBuffer(tcpClient, stream, typeLengthArr);
                                var typeLength = typeLengthArr[0];

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var dataLengthArr = new byte[4];
                                ReadBuffer(tcpClient, stream, dataLengthArr);
                                var dataLength = dataLengthArr.ToInt32DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var type = new byte[typeLength];
                                ReadBuffer(tcpClient, stream, type);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var data = new byte[dataLength];
                                ReadBuffer(tcpClient, stream, data);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();
                                var typeStr = type.ToStringDC();

                                lock (Brain.Neurons)
                                {
                                    if (Brain.NeuronExists(keyStr) == false)
                                        Brain.AddANeuron(keyStr, typeStr);
                                    Brain.SetNeuronValue(keyStr, pathStr, data);
                                }
                            }
                            break;
                        case BrainPackageType.AddNeuronValueWithAdd:
                            {
                                // [1,2] BYTE = Key Length
                                // [3] BYTE = Type Length
                                // [4,5] BYTE = Path Length
                                // [6,7,8,9] BYTE = Data Length
                                // [<key_length>] = Key Text
                                // [<type_length>] = Type Text
                                // [<path_length>] = Path Text
                                // [<data_length>] = Data Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var typeLengthArr = new byte[1];
                                ReadBuffer(tcpClient, stream, typeLengthArr);
                                var typeLength = typeLengthArr[0];

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var dataLengthArr = new byte[4];
                                ReadBuffer(tcpClient, stream, dataLengthArr);
                                var dataLength = dataLengthArr.ToInt32DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var type = new byte[typeLength];
                                ReadBuffer(tcpClient, stream, type);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var data = new byte[dataLength];
                                ReadBuffer(tcpClient, stream, data);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();
                                var typeStr = type.ToStringDC();

                                byte[] value = null;
                                lock (Brain.Neurons)
                                {
                                    if (Brain.NeuronExists(keyStr) == false)
                                        Brain.AddANeuron(keyStr, typeStr);
                                    value = Brain.AddNeuronValue(keyStr, pathStr, data);
                                }

                                var valueLen = value.Length.ToByteArrayDC();
                                response = new byte[valueLen.Length + value.Length + 2];
                                Array.Copy(valueLen, 0, response, 2, valueLen.Length);
                                Array.Copy(value, 0, response, 2 + valueLen.Length, value.Length);
                            }
                            break;
                        case BrainPackageType.SubtractNeuronValueWithAdd:
                            {
                                // [1,2] BYTE = Key Length
                                // [3] BYTE = Type Length
                                // [4,5] BYTE = Path Length
                                // [6,7,8,9] BYTE = Data Length
                                // [<key_length>] = Key Text
                                // [<type_length>] = Type Text
                                // [<path_length>] = Path Text
                                // [<data_length>] = Data Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var typeLengthArr = new byte[1];
                                ReadBuffer(tcpClient, stream, typeLengthArr);
                                var typeLength = typeLengthArr[0];

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var dataLengthArr = new byte[4];
                                ReadBuffer(tcpClient, stream, dataLengthArr);
                                var dataLength = dataLengthArr.ToInt32DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var type = new byte[typeLength];
                                ReadBuffer(tcpClient, stream, type);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var data = new byte[dataLength];
                                ReadBuffer(tcpClient, stream, data);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();
                                var typeStr = type.ToStringDC();

                                byte[] value = null;
                                lock (Brain.Neurons)
                                {
                                    if (Brain.NeuronExists(keyStr) == false)
                                        Brain.AddANeuron(keyStr, typeStr);
                                    value = Brain.SubtractNeuronValue(keyStr, pathStr, data);
                                }

                                var valueLen = value.Length.ToByteArrayDC();
                                response = new byte[valueLen.Length + value.Length + 2];
                                Array.Copy(valueLen, 0, response, 2, valueLen.Length);
                                Array.Copy(value, 0, response, 2 + valueLen.Length, value.Length);
                            }
                            break;
                        case BrainPackageType.MultiplyNeuronValueWithAdd:
                            {
                                // [1,2] BYTE = Key Length
                                // [3] BYTE = Type Length
                                // [4,5] BYTE = Path Length
                                // [6,7,8,9] BYTE = Data Length
                                // [<key_length>] = Key Text
                                // [<type_length>] = Type Text
                                // [<path_length>] = Path Text
                                // [<data_length>] = Data Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var typeLengthArr = new byte[1];
                                ReadBuffer(tcpClient, stream, typeLengthArr);
                                var typeLength = typeLengthArr[0];

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var dataLengthArr = new byte[4];
                                ReadBuffer(tcpClient, stream, dataLengthArr);
                                var dataLength = dataLengthArr.ToInt32DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var type = new byte[typeLength];
                                ReadBuffer(tcpClient, stream, type);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var data = new byte[dataLength];
                                ReadBuffer(tcpClient, stream, data);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();
                                var typeStr = type.ToStringDC();

                                byte[] value = null;
                                lock (Brain.Neurons)
                                {
                                    if (Brain.NeuronExists(keyStr) == false)
                                        Brain.AddANeuron(keyStr, typeStr);
                                    value = Brain.MultiplyNeuronValue(keyStr, pathStr, data);
                                }

                                var valueLen = value.Length.ToByteArrayDC();
                                response = new byte[valueLen.Length + value.Length + 2];
                                Array.Copy(valueLen, 0, response, 2, valueLen.Length);
                                Array.Copy(value, 0, response, 2 + valueLen.Length, value.Length);
                            }
                            break;
                        case BrainPackageType.DivideNeuronValueWithAdd:
                            {
                                // [1,2] BYTE = Key Length
                                // [3] BYTE = Type Length
                                // [4,5] BYTE = Path Length
                                // [6,7,8,9] BYTE = Data Length
                                // [<key_length>] = Key Text
                                // [<type_length>] = Type Text
                                // [<path_length>] = Path Text
                                // [<data_length>] = Data Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var typeLengthArr = new byte[1];
                                ReadBuffer(tcpClient, stream, typeLengthArr);
                                var typeLength = typeLengthArr[0];

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var dataLengthArr = new byte[4];
                                ReadBuffer(tcpClient, stream, dataLengthArr);
                                var dataLength = dataLengthArr.ToInt32DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var type = new byte[typeLength];
                                ReadBuffer(tcpClient, stream, type);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var data = new byte[dataLength];
                                ReadBuffer(tcpClient, stream, data);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();
                                var typeStr = type.ToStringDC();

                                byte[] value = null;
                                lock (Brain.Neurons)
                                {
                                    if (Brain.NeuronExists(keyStr) == false)
                                        Brain.AddANeuron(keyStr, typeStr);
                                    value = Brain.DivideNeuronValue(keyStr, pathStr, data);
                                }

                                var valueLen = value.Length.ToByteArrayDC();
                                response = new byte[valueLen.Length + value.Length + 2];
                                Array.Copy(valueLen, 0, response, 2, valueLen.Length);
                                Array.Copy(value, 0, response, 2 + valueLen.Length, value.Length);
                            }
                            break;
                        case BrainPackageType.ModuloNeuronValueWithAdd:
                            {
                                // [1,2] BYTE = Key Length
                                // [3] BYTE = Type Length
                                // [4,5] BYTE = Path Length
                                // [6,7,8,9] BYTE = Data Length
                                // [<key_length>] = Key Text
                                // [<type_length>] = Type Text
                                // [<path_length>] = Path Text
                                // [<data_length>] = Data Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var typeLengthArr = new byte[1];
                                ReadBuffer(tcpClient, stream, typeLengthArr);
                                var typeLength = typeLengthArr[0];

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var dataLengthArr = new byte[4];
                                ReadBuffer(tcpClient, stream, dataLengthArr);
                                var dataLength = dataLengthArr.ToInt32DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var type = new byte[typeLength];
                                ReadBuffer(tcpClient, stream, type);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var data = new byte[dataLength];
                                ReadBuffer(tcpClient, stream, data);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();
                                var typeStr = type.ToStringDC();

                                byte[] value = null;
                                lock (Brain.Neurons)
                                {
                                    if (Brain.NeuronExists(keyStr) == false)
                                        Brain.AddANeuron(keyStr, typeStr);
                                    value = Brain.ModuloNeuronValue(keyStr, pathStr, data);
                                }

                                var valueLen = value.Length.ToByteArrayDC();
                                response = new byte[valueLen.Length + value.Length + 2];
                                Array.Copy(valueLen, 0, response, 2, valueLen.Length);
                                Array.Copy(value, 0, response, 2 + valueLen.Length, value.Length);
                            }
                            break;
                        case BrainPackageType.GetNeuronValue:
                            {
                                // [1,2] BYTE = Key Length
                                // [3,4] BYTE = Path Length
                                // [<key_length>] = Key Text
                                // [<path_length>] = Path Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();

                                byte[] value = null;
                                lock (Brain.Neurons)
                                {
                                    value = Brain.GetNeuronValue(keyStr, pathStr);
                                }

                                var valueLen = value.Length.ToByteArrayDC();
                                response = new byte[valueLen.Length + value.Length + 2];
                                Array.Copy(valueLen, 0, response, 2, valueLen.Length);
                                Array.Copy(value, 0, response, 2 + valueLen.Length, value.Length);
                            }
                            break;
                        case BrainPackageType.GetOrAddNeuronValue:
                            {
                                // [1,2] BYTE = Key Length
                                // [3] BYTE = Type Length
                                // [4,5] BYTE = Path Length
                                // [<key_length>] = Key Text
                                // [<type_length>] = Type Text
                                // [<path_length>] = Path Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var typeLengthArr = new byte[1];
                                ReadBuffer(tcpClient, stream, typeLengthArr);
                                var typeLength = typeLengthArr[0];

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var type = new byte[typeLength];
                                ReadBuffer(tcpClient, stream, type);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var keyStr = key.ToStringDC();
                                var typeStr = type.ToStringDC();
                                var pathStr = path.ToStringDC();

                                byte[] value = null;
                                lock (Brain.Neurons)
                                {
                                    if (Brain.NeuronExists(keyStr) == false)
                                        Brain.AddANeuron(keyStr, typeStr);
                                    value = Brain.GetNeuronValue(keyStr, pathStr);
                                }

                                var valueLen = value.Length.ToByteArrayDC();
                                response = new byte[valueLen.Length + value.Length + 2];
                                Array.Copy(valueLen, 0, response, 2, valueLen.Length);
                                Array.Copy(value, 0, response, 2 + valueLen.Length, value.Length);
                            }
                            break;
                        case BrainPackageType.GetNullableNeuronValue:
                            {
                                // [1,2] BYTE = Key Length
                                // [3,4] BYTE = Path Length
                                // [<key_length>] = Key Text
                                // [<path_length>] = Path Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();
                                byte[] value = new byte[0];

                                lock (Brain.Neurons)
                                {
                                    if (Brain.NeuronExists(keyStr) == true)
                                        value = Brain.GetNeuronValue(keyStr, pathStr);
                                }

                                var valueLen = value.Length.ToByteArrayDC();
                                response = new byte[valueLen.Length + value.Length + 2];
                                Array.Copy(valueLen, 0, response, 2, valueLen.Length);
                                Array.Copy(value, 0, response, 2 + valueLen.Length, value.Length);
                            }
                            break;
                        case BrainPackageType.SetNeuronValueToNeuron:
                            {
                                // [1,2] BYTE = Key Length
                                // [3,4] BYTE = Path Length
                                // [5,6] BYTE = Neuron Length
                                // [<key_length>] = Key Text
                                // [<path_length>] = Path Text
                                // [<neuron_length>] = Neuron Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var neuronLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, neuronLengthArr);
                                var neuronLength = neuronLengthArr.ToInt16DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var neuron = new byte[neuronLength];
                                ReadBuffer(tcpClient, stream, neuron);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();
                                var neuronStr = neuron.ToStringDC();

                                lock (Brain.Neurons)
                                {
                                    Brain.SetNeuronValueToNeuron(keyStr, pathStr, neuronStr);
                                }
                            }
                            break;
                        case BrainPackageType.SetOrAddNeuronValueToNeuron:
                            {
                                // [1,2] BYTE = Key Length
                                // [3] BYTE = Type Length
                                // [4,5] BYTE = Path Length
                                // [6,7] BYTE = Neuron Length
                                // [<key_length>] = Key Text
                                // [<path_length>] = Path Text
                                // [<neuron_length>] = Neuron Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var typeLengthArr = new byte[1];
                                ReadBuffer(tcpClient, stream, typeLengthArr);
                                var typeLength = typeLengthArr[0];

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var neuronLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, neuronLengthArr);
                                var neuronLength = neuronLengthArr.ToInt16DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var type = new byte[typeLength];
                                ReadBuffer(tcpClient, stream, type);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var neuron = new byte[neuronLength];
                                ReadBuffer(tcpClient, stream, neuron);

                                var keyStr = key.ToStringDC();
                                var typeStr = type.ToStringDC();
                                var pathStr = path.ToStringDC();
                                var neuronStr = neuron.ToStringDC();

                                lock (Brain.Neurons)
                                {
                                    if (Brain.NeuronExists(keyStr) == false)
                                        Brain.AddANeuron(keyStr, typeStr);
                                    Brain.SetNeuronValueToNeuron(keyStr, pathStr, neuronStr);
                                }
                            }
                            break;
                        case BrainPackageType.AddANeuronToList:
                            {
                                // [1,2] BYTE = Key Length
                                // [3,4] BYTE = Path Length
                                // [<key_length>] = Key Text
                                // [<path_length>] = path Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();

                                lock (Brain.Neurons)
                                {
                                    Brain.AddANeuronToList(keyStr, pathStr);
                                }
                            }
                            break;
                        case BrainPackageType.RemoveANeuronFromList:
                            {
                                // [1,2] BYTE = Key Length
                                // [3,4] BYTE = Path Length
                                // [5,6,7,8] BYTE = Neuron Index
                                // [<key_length>] = Key Text
                                // [<path_length>] = Path Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var neuronIndexArr = new byte[4];
                                ReadBuffer(tcpClient, stream, neuronIndexArr);
                                var neuronIndex = neuronIndexArr.ToInt32DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();

                                lock (Brain.Neurons)
                                {
                                    Brain.RemoveANeuronFromList(keyStr, pathStr, neuronIndex);
                                }
                            }
                            break;
                        case BrainPackageType.ClearNeuronList:
                            {
                                // [1,2] BYTE = Key Length
                                // [3,4] BYTE = Path Length
                                // [<key_length>] = Key Text
                                // [<path_length>] = Path Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var pathLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, pathLengthArr);
                                var pathLength = pathLengthArr.ToInt16DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var path = new byte[pathLength];
                                ReadBuffer(tcpClient, stream, path);

                                var keyStr = key.ToStringDC();
                                var pathStr = path.ToStringDC();

                                lock (Brain.Neurons)
                                {
                                    Brain.ClearNeuronList(keyStr, pathStr);
                                }
                            }
                            break;
                        case BrainPackageType.NeuronExists:
                            {
                                // [1,2] BYTE = Key Length
                                // [<key_length>] = Key Text
                                var keyLengthArr = new byte[2];
                                ReadBuffer(tcpClient, stream, keyLengthArr);
                                var keyLength = keyLengthArr.ToInt16DC();

                                var key = new byte[keyLength];
                                ReadBuffer(tcpClient, stream, key);

                                var keyStr = key.ToStringDC();
                                var result = false;

                                lock (Brain.Neurons)
                                {
                                    result = Brain.NeuronExists(keyStr);
                                }

                                response = new byte[1 + 2];
                                response[2] = result == true ? (byte)1 : (byte)0;
                            }
                            break;
                        default:
                            break;
                    }

                    if (stream.CanWrite)
                    {
                        // OK
                        response[0] = 79;
                        response[1] = 75;
                        stream.Write(response, 0, response.Length);
                    }
                }

                tcpClient.Close();
                tcpClient.Dispose();
            }
            catch (Exception ex)
            {
                try
                {
                    if (stream != null)
                    {
                        // ER
                        stream.Write(new byte[] { 69, 82 }, 0, 2);
                    }
                }
                catch
                { }

                try
                {
                    if (tcpClient != null)
                        tcpClient.Dispose();
                }
                catch
                { }

                Console.WriteLine(
                    "**********************" +
                    Environment.NewLine +
                    ex.ToString() +
                    Environment.NewLine +
                    "**********************"
                    );
            }
        }

        /// <summary>
        /// Close server.
        /// </summary>
        public void Close()
        {
            this.enable = false;
            this.tcpListener.Stop();
            this.actionQueue.Dispose();
        }
        #endregion
    }
}
