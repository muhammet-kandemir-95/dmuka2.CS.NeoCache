using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace dmuka2.CS.NeoCache
{
    /// <summary>
    /// You connect any brain server using this clas via TCP.
    /// </summary>
    public class BrainClient : IDisposable
    {
        #region Variables
        TcpClient client = null;
        Stream stream = null;
        #endregion

        #region Constructors
        #endregion

        #region Methods
        /// <summary>
        /// Open connection.
        /// </summary>
        /// <param name="host">Host name.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="password">Security password.</param>
        /// <param name="ssl">Do you want to use ssl?</param>
        /// <param name="sslTargetHost">What is the ssl target host?</param>
        public void Open(string host, int port, string password, bool ssl = false, string sslTargetHost = null, int readTimeout = 0)
        {
            this.client = new TcpClient();
            this.client.ReceiveTimeout = readTimeout;
            this.client.Connect(host, port);
            this.stream = this.client.GetStream();

            if (ssl == true)
            {
                var sslStream = new SslStream(this.stream);
                sslStream.AuthenticateAsClient(sslTargetHost);
                this.stream = sslStream;
            }

            var helloMessage = new byte[5];
            BrainServer.ReadBuffer(this.client, this.stream, helloMessage);

            if (Encoding.UTF8.GetString(helloMessage) != "HELLO")
            {
                this.client.Close();
                throw new Exception("Couldn't get HELLO!");
            }
            
            var passwordArr = password.ToByteArrayDC();
            byte[] passwordData = new byte[1 + passwordArr.Length];
            passwordData[0] = (byte)passwordArr.Length;
            Array.Copy(passwordArr, 0, passwordData, 1, passwordArr.Length);
            stream.Write(passwordData, 0, passwordData.Length);

            var okAuth = new byte[7];
            BrainServer.ReadBuffer(this.client, this.stream, okAuth);
            if (Encoding.UTF8.GetString(okAuth) != "OK_AUTH")
            {
                this.client.Close();
                throw new Exception("Authorization error!");
            }
        }

        /// <summary>
        /// Close the connection.
        /// </summary>
        public void Close()
        {
            if (this.client != null)
                stream.Write(new byte[] { (byte)BrainPackageType.Exit }, 0, 1);
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.client != null)
            {
                this.client.Close();
                this.client.Dispose();
            }
            this.client = null;
        }

        #region Server Methods
        /// <summary>
        /// Message worked?
        /// </summary>
        private void checkOkMessage()
        {
            var okMessage = new byte[2];
            BrainServer.ReadBuffer(this.client, this.stream, okMessage);
            if (okMessage[0] != 79 || okMessage[1] != 75)
                throw new Exception("Response wrong!");
        }

        /// <summary>
        /// We are adding a new neuron to brain.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron class name.</param>
        public void AddANeuron(string key, string type)
        {
            var keyArr = key.ToByteArrayDC();
            var typeArr = type.ToByteArrayDC();
            
            byte[] data = new byte[1 + 2 + 1 + keyArr.Length + typeArr.Length];
            data[0] = (byte)BrainPackageType.AddANeuron;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            data[1 + 2] = (byte)typeArr.Length;
            Array.Copy(keyArr, 0, data, 1 + 2 + 1, keyArr.Length);
            Array.Copy(typeArr, 0, data, 1 + 2 + 1 + keyArr.Length, typeArr.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();
        }

        /// <summary>
        /// We are removing a neuron from brain.
        /// </summary>
        /// <param name="key">Data ID.</param>
        public void RemoveANeuron(string key)
        {
            var keyArr = key.ToByteArrayDC();
            
            byte[] data = new byte[1 + 2 + keyArr.Length];
            data[0] = (byte)BrainPackageType.RemoveANeuron;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(keyArr, 0, data, 1 + 2, keyArr.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();
        }

        /// <summary>
        /// We are remove a neuron from brain and then creating a new neuron instead of it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron class name.</param>
        public void RefreshANeuron(string key, string type)
        {
            var keyArr = key.ToByteArrayDC();
            var typeArr = type.ToByteArrayDC();
            
            byte[] data = new byte[1 + 2 + 1 + keyArr.Length + typeArr.Length];
            data[0] = (byte)BrainPackageType.RefreshANeuron;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            data[1 + 2] = (byte)typeArr.Length;
            Array.Copy(keyArr, 0, data, 1 + 2 + 1, keyArr.Length);
            Array.Copy(typeArr, 0, data, 1 + 2 + 1 + keyArr.Length, typeArr.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetNeuronValue(string key, string path, string value)
        {
            this.SetNeuronValue(key, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetNeuronValue(string key, string path, byte value)
        {
            this.SetNeuronValue(key, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetNeuronValue(string key, string path, sbyte value)
        {
            this.SetNeuronValue(key, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetNeuronValue(string key, string path, short value)
        {
            this.SetNeuronValue(key, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetNeuronValue(string key, string path, ushort value)
        {
            this.SetNeuronValue(key, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetNeuronValue(string key, string path, int value)
        {
            this.SetNeuronValue(key, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetNeuronValue(string key, string path, uint value)
        {
            this.SetNeuronValue(key, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetNeuronValue(string key, string path, long value)
        {
            this.SetNeuronValue(key, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetNeuronValue(string key, string path, ulong value)
        {
            this.SetNeuronValue(key, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetNeuronValue(string key, string path, float value)
        {
            this.SetNeuronValue(key, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetNeuronValue(string key, string path, double value)
        {
            this.SetNeuronValue(key, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetNeuronValue(string key, string path, decimal value)
        {
            this.SetNeuronValue(key, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetNeuronValue(string key, string path, byte[] value)
        {
            var keyArr = key.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 2 + 4 + keyArr.Length + pathArr.Length + value.Length];
            data[0] = (byte)BrainPackageType.SetNeuronValue;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 2, 2);
            Array.Copy(value.Length.ToByteArrayDC(), 0, data, 1 + 2 + 2, 4);
            Array.Copy(keyArr, 0, data, 1 + 2 + 2 + 4, keyArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 2 + 2 + 4 + keyArr.Length, pathArr.Length);
            Array.Copy(value, 0, data, 1 + 2 + 2 + 4 + keyArr.Length + pathArr.Length, value.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public string AddNeuronValue(string key, string path, string value)
        {
            return this.AddNeuronValue(key, path, value.ToByteArrayDC()).ToStringDC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte AddNeuronValue(string key, string path, byte value)
        {
            return this.AddNeuronValue(key, path, value.ToByteArrayDC()).ToByteDC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public sbyte AddNeuronValue(string key, string path, sbyte value)
        {
            return this.AddNeuronValue(key, path, value.ToByteArrayDC()).ToSByteDC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public short AddNeuronValue(string key, string path, short value)
        {
            return this.AddNeuronValue(key, path, value.ToByteArrayDC()).ToInt16DC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ushort AddNeuronValue(string key, string path, ushort value)
        {
            return this.AddNeuronValue(key, path, value.ToByteArrayDC()).ToUInt16DC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public int AddNeuronValue(string key, string path, int value)
        {
            return this.AddNeuronValue(key, path, value.ToByteArrayDC()).ToInt32DC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public uint AddNeuronValue(string key, string path, uint value)
        {
            return this.AddNeuronValue(key, path, value.ToByteArrayDC()).ToUInt32DC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public long AddNeuronValue(string key, string path, long value)
        {
            return this.AddNeuronValue(key, path, value.ToByteArrayDC()).ToInt64DC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ulong AddNeuronValue(string key, string path, ulong value)
        {
            return this.AddNeuronValue(key, path, value.ToByteArrayDC()).ToUInt64DC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public float AddNeuronValue(string key, string path, float value)
        {
            return this.AddNeuronValue(key, path, value.ToByteArrayDC()).ToSingleDC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public double AddNeuronValue(string key, string path, double value)
        {
            return this.AddNeuronValue(key, path, value.ToByteArrayDC()).ToDoubleDC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public decimal AddNeuronValue(string key, string path, decimal value)
        {
            return this.AddNeuronValue(key, path, value.ToByteArrayDC()).ToDecimalDC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte[] AddNeuronValue(string key, string path, byte[] value)
        {
            var keyArr = key.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 2 + 4 + keyArr.Length + pathArr.Length + value.Length];
            data[0] = (byte)BrainPackageType.AddNeuronValue;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 2, 2);
            Array.Copy(value.Length.ToByteArrayDC(), 0, data, 1 + 2 + 2, 4);
            Array.Copy(keyArr, 0, data, 1 + 2 + 2 + 4, keyArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 2 + 2 + 4 + keyArr.Length, pathArr.Length);
            Array.Copy(value, 0, data, 1 + 2 + 2 + 4 + keyArr.Length + pathArr.Length, value.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var resultLen = new byte[4];
            BrainServer.ReadBuffer(this.client, this.stream, resultLen);

            var result = new byte[resultLen.ToInt32DC()];
            BrainServer.ReadBuffer(this.client, this.stream, result);

            return result;
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public string SubtractNeuronValue(string key, string path, string value)
        {
            return this.SubtractNeuronValue(key, path, value.ToByteArrayDC()).ToStringDC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte SubtractNeuronValue(string key, string path, byte value)
        {
            return this.SubtractNeuronValue(key, path, value.ToByteArrayDC()).ToByteDC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public sbyte SubtractNeuronValue(string key, string path, sbyte value)
        {
            return this.SubtractNeuronValue(key, path, value.ToByteArrayDC()).ToSByteDC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public short SubtractNeuronValue(string key, string path, short value)
        {
            return this.SubtractNeuronValue(key, path, value.ToByteArrayDC()).ToInt16DC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ushort SubtractNeuronValue(string key, string path, ushort value)
        {
            return this.SubtractNeuronValue(key, path, value.ToByteArrayDC()).ToUInt16DC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public int SubtractNeuronValue(string key, string path, int value)
        {
            return this.SubtractNeuronValue(key, path, value.ToByteArrayDC()).ToInt32DC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public uint SubtractNeuronValue(string key, string path, uint value)
        {
            return this.SubtractNeuronValue(key, path, value.ToByteArrayDC()).ToUInt32DC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public long SubtractNeuronValue(string key, string path, long value)
        {
            return this.SubtractNeuronValue(key, path, value.ToByteArrayDC()).ToInt64DC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ulong SubtractNeuronValue(string key, string path, ulong value)
        {
            return this.SubtractNeuronValue(key, path, value.ToByteArrayDC()).ToUInt64DC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public float SubtractNeuronValue(string key, string path, float value)
        {
            return this.SubtractNeuronValue(key, path, value.ToByteArrayDC()).ToSingleDC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public double SubtractNeuronValue(string key, string path, double value)
        {
            return this.SubtractNeuronValue(key, path, value.ToByteArrayDC()).ToDoubleDC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public decimal SubtractNeuronValue(string key, string path, decimal value)
        {
            return this.SubtractNeuronValue(key, path, value.ToByteArrayDC()).ToDecimalDC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte[] SubtractNeuronValue(string key, string path, byte[] value)
        {
            var keyArr = key.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 2 + 4 + keyArr.Length + pathArr.Length + value.Length];
            data[0] = (byte)BrainPackageType.SubtractNeuronValue;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 2, 2);
            Array.Copy(value.Length.ToByteArrayDC(), 0, data, 1 + 2 + 2, 4);
            Array.Copy(keyArr, 0, data, 1 + 2 + 2 + 4, keyArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 2 + 2 + 4 + keyArr.Length, pathArr.Length);
            Array.Copy(value, 0, data, 1 + 2 + 2 + 4 + keyArr.Length + pathArr.Length, value.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var resultLen = new byte[4];
            BrainServer.ReadBuffer(this.client, this.stream, resultLen);

            var result = new byte[resultLen.ToInt32DC()];
            BrainServer.ReadBuffer(this.client, this.stream, result);

            return result;
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public string MultiplyNeuronValue(string key, string path, string value)
        {
            return this.MultiplyNeuronValue(key, path, value.ToByteArrayDC()).ToStringDC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte MultiplyNeuronValue(string key, string path, byte value)
        {
            return this.MultiplyNeuronValue(key, path, value.ToByteArrayDC()).ToByteDC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public sbyte MultiplyNeuronValue(string key, string path, sbyte value)
        {
            return this.MultiplyNeuronValue(key, path, value.ToByteArrayDC()).ToSByteDC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public short MultiplyNeuronValue(string key, string path, short value)
        {
            return this.MultiplyNeuronValue(key, path, value.ToByteArrayDC()).ToInt16DC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ushort MultiplyNeuronValue(string key, string path, ushort value)
        {
            return this.MultiplyNeuronValue(key, path, value.ToByteArrayDC()).ToUInt16DC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public int MultiplyNeuronValue(string key, string path, int value)
        {
            return this.MultiplyNeuronValue(key, path, value.ToByteArrayDC()).ToInt32DC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public uint MultiplyNeuronValue(string key, string path, uint value)
        {
            return this.MultiplyNeuronValue(key, path, value.ToByteArrayDC()).ToUInt32DC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public long MultiplyNeuronValue(string key, string path, long value)
        {
            return this.MultiplyNeuronValue(key, path, value.ToByteArrayDC()).ToInt64DC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ulong MultiplyNeuronValue(string key, string path, ulong value)
        {
            return this.MultiplyNeuronValue(key, path, value.ToByteArrayDC()).ToUInt64DC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public float MultiplyNeuronValue(string key, string path, float value)
        {
            return this.MultiplyNeuronValue(key, path, value.ToByteArrayDC()).ToSingleDC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public double MultiplyNeuronValue(string key, string path, double value)
        {
            return this.MultiplyNeuronValue(key, path, value.ToByteArrayDC()).ToDoubleDC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public decimal MultiplyNeuronValue(string key, string path, decimal value)
        {
            return this.MultiplyNeuronValue(key, path, value.ToByteArrayDC()).ToDecimalDC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte[] MultiplyNeuronValue(string key, string path, byte[] value)
        {
            var keyArr = key.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 2 + 4 + keyArr.Length + pathArr.Length + value.Length];
            data[0] = (byte)BrainPackageType.MultiplyNeuronValue;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 2, 2);
            Array.Copy(value.Length.ToByteArrayDC(), 0, data, 1 + 2 + 2, 4);
            Array.Copy(keyArr, 0, data, 1 + 2 + 2 + 4, keyArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 2 + 2 + 4 + keyArr.Length, pathArr.Length);
            Array.Copy(value, 0, data, 1 + 2 + 2 + 4 + keyArr.Length + pathArr.Length, value.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var resultLen = new byte[4];
            BrainServer.ReadBuffer(this.client, this.stream, resultLen);

            var result = new byte[resultLen.ToInt32DC()];
            BrainServer.ReadBuffer(this.client, this.stream, result);

            return result;
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public string DivideNeuronValue(string key, string path, string value)
        {
            return this.DivideNeuronValue(key, path, value.ToByteArrayDC()).ToStringDC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte DivideNeuronValue(string key, string path, byte value)
        {
            return this.DivideNeuronValue(key, path, value.ToByteArrayDC()).ToByteDC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public sbyte DivideNeuronValue(string key, string path, sbyte value)
        {
            return this.DivideNeuronValue(key, path, value.ToByteArrayDC()).ToSByteDC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public short DivideNeuronValue(string key, string path, short value)
        {
            return this.DivideNeuronValue(key, path, value.ToByteArrayDC()).ToInt16DC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ushort DivideNeuronValue(string key, string path, ushort value)
        {
            return this.DivideNeuronValue(key, path, value.ToByteArrayDC()).ToUInt16DC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public int DivideNeuronValue(string key, string path, int value)
        {
            return this.DivideNeuronValue(key, path, value.ToByteArrayDC()).ToInt32DC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public uint DivideNeuronValue(string key, string path, uint value)
        {
            return this.DivideNeuronValue(key, path, value.ToByteArrayDC()).ToUInt32DC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public long DivideNeuronValue(string key, string path, long value)
        {
            return this.DivideNeuronValue(key, path, value.ToByteArrayDC()).ToInt64DC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ulong DivideNeuronValue(string key, string path, ulong value)
        {
            return this.DivideNeuronValue(key, path, value.ToByteArrayDC()).ToUInt64DC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public float DivideNeuronValue(string key, string path, float value)
        {
            return this.DivideNeuronValue(key, path, value.ToByteArrayDC()).ToSingleDC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public double DivideNeuronValue(string key, string path, double value)
        {
            return this.DivideNeuronValue(key, path, value.ToByteArrayDC()).ToDoubleDC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public decimal DivideNeuronValue(string key, string path, decimal value)
        {
            return this.DivideNeuronValue(key, path, value.ToByteArrayDC()).ToDecimalDC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte[] DivideNeuronValue(string key, string path, byte[] value)
        {
            var keyArr = key.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 2 + 4 + keyArr.Length + pathArr.Length + value.Length];
            data[0] = (byte)BrainPackageType.DivideNeuronValue;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 2, 2);
            Array.Copy(value.Length.ToByteArrayDC(), 0, data, 1 + 2 + 2, 4);
            Array.Copy(keyArr, 0, data, 1 + 2 + 2 + 4, keyArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 2 + 2 + 4 + keyArr.Length, pathArr.Length);
            Array.Copy(value, 0, data, 1 + 2 + 2 + 4 + keyArr.Length + pathArr.Length, value.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var resultLen = new byte[4];
            BrainServer.ReadBuffer(this.client, this.stream, resultLen);

            var result = new byte[resultLen.ToInt32DC()];
            BrainServer.ReadBuffer(this.client, this.stream, result);

            return result;
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public string ModuloNeuronValue(string key, string path, string value)
        {
            return this.ModuloNeuronValue(key, path, value.ToByteArrayDC()).ToStringDC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte ModuloNeuronValue(string key, string path, byte value)
        {
            return this.ModuloNeuronValue(key, path, value.ToByteArrayDC()).ToByteDC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public sbyte ModuloNeuronValue(string key, string path, sbyte value)
        {
            return this.ModuloNeuronValue(key, path, value.ToByteArrayDC()).ToSByteDC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public short ModuloNeuronValue(string key, string path, short value)
        {
            return this.ModuloNeuronValue(key, path, value.ToByteArrayDC()).ToInt16DC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ushort ModuloNeuronValue(string key, string path, ushort value)
        {
            return this.ModuloNeuronValue(key, path, value.ToByteArrayDC()).ToUInt16DC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public int ModuloNeuronValue(string key, string path, int value)
        {
            return this.ModuloNeuronValue(key, path, value.ToByteArrayDC()).ToInt32DC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public uint ModuloNeuronValue(string key, string path, uint value)
        {
            return this.ModuloNeuronValue(key, path, value.ToByteArrayDC()).ToUInt32DC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public long ModuloNeuronValue(string key, string path, long value)
        {
            return this.ModuloNeuronValue(key, path, value.ToByteArrayDC()).ToInt64DC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ulong ModuloNeuronValue(string key, string path, ulong value)
        {
            return this.ModuloNeuronValue(key, path, value.ToByteArrayDC()).ToUInt64DC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public float ModuloNeuronValue(string key, string path, float value)
        {
            return this.ModuloNeuronValue(key, path, value.ToByteArrayDC()).ToSingleDC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public double ModuloNeuronValue(string key, string path, double value)
        {
            return this.ModuloNeuronValue(key, path, value.ToByteArrayDC()).ToDoubleDC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public decimal ModuloNeuronValue(string key, string path, decimal value)
        {
            return this.ModuloNeuronValue(key, path, value.ToByteArrayDC()).ToDecimalDC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte[] ModuloNeuronValue(string key, string path, byte[] value)
        {
            var keyArr = key.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 2 + 4 + keyArr.Length + pathArr.Length + value.Length];
            data[0] = (byte)BrainPackageType.ModuloNeuronValue;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 2, 2);
            Array.Copy(value.Length.ToByteArrayDC(), 0, data, 1 + 2 + 2, 4);
            Array.Copy(keyArr, 0, data, 1 + 2 + 2 + 4, keyArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 2 + 2 + 4 + keyArr.Length, pathArr.Length);
            Array.Copy(value, 0, data, 1 + 2 + 2 + 4 + keyArr.Length + pathArr.Length, value.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var resultLen = new byte[4];
            BrainServer.ReadBuffer(this.client, this.stream, resultLen);

            var result = new byte[resultLen.ToInt32DC()];
            BrainServer.ReadBuffer(this.client, this.stream, result);

            return result;
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetOrAddNeuronValue(string key, string type, string path, string value)
        {
            this.SetOrAddNeuronValue(key, type, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetOrAddNeuronValue(string key, string type, string path, byte value)
        {
            this.SetOrAddNeuronValue(key, type, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetOrAddNeuronValue(string key, string type, string path, sbyte value)
        {
            this.SetOrAddNeuronValue(key, type, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetOrAddNeuronValue(string key, string type, string path, short value)
        {
            this.SetOrAddNeuronValue(key, type, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetOrAddNeuronValue(string key, string type, string path, ushort value)
        {
            this.SetOrAddNeuronValue(key, type, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetOrAddNeuronValue(string key, string type, string path, int value)
        {
            this.SetOrAddNeuronValue(key, type, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetOrAddNeuronValue(string key, string type, string path, uint value)
        {
            this.SetOrAddNeuronValue(key, type, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetOrAddNeuronValue(string key, string type, string path, long value)
        {
            this.SetOrAddNeuronValue(key, type, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetOrAddNeuronValue(string key, string type, string path, ulong value)
        {
            this.SetOrAddNeuronValue(key, type, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetOrAddNeuronValue(string key, string type, string path, float value)
        {
            this.SetOrAddNeuronValue(key, type, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetOrAddNeuronValue(string key, string type, string path, double value)
        {
            this.SetOrAddNeuronValue(key, type, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetOrAddNeuronValue(string key, string type, string path, decimal value)
        {
            this.SetOrAddNeuronValue(key, type, path, value.ToByteArrayDC());
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public void SetOrAddNeuronValue(string key, string type, string path, byte[] value)
        {
            var keyArr = key.ToByteArrayDC();
            var typeArr = type.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 1 + 2 + 4 + keyArr.Length + typeArr.Length + pathArr.Length + value.Length];
            data[0] = (byte)BrainPackageType.SetOrAddNeuronValue;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(new byte[] { (byte)typeArr.Length }, 0, data, 1 + 2, 1);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 1 + 2, 2);
            Array.Copy(value.Length.ToByteArrayDC(), 0, data, 1 + 1 + 2 + 2, 4);
            Array.Copy(keyArr, 0, data, 1 + 1 + 2 + 2 + 4, keyArr.Length);
            Array.Copy(typeArr, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length, typeArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length + typeArr.Length, pathArr.Length);
            Array.Copy(value, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length + typeArr.Length + pathArr.Length, value.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public string AddNeuronValueWithAdd(string key, string type, string path, string value)
        {
            return this.AddNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToStringDC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte AddNeuronValueWithAdd(string key, string type, string path, byte value)
        {
            return this.AddNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToByteDC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public sbyte AddNeuronValueWithAdd(string key, string type, string path, sbyte value)
        {
            return this.AddNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToSByteDC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public short AddNeuronValueWithAdd(string key, string type, string path, short value)
        {
            return this.AddNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt16DC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ushort AddNeuronValueWithAdd(string key, string type, string path, ushort value)
        {
            return this.AddNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt16DC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public int AddNeuronValueWithAdd(string key, string type, string path, int value)
        {
            return this.AddNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt32DC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public uint AddNeuronValueWithAdd(string key, string type, string path, uint value)
        {
            return this.AddNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt32DC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public long AddNeuronValueWithAdd(string key, string type, string path, long value)
        {
            return this.AddNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt64DC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ulong AddNeuronValueWithAdd(string key, string type, string path, ulong value)
        {
            return this.AddNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt64DC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public float AddNeuronValueWithAdd(string key, string type, string path, float value)
        {
            return this.AddNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToSingleDC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public double AddNeuronValueWithAdd(string key, string type, string path, double value)
        {
            return this.AddNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToDoubleDC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public decimal AddNeuronValueWithAdd(string key, string type, string path, decimal value)
        {
            return this.AddNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToDecimalDC();
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte[] AddNeuronValueWithAdd(string key, string type, string path, byte[] value)
        {
            var keyArr = key.ToByteArrayDC();
            var typeArr = type.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 1 + 2 + 4 + keyArr.Length + typeArr.Length + pathArr.Length + value.Length];
            data[0] = (byte)BrainPackageType.AddNeuronValueWithAdd;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(new byte[] { (byte)typeArr.Length }, 0, data, 1 + 2, 1);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 1 + 2, 2);
            Array.Copy(value.Length.ToByteArrayDC(), 0, data, 1 + 1 + 2 + 2, 4);
            Array.Copy(keyArr, 0, data, 1 + 1 + 2 + 2 + 4, keyArr.Length);
            Array.Copy(typeArr, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length, typeArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length + typeArr.Length, pathArr.Length);
            Array.Copy(value, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length + typeArr.Length + pathArr.Length, value.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var resultLen = new byte[4];
            BrainServer.ReadBuffer(this.client, this.stream, resultLen);

            var result = new byte[resultLen.ToInt32DC()];
            BrainServer.ReadBuffer(this.client, this.stream, result);

            return result;
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public string SubtractNeuronValueWithAdd(string key, string type, string path, string value)
        {
            return this.SubtractNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToStringDC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte SubtractNeuronValueWithAdd(string key, string type, string path, byte value)
        {
            return this.SubtractNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToByteDC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public sbyte SubtractNeuronValueWithAdd(string key, string type, string path, sbyte value)
        {
            return this.SubtractNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToSByteDC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public short SubtractNeuronValueWithAdd(string key, string type, string path, short value)
        {
            return this.SubtractNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt16DC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ushort SubtractNeuronValueWithAdd(string key, string type, string path, ushort value)
        {
            return this.SubtractNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt16DC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public int SubtractNeuronValueWithAdd(string key, string type, string path, int value)
        {
            return this.SubtractNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt32DC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public uint SubtractNeuronValueWithAdd(string key, string type, string path, uint value)
        {
            return this.SubtractNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt32DC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public long SubtractNeuronValueWithAdd(string key, string type, string path, long value)
        {
            return this.SubtractNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt64DC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ulong SubtractNeuronValueWithAdd(string key, string type, string path, ulong value)
        {
            return this.SubtractNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt64DC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public float SubtractNeuronValueWithAdd(string key, string type, string path, float value)
        {
            return this.SubtractNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToSingleDC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public double SubtractNeuronValueWithAdd(string key, string type, string path, double value)
        {
            return this.SubtractNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToDoubleDC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public decimal SubtractNeuronValueWithAdd(string key, string type, string path, decimal value)
        {
            return this.SubtractNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToDecimalDC();
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte[] SubtractNeuronValueWithAdd(string key, string type, string path, byte[] value)
        {
            var keyArr = key.ToByteArrayDC();
            var typeArr = type.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 1 + 2 + 4 + keyArr.Length + typeArr.Length + pathArr.Length + value.Length];
            data[0] = (byte)BrainPackageType.SubtractNeuronValueWithAdd;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(new byte[] { (byte)typeArr.Length }, 0, data, 1 + 2, 1);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 1 + 2, 2);
            Array.Copy(value.Length.ToByteArrayDC(), 0, data, 1 + 1 + 2 + 2, 4);
            Array.Copy(keyArr, 0, data, 1 + 1 + 2 + 2 + 4, keyArr.Length);
            Array.Copy(typeArr, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length, typeArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length + typeArr.Length, pathArr.Length);
            Array.Copy(value, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length + typeArr.Length + pathArr.Length, value.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var resultLen = new byte[4];
            BrainServer.ReadBuffer(this.client, this.stream, resultLen);

            var result = new byte[resultLen.ToInt32DC()];
            BrainServer.ReadBuffer(this.client, this.stream, result);

            return result;
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public string MultiplyNeuronValueWithAdd(string key, string type, string path, string value)
        {
            return this.MultiplyNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToStringDC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte MultiplyNeuronValueWithAdd(string key, string type, string path, byte value)
        {
            return this.MultiplyNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToByteDC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public sbyte MultiplyNeuronValueWithAdd(string key, string type, string path, sbyte value)
        {
            return this.MultiplyNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToSByteDC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public short MultiplyNeuronValueWithAdd(string key, string type, string path, short value)
        {
            return this.MultiplyNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt16DC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ushort MultiplyNeuronValueWithAdd(string key, string type, string path, ushort value)
        {
            return this.MultiplyNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt16DC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public int MultiplyNeuronValueWithAdd(string key, string type, string path, int value)
        {
            return this.MultiplyNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt32DC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public uint MultiplyNeuronValueWithAdd(string key, string type, string path, uint value)
        {
            return this.MultiplyNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt32DC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public long MultiplyNeuronValueWithAdd(string key, string type, string path, long value)
        {
            return this.MultiplyNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt64DC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ulong MultiplyNeuronValueWithAdd(string key, string type, string path, ulong value)
        {
            return this.MultiplyNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt64DC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public float MultiplyNeuronValueWithAdd(string key, string type, string path, float value)
        {
            return this.MultiplyNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToSingleDC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public double MultiplyNeuronValueWithAdd(string key, string type, string path, double value)
        {
            return this.MultiplyNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToDoubleDC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public decimal MultiplyNeuronValueWithAdd(string key, string type, string path, decimal value)
        {
            return this.MultiplyNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToDecimalDC();
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte[] MultiplyNeuronValueWithAdd(string key, string type, string path, byte[] value)
        {
            var keyArr = key.ToByteArrayDC();
            var typeArr = type.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 1 + 2 + 4 + keyArr.Length + typeArr.Length + pathArr.Length + value.Length];
            data[0] = (byte)BrainPackageType.MultiplyNeuronValueWithAdd;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(new byte[] { (byte)typeArr.Length }, 0, data, 1 + 2, 1);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 1 + 2, 2);
            Array.Copy(value.Length.ToByteArrayDC(), 0, data, 1 + 1 + 2 + 2, 4);
            Array.Copy(keyArr, 0, data, 1 + 1 + 2 + 2 + 4, keyArr.Length);
            Array.Copy(typeArr, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length, typeArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length + typeArr.Length, pathArr.Length);
            Array.Copy(value, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length + typeArr.Length + pathArr.Length, value.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var resultLen = new byte[4];
            BrainServer.ReadBuffer(this.client, this.stream, resultLen);

            var result = new byte[resultLen.ToInt32DC()];
            BrainServer.ReadBuffer(this.client, this.stream, result);

            return result;
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public string DivideNeuronValueWithAdd(string key, string type, string path, string value)
        {
            return this.DivideNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToStringDC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte DivideNeuronValueWithAdd(string key, string type, string path, byte value)
        {
            return this.DivideNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToByteDC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public sbyte DivideNeuronValueWithAdd(string key, string type, string path, sbyte value)
        {
            return this.DivideNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToSByteDC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public short DivideNeuronValueWithAdd(string key, string type, string path, short value)
        {
            return this.DivideNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt16DC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ushort DivideNeuronValueWithAdd(string key, string type, string path, ushort value)
        {
            return this.DivideNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt16DC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public int DivideNeuronValueWithAdd(string key, string type, string path, int value)
        {
            return this.DivideNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt32DC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public uint DivideNeuronValueWithAdd(string key, string type, string path, uint value)
        {
            return this.DivideNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt32DC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public long DivideNeuronValueWithAdd(string key, string type, string path, long value)
        {
            return this.DivideNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt64DC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ulong DivideNeuronValueWithAdd(string key, string type, string path, ulong value)
        {
            return this.DivideNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt64DC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public float DivideNeuronValueWithAdd(string key, string type, string path, float value)
        {
            return this.DivideNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToSingleDC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public double DivideNeuronValueWithAdd(string key, string type, string path, double value)
        {
            return this.DivideNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToDoubleDC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public decimal DivideNeuronValueWithAdd(string key, string type, string path, decimal value)
        {
            return this.DivideNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToDecimalDC();
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte[] DivideNeuronValueWithAdd(string key, string type, string path, byte[] value)
        {
            var keyArr = key.ToByteArrayDC();
            var typeArr = type.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 1 + 2 + 4 + keyArr.Length + typeArr.Length + pathArr.Length + value.Length];
            data[0] = (byte)BrainPackageType.DivideNeuronValueWithAdd;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(new byte[] { (byte)typeArr.Length }, 0, data, 1 + 2, 1);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 1 + 2, 2);
            Array.Copy(value.Length.ToByteArrayDC(), 0, data, 1 + 1 + 2 + 2, 4);
            Array.Copy(keyArr, 0, data, 1 + 1 + 2 + 2 + 4, keyArr.Length);
            Array.Copy(typeArr, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length, typeArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length + typeArr.Length, pathArr.Length);
            Array.Copy(value, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length + typeArr.Length + pathArr.Length, value.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var resultLen = new byte[4];
            BrainServer.ReadBuffer(this.client, this.stream, resultLen);

            var result = new byte[resultLen.ToInt32DC()];
            BrainServer.ReadBuffer(this.client, this.stream, result);

            return result;
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public string ModuloNeuronValueWithAdd(string key, string type, string path, string value)
        {
            return this.ModuloNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToStringDC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte ModuloNeuronValueWithAdd(string key, string type, string path, byte value)
        {
            return this.ModuloNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToByteDC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public sbyte ModuloNeuronValueWithAdd(string key, string type, string path, sbyte value)
        {
            return this.ModuloNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToSByteDC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public short ModuloNeuronValueWithAdd(string key, string type, string path, short value)
        {
            return this.ModuloNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt16DC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ushort ModuloNeuronValueWithAdd(string key, string type, string path, ushort value)
        {
            return this.ModuloNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt16DC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public int ModuloNeuronValueWithAdd(string key, string type, string path, int value)
        {
            return this.ModuloNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt32DC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public uint ModuloNeuronValueWithAdd(string key, string type, string path, uint value)
        {
            return this.ModuloNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt32DC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public long ModuloNeuronValueWithAdd(string key, string type, string path, long value)
        {
            return this.ModuloNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToInt64DC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public ulong ModuloNeuronValueWithAdd(string key, string type, string path, ulong value)
        {
            return this.ModuloNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToUInt64DC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public float ModuloNeuronValueWithAdd(string key, string type, string path, float value)
        {
            return this.ModuloNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToSingleDC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public double ModuloNeuronValueWithAdd(string key, string type, string path, double value)
        {
            return this.ModuloNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToDoubleDC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public decimal ModuloNeuronValueWithAdd(string key, string type, string path, decimal value)
        {
            return this.ModuloNeuronValueWithAdd(key, type, path, value.ToByteArrayDC()).ToDecimalDC();
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public byte[] ModuloNeuronValueWithAdd(string key, string type, string path, byte[] value)
        {
            var keyArr = key.ToByteArrayDC();
            var typeArr = type.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 1 + 2 + 4 + keyArr.Length + typeArr.Length + pathArr.Length + value.Length];
            data[0] = (byte)BrainPackageType.ModuloNeuronValueWithAdd;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(new byte[] { (byte)typeArr.Length }, 0, data, 1 + 2, 1);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 1 + 2, 2);
            Array.Copy(value.Length.ToByteArrayDC(), 0, data, 1 + 1 + 2 + 2, 4);
            Array.Copy(keyArr, 0, data, 1 + 1 + 2 + 2 + 4, keyArr.Length);
            Array.Copy(typeArr, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length, typeArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length + typeArr.Length, pathArr.Length);
            Array.Copy(value, 0, data, 1 + 1 + 2 + 2 + 4 + keyArr.Length + typeArr.Length + pathArr.Length, value.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var resultLen = new byte[4];
            BrainServer.ReadBuffer(this.client, this.stream, resultLen);

            var result = new byte[resultLen.ToInt32DC()];
            BrainServer.ReadBuffer(this.client, this.stream, result);

            return result;
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public string GetNeuronValueAsString(string key, string path)
        {
            return GetNeuronValue(key, path).ToStringDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public byte GetNeuronValueAsByte(string key, string path)
        {
            return GetNeuronValue(key, path).ToByteDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public sbyte GetNeuronValueAsSByte(string key, string path)
        {
            return GetNeuronValue(key, path).ToSByteDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public short GetNeuronValueAsInt16(string key, string path)
        {
            return GetNeuronValue(key, path).ToInt16DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public ushort GetNeuronValueAsUInt16(string key, string path)
        {
            return GetNeuronValue(key, path).ToUInt16DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public int GetNeuronValueAsInt32(string key, string path)
        {
            return GetNeuronValue(key, path).ToInt32DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public uint GetNeuronValueAsUInt32(string key, string path)
        {
            return GetNeuronValue(key, path).ToUInt32DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public long GetNeuronValueAsInt64(string key, string path)
        {
            return GetNeuronValue(key, path).ToInt64DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public ulong GetNeuronValueAsUInt64(string key, string path)
        {
            return GetNeuronValue(key, path).ToUInt64DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public float GetNeuronValueAsSingle(string key, string path)
        {
            return GetNeuronValue(key, path).ToSingleDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public double GetNeuronValueAsDouble(string key, string path)
        {
            return GetNeuronValue(key, path).ToDoubleDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public decimal GetNeuronValueAsDecimal(string key, string path)
        {
            return GetNeuronValue(key, path).ToDecimalDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public byte[] GetNeuronValue(string key, string path)
        {
            var keyArr = key.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();
            
            byte[] data = new byte[1 + 2 + 2 + keyArr.Length + pathArr.Length];
            data[0] = (byte)BrainPackageType.GetNeuronValue;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 2, 2);
            Array.Copy(keyArr, 0, data, 1 + 2 + 2, keyArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 2 + 2 + keyArr.Length, pathArr.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var valueLen = new byte[4];
            BrainServer.ReadBuffer(this.client, this.stream, valueLen);

            var value = new byte[valueLen.ToInt32DC()];
            BrainServer.ReadBuffer(this.client, this.stream, value);

            return value;
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        public string GetOrAddNeuronValueAsString(string key, string type, string path)
        {
            return GetOrAddNeuronValue(key, type, path).ToStringDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
		/// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        public byte GetOrAddNeuronValueAsByte(string key, string type, string path)
        {
            return GetOrAddNeuronValue(key, type, path).ToByteDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
		/// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        public sbyte GetOrAddNeuronValueAsSByte(string key, string type, string path)
        {
            return GetOrAddNeuronValue(key, type, path).ToSByteDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
		/// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        public short GetOrAddNeuronValueAsInt16(string key, string type, string path)
        {
            return GetOrAddNeuronValue(key, type, path).ToInt16DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
		/// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        public ushort GetOrAddNeuronValueAsUInt16(string key, string type, string path)
        {
            return GetOrAddNeuronValue(key, type, path).ToUInt16DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
		/// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        public int GetOrAddNeuronValueAsInt32(string key, string type, string path)
        {
            return GetOrAddNeuronValue(key, type, path).ToInt32DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
		/// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        public uint GetOrAddNeuronValueAsUInt32(string key, string type, string path)
        {
            return GetOrAddNeuronValue(key, type, path).ToUInt32DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
		/// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        public long GetOrAddNeuronValueAsInt64(string key, string type, string path)
        {
            return GetOrAddNeuronValue(key, type, path).ToInt64DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
		/// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        public ulong GetOrAddNeuronValueAsUInt64(string key, string type, string path)
        {
            return GetOrAddNeuronValue(key, type, path).ToUInt64DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
		/// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        public float GetOrAddNeuronValueAsSingle(string key, string type, string path)
        {
            return GetOrAddNeuronValue(key, type, path).ToSingleDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
		/// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        public double GetOrAddNeuronValueAsDouble(string key, string type, string path)
        {
            return GetOrAddNeuronValue(key, type, path).ToDoubleDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
		/// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        public decimal GetOrAddNeuronValueAsDecimal(string key, string type, string path)
        {
            return GetOrAddNeuronValue(key, type, path).ToDecimalDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
		/// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        public byte[] GetOrAddNeuronValue(string key, string type, string path)
        {
            var keyArr = key.ToByteArrayDC();
            var typeArr = type.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 1 + 2 + 2 + keyArr.Length + typeArr.Length + pathArr.Length];
            data[0] = (byte)BrainPackageType.GetOrAddNeuronValue;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(new byte[] { (byte)typeArr.Length }, 0, data, 1 + 2, 1);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 1 + 2, 2);
            Array.Copy(keyArr, 0, data, 1 + 1 + 2 + 2, keyArr.Length);
            Array.Copy(typeArr, 0, data, 1 + 1 + 2 + 2 + keyArr.Length, typeArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 1 + 2 + 2 + keyArr.Length + typeArr.Length, pathArr.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var valueLen = new byte[4];
            BrainServer.ReadBuffer(this.client, this.stream, valueLen);

            var value = new byte[valueLen.ToInt32DC()];
            BrainServer.ReadBuffer(this.client, this.stream, value);

            return value;
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// If it is not exists, we will get a null value.
		/// You shouldn't forget that it is working by key!
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public string GetNullableNeuronValueAsString(string key, string path)
        {
            var value = GetNullableNeuronValue(key, path);
            if (value == null)
                return null;
            return value.ToStringDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will get a null value.
		/// You shouldn't forget that it is working by key!
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public byte? GetNullableNeuronValueAsByte(string key, string path)
        {
            var value = GetNullableNeuronValue(key, path);
            if (value == null)
                return null;
            return value.ToByteDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will get a null value.
		/// You shouldn't forget that it is working by key!
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public sbyte? GetNullableNeuronValueAsSByte(string key, string path)
        {
            var value = GetNullableNeuronValue(key, path);
            if (value == null)
                return null;
            return value.ToSByteDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will get a null value.
		/// You shouldn't forget that it is working by key!
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public short? GetNullableNeuronValueAsInt16(string key, string path)
        {
            var value = GetNullableNeuronValue(key, path);
            if (value == null)
                return null;
            return value.ToInt16DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will get a null value.
		/// You shouldn't forget that it is working by key!
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public ushort? GetNullableNeuronValueAsUInt16(string key, string path)
        {
            var value = GetNullableNeuronValue(key, path);
            if (value == null)
                return null;
            return value.ToUInt16DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will get a null value.
		/// You shouldn't forget that it is working by key!
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public int? GetNullableNeuronValueAsInt32(string key, string path)
        {
            var value = GetNullableNeuronValue(key, path);
            if (value == null)
                return null;
            return value.ToInt32DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will get a null value.
		/// You shouldn't forget that it is working by key!
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public uint? GetNullableNeuronValueAsUInt32(string key, string path)
        {
            var value = GetNullableNeuronValue(key, path);
            if (value == null)
                return null;
            return value.ToUInt32DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will get a null value.
		/// You shouldn't forget that it is working by key!
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public long? GetNullableNeuronValueAsInt64(string key, string path)
        {
            var value = GetNullableNeuronValue(key, path);
            if (value == null)
                return null;
            return value.ToInt64DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will get a null value.
		/// You shouldn't forget that it is working by key!
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public ulong? GetNullableNeuronValueAsUInt64(string key, string path)
        {
            var value = GetNullableNeuronValue(key, path);
            if (value == null)
                return null;
            return value.ToUInt64DC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will get a null value.
		/// You shouldn't forget that it is working by key!
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public float? GetNullableNeuronValueAsSingle(string key, string path)
        {
            var value = GetNullableNeuronValue(key, path);
            if (value == null)
                return null;
            return value.ToSingleDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will get a null value.
		/// You shouldn't forget that it is working by key!
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public double? GetNullableNeuronValueAsDouble(string key, string path)
        {
            var value = GetNullableNeuronValue(key, path);
            if (value == null)
                return null;
            return value.ToDoubleDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will get a null value.
		/// You shouldn't forget that it is working by key!
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public decimal? GetNullableNeuronValueAsDecimal(string key, string path)
        {
            var value = GetNullableNeuronValue(key, path);
            if (value == null)
                return null;
            return value.ToDecimalDC();
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
		/// If it is not exists, we will get a null value.
		/// You shouldn't forget that it is working by key!
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public byte[] GetNullableNeuronValue(string key, string path)
        {
            var keyArr = key.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 2 + keyArr.Length + pathArr.Length];
            data[0] = (byte)BrainPackageType.GetNullableNeuronValue;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 2, 2);
            Array.Copy(keyArr, 0, data, 1 + 2 + 2, keyArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 2 + 2 + keyArr.Length, pathArr.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var valueLen = new byte[4];
            BrainServer.ReadBuffer(this.client, this.stream, valueLen);

            var len = valueLen.ToInt32DC();
            if (len == 0)
                return null;

            var value = new byte[len];
            BrainServer.ReadBuffer(this.client, this.stream, value);

            return value;
        }

        /// <summary>
        /// We are setting a neuron to neuron field by keys.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="neuronKey">What is the new neuron?</param>
        public void SetNeuronValueToNeuron(string key, string path, string neuronKey)
        {
            var keyArr = key.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();
            var neuronKeyArr = neuronKey.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 2 + 2 + keyArr.Length + pathArr.Length + neuronKeyArr.Length];
            data[0] = (byte)BrainPackageType.SetNeuronValueToNeuron;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 2, 2);
            Array.Copy(((short)neuronKeyArr.Length).ToByteArrayDC(), 0, data, 1 + 2 + 2, 2);
            Array.Copy(keyArr, 0, data, 1 + 2 + 2 + 2, keyArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 2 + 2 + 2 + keyArr.Length, pathArr.Length);
            Array.Copy(neuronKeyArr, 0, data, 1 + 2 + 2 + 2 + keyArr.Length + pathArr.Length, neuronKeyArr.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();
        }

        /// <summary>
        /// We are setting a neuron to neuron field by keys.
        /// If it is not exists, we will add a neuron and then set value of it.
        /// But you shouldn't forget that this is not for arrays. 
        /// You have to use <see cref="AddANeuronToList(string, string)"/> for it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron type.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="neuronKey">What is the new neuron?</param>
        public void SetOrAddNeuronValueToNeuron(string key, string type, string path, string neuronKey)
        {
            var keyArr = key.ToByteArrayDC();
            var typeArr = type.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();
            var neuronKeyArr = neuronKey.ToByteArrayDC();

            byte[] data = new byte[1 + 1 + 2 + 2 + 2 + keyArr.Length + typeArr.Length + pathArr.Length + neuronKeyArr.Length];
            data[0] = (byte)BrainPackageType.SetOrAddNeuronValueToNeuron;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(new byte[] { (byte)typeArr.Length }, 0, data, 1 + 2, 1);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 2 + 1, 2);
            Array.Copy(((short)neuronKeyArr.Length).ToByteArrayDC(), 0, data, 1 + 2 + 1 + 2, 2);
            Array.Copy(keyArr, 0, data, 1 + 2 + 1 + 2 + 2, keyArr.Length);
            Array.Copy(typeArr, 0, data, 1 + 2 + 1 + 2 + 2 + keyArr.Length, typeArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 2 + 1 + 2 + 2 + keyArr.Length + typeArr.Length, pathArr.Length);
            Array.Copy(neuronKeyArr, 0, data, 1 + 2 + 1 + 2 + 2 + keyArr.Length + typeArr.Length + pathArr.Length, neuronKeyArr.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();
        }

        /// <summary>
        /// We are adding a new neuron to list of a neuron.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public void AddANeuronToList(string key, string path)
        {
            var keyArr = key.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();
            
            byte[] data = new byte[1 + 2 + 2 + keyArr.Length + pathArr.Length];
            data[0] = (byte)BrainPackageType.AddANeuronToList;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 2, 2);
            Array.Copy(keyArr, 0, data, 1 + 2 + 2, keyArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 2 + 2 + keyArr.Length, pathArr.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();
        }

        /// <summary>
        /// We are removing a new neuron from list.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="index">Index of data.</param>
        public void RemoveANeuronFromList(string key, string path, int index)
        {
            var keyArr = key.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 2 + 4 + keyArr.Length + pathArr.Length];
            data[0] = (byte)BrainPackageType.RemoveANeuronFromList;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 2, 2);
            Array.Copy(index.ToByteArrayDC(), 0, data, 1 + 2 + 2, 4);
            Array.Copy(keyArr, 0, data, 1 + 2 + 2 + 4, keyArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 2 + 2 + 4 + keyArr.Length, pathArr.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();
        }

        /// <summary>
        /// Clear a neuron list.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public void ClearNeuronList(string key, string path)
        {
            var keyArr = key.ToByteArrayDC();
            var pathArr = path.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + 2 + keyArr.Length + pathArr.Length];
            data[0] = (byte)BrainPackageType.ClearNeuronList;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(((short)pathArr.Length).ToByteArrayDC(), 0, data, 1 + 2, 2);
            Array.Copy(keyArr, 0, data, 1 + 2 + 2, keyArr.Length);
            Array.Copy(pathArr, 0, data, 1 + 2 + 2 + keyArr.Length, pathArr.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();
        }

        /// <summary>
        /// Does neuron exists?
        /// </summary>
        /// <param name="key">Data ID.</param>
        public bool NeuronExists(string key)
        {
            var keyArr = key.ToByteArrayDC();

            byte[] data = new byte[1 + 2 + keyArr.Length];
            data[0] = (byte)BrainPackageType.NeuronExists;

            Array.Copy(((short)keyArr.Length).ToByteArrayDC(), 0, data, 1, 2);
            Array.Copy(keyArr, 0, data, 1 + 2, keyArr.Length);

            this.stream.Write(data, 0, data.Length);

            this.checkOkMessage();

            var value = new byte[1];
            BrainServer.ReadBuffer(this.client, this.stream, value);

            return value[0] == 1;
        }
        #endregion
        #endregion
    }
}
