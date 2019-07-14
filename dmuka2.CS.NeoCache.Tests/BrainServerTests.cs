using dmuka2.CS.NeoCache;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Tests
{
    public class BrainServerTests
    {
        /// <summary>
        /// Can server open/close?
        /// </summary>
        [Test]
        public void ServerOpenClose()
        {
            BrainServer server = new BrainServer(4, "p@ss", 1000);
            bool started = false;
            bool stoped = false;

            Thread thread = new Thread(() =>
            {
                try
                {
                    started = true;
                    server.Open();
                    stoped = true;
                }
                catch
                {
                }
            });
            thread.Start();

            Thread.Sleep(1000);
            Assert.IsTrue(started);
            server.Close();
            Thread.Sleep(1000);
            Assert.IsTrue(stoped);
        }

        /// <summary>
        /// Can we login?
        /// </summary>
        [Test]
        public void Login()
        {
            BrainServer server = new BrainServer(4, "p@ss", 1001);
            bool started = false;
            bool stoped = false;

            Thread thread = new Thread(() =>
            {
                try
                {
                    started = true;
                    server.Open();
                    stoped = true;
                }
                catch
                {
                }
            });
            thread.Start();

            Thread.Sleep(1000);
            Assert.IsTrue(started);

            BrainClient client = new BrainClient();
            client.Open("localhost", 1001, "p@ss");
            client.Close();

            server.Close();
            Thread.Sleep(1000);
            Assert.IsTrue(stoped);
        }

        /// <summary>
        /// Is authorization working correct?
        /// </summary>
        [Test]
        public void LoginAuthorization()
        {
            BrainServer server = new BrainServer(4, "p@ss", 1002);
            bool started = false;
            bool stoped = false;

            Thread thread = new Thread(() =>
            {
                try
                {
                    started = true;
                    server.Open();
                    stoped = true;
                }
                catch
                {
                }
            });
            thread.Start();

            Thread.Sleep(1000);
            Assert.IsTrue(started);

            try
            {
                BrainClient client = new BrainClient();
                client.Open("localhost", 1002, "p@ss2");
                client.Close();
                Assert.Fail("Auth is not working?");
            }
            catch
            { }

            BrainClient client2 = new BrainClient();
            client2.Open("localhost", 1002, "p@ss");
            client2.Close();

            server.Close();
            Thread.Sleep(1000);
            Assert.IsTrue(stoped);
        }

        /// <summary>
        /// We will try everything.
        /// </summary>
        [Test]
        public void TryEverything()
        {
            BrainServer server = new BrainServer(4, "p@ss", 1003);
            bool started = false;
            bool stoped = false;

            Thread thread = new Thread(() =>
            {
                try
                {
                    started = true;
                    server.Open();
                    stoped = true;
                }
                catch
                {
                }
            });
            thread.Start();

            Thread.Sleep(1000);
            Assert.IsTrue(started);

            BrainClient client = new BrainClient();
            client.Open("localhost", 1003, "p@ss");

            Assert.AreEqual(client.GetNeuronValueAsInt32("global", "key_count"), 1);
            Assert.AreEqual(client.GetNeuronValueAsString("global", "keys"), "global");
            Assert.AreEqual(client.GetNeuronValueAsInt32("global", "type_count"), 4);

            var nt1_1 = "nt1_1";
            Assert.IsFalse(client.NeuronExists(nt1_1));
            client.AddANeuron(nt1_1, nameof(NeuronTestModel1));
            client.AddANeuronToList(nt1_1, nameof(NeuronTestModel1.test2datas));
            Assert.IsTrue(client.NeuronExists(nt1_1));
            Assert.AreEqual(client.GetNeuronValueAsString(nt1_1, "key"), nt1_1);

            Assert.AreEqual(client.GetNeuronValueAsInt32("global", "key_count"), 2);
            Assert.AreEqual(client.GetNeuronValueAsString("global", "keys"), "global~nt1_1");
            Assert.AreEqual(client.GetNeuronValueAsInt32("global", "type_count"), 4);

            client.SetNeuronValue(nt1_1, nameof(NeuronTestModel1.stringValue), "abc");
            Assert.AreEqual(client.GetNeuronValueAsString(nt1_1, nameof(NeuronTestModel1.stringValue)), "abc");

            client.SetNeuronValue(nt1_1, nameof(NeuronTestModel1.byteValue), (byte)55);
            Assert.AreEqual(client.GetNeuronValueAsByte(nt1_1, nameof(NeuronTestModel1.byteValue)), (byte)55);

            client.SetNeuronValue(nt1_1, nameof(NeuronTestModel1.sbyteValue), (sbyte)55);
            Assert.AreEqual(client.GetNeuronValueAsSByte(nt1_1, nameof(NeuronTestModel1.sbyteValue)), (sbyte)55);

            client.SetNeuronValue(nt1_1, nameof(NeuronTestModel1.shortValue), (short)55);
            Assert.AreEqual(client.GetNeuronValueAsInt16(nt1_1, nameof(NeuronTestModel1.shortValue)), (short)55);

            client.SetNeuronValue(nt1_1, nameof(NeuronTestModel1.ushortValue), (ushort)55);
            Assert.AreEqual(client.GetNeuronValueAsUInt16(nt1_1, nameof(NeuronTestModel1.ushortValue)), (ushort)55);

            client.SetNeuronValue(nt1_1, nameof(NeuronTestModel1.intValue), (int)55);
            Assert.AreEqual(client.GetNeuronValueAsInt32(nt1_1, nameof(NeuronTestModel1.intValue)), (int)55);

            client.SetNeuronValue(nt1_1, nameof(NeuronTestModel1.uintValue), (uint)55);
            Assert.AreEqual(client.GetNeuronValueAsUInt32(nt1_1, nameof(NeuronTestModel1.uintValue)), (uint)55);

            client.SetNeuronValue(nt1_1, nameof(NeuronTestModel1.longValue), (long)55);
            Assert.AreEqual(client.GetNeuronValueAsInt64(nt1_1, nameof(NeuronTestModel1.longValue)), (long)55);

            client.SetNeuronValue(nt1_1, nameof(NeuronTestModel1.ulongValue), (ulong)55);
            Assert.AreEqual(client.GetNeuronValueAsUInt64(nt1_1, nameof(NeuronTestModel1.ulongValue)), (ulong)55);

            client.SetNeuronValue(nt1_1, nameof(NeuronTestModel1.floatValue), (float)55);
            Assert.AreEqual(client.GetNeuronValueAsSingle(nt1_1, nameof(NeuronTestModel1.floatValue)), (float)55);

            client.SetNeuronValue(nt1_1, nameof(NeuronTestModel1.doubleValue), (double)55);
            Assert.AreEqual(client.GetNeuronValueAsDouble(nt1_1, nameof(NeuronTestModel1.doubleValue)), (double)55);

            client.SetNeuronValue(nt1_1, nameof(NeuronTestModel1.decimalValue), (decimal)55);
            Assert.AreEqual(client.GetNeuronValueAsDecimal(nt1_1, nameof(NeuronTestModel1.decimalValue)), (decimal)55);

            client.AddANeuronToList(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[0].{nameof(NeuronTestModel2.test3datas)}");
            client.AddANeuronToList(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[0].{nameof(NeuronTestModel2.test3datas)}");
            client.AddANeuronToList(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[0].{nameof(NeuronTestModel2.test3datas)}");
            Assert.AreEqual(client.GetNeuronValueAsInt32(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}.{{len}}"), 1);
            Assert.AreEqual(client.GetNeuronValueAsInt32(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[0].{nameof(NeuronTestModel2.test3datas)}.{{len}}"), 3);

            client.AddANeuronToList(nt1_1, nameof(NeuronTestModel1.test2datas));
            client.AddANeuronToList(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[1].{nameof(NeuronTestModel2.test3datas)}");
            client.AddANeuronToList(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[1].{nameof(NeuronTestModel2.test3datas)}");
            Assert.AreEqual(client.GetNeuronValueAsInt32(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}.{{len}}"), 2);
            Assert.AreEqual(client.GetNeuronValueAsInt32(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[1].{nameof(NeuronTestModel2.test3datas)}.{{len}}"), 2);

            client.AddANeuronToList(nt1_1, nameof(NeuronTestModel1.test2datas));
            client.AddANeuronToList(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[2].{nameof(NeuronTestModel2.test3datas)}");
            Assert.AreEqual(client.GetNeuronValueAsInt32(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}.{{len}}"), 3);
            Assert.AreEqual(client.GetNeuronValueAsInt32(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[2].{nameof(NeuronTestModel2.test3datas)}.{{len}}"), 1);

            client.SetNeuronValue(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[2].{nameof(NeuronTestModel2.test3datas)}[0].{nameof(NeuronTestModel3.decimalValue)}", (decimal)-234);
            Assert.AreEqual(client.GetNeuronValueAsDecimal(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[2].{nameof(NeuronTestModel2.test3datas)}[0].{nameof(NeuronTestModel3.decimalValue)}"), (decimal)-234);

            client.SetNeuronValue(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[1].{nameof(NeuronTestModel2.test3datas)}[1].{nameof(NeuronTestModel3.stringValue)}", "/*-258");
            Assert.AreEqual(client.GetNeuronValueAsString(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[1].{nameof(NeuronTestModel2.test3datas)}[1].{nameof(NeuronTestModel3.stringValue)}"), "/*-258");

            var nt2_1 = "nt2_1";
            client.AddANeuron(nt2_1, nameof(NeuronTestModel2));
            client.SetNeuronValue(nt2_1, $"{nameof(NeuronTestModel2.stringValue)}", "159283");

            Assert.AreEqual(client.GetNeuronValueAsInt32("global", "key_count"), 3);
            Assert.AreEqual(client.GetNeuronValueAsString("global", "keys"), "global~nt1_1~nt2_1");
            Assert.AreEqual(client.GetNeuronValueAsInt32("global", "type_count"), 4);

            client.SetNeuronValueToNeuron(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[1]", nt2_1);
            Assert.AreEqual(client.GetNeuronValueAsString(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[1].key"), nt2_1);
            Assert.AreEqual(client.GetNeuronValueAsString(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[1].{nameof(NeuronTestModel2.stringValue)}"), "159283");
            client.SetNeuronValue(nt2_1, $"{nameof(NeuronTestModel2.stringValue)}", "+++---55");
            Assert.AreEqual(client.GetNeuronValueAsString(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[1].{nameof(NeuronTestModel2.stringValue)}"), "+++---55");

            client.RefreshANeuron(nt2_1, nameof(NeuronTestModel2));
            Assert.AreEqual(client.GetNeuronValueAsString(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[1].{nameof(NeuronTestModel2.stringValue)}"), "+++---55");
            Assert.AreEqual(client.GetNeuronValueAsString(nt2_1, $"{nameof(NeuronTestModel2.stringValue)}"), "");

            client.RemoveANeuronFromList(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}", 0);
            Assert.AreEqual(client.GetNeuronValueAsString(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}[0].{nameof(NeuronTestModel2.stringValue)}"), "+++---55");
            Assert.AreEqual(client.GetNeuronValueAsInt32(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}.{{len}}"), 2);
            client.ClearNeuronList(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}");
            Assert.AreEqual(client.GetNeuronValueAsInt32(nt1_1, $"{nameof(NeuronTestModel1.test2datas)}.{{len}}"), 0);

            Assert.IsTrue(client.NeuronExists(nt1_1));
            client.RemoveANeuron(nt1_1);
            Assert.IsFalse(client.NeuronExists(nt1_1));

            var nt3_1 = "nt3_1";
            client.SetOrAddNeuronValue(nt3_1, $"{nameof(NeuronTestModel1)}", $"{nameof(NeuronTestModel1.stringValue)}", "78555478");
            Assert.AreEqual(client.GetNeuronValueAsString(nt3_1, $"{nameof(NeuronTestModel2.stringValue)}"), "78555478");

            var nt5_3 = "nt5_3";
            client.SetOrAddNeuronValue(nt5_3, $"{nameof(NeuronTestModel3)}", $"{nameof(NeuronTestModel3.stringValue)}", "2/3/5855f");
            var nt4_1 = "nt4_1";
            client.SetOrAddNeuronValueToNeuron(nt4_1, $"{nameof(NeuronTestModel1)}", $"{nameof(NeuronTestModel1.model3)}", nt5_3);
            client.SetOrAddNeuronValue(nt5_3, $"{nameof(NeuronTestModel3)}", $"{nameof(NeuronTestModel3.stringValue)}", "5d45s11s51s1f452");
            Assert.AreEqual(client.GetNeuronValueAsString(nt4_1, $"{nameof(NeuronTestModel1.model3)}.{nameof(NeuronTestModel3.stringValue)}"), "5d45s11s51s1f452");

            client.SetNeuronValue(nt4_1, $"{nameof(NeuronTestModel3.stringValue)}", "a");
            client.AddNeuronValue(nt4_1, $"{nameof(NeuronTestModel3.stringValue)}", "b");
            Assert.AreEqual(client.GetNeuronValueAsString(nt4_1, $"{nameof(NeuronTestModel3.stringValue)}"), "ab");

            client.SetNeuronValue(nt4_1, $"{nameof(NeuronTestModel3.decimalValue)}", (decimal)10);
            Assert.AreEqual(client.AddNeuronValue(nt4_1, $"{nameof(NeuronTestModel3.decimalValue)}", (decimal)20.50), 30.50m);
            Assert.AreEqual(client.SubtractNeuronValue(nt4_1, $"{nameof(NeuronTestModel3.decimalValue)}", (decimal)0.25), 30.25m);
            client.SetNeuronValue(nt4_1, $"{nameof(NeuronTestModel3.decimalValue)}", (decimal)2.35);
            Assert.AreEqual(client.MultiplyNeuronValue(nt4_1, $"{nameof(NeuronTestModel3.decimalValue)}", (decimal)5), 11.75m);
            Assert.AreEqual(client.DivideNeuronValue(nt4_1, $"{nameof(NeuronTestModel3.decimalValue)}", (decimal)5), 2.35m);
            client.SetNeuronValue(nt4_1, $"{nameof(NeuronTestModel3.decimalValue)}", (decimal)13);
            Assert.AreEqual(client.ModuloNeuronValue(nt4_1, $"{nameof(NeuronTestModel3.decimalValue)}", (decimal)3), 1);

            Assert.AreEqual(client.AddNeuronValueWithAdd(Guid.NewGuid().ToString(), $"{nameof(NeuronTestModel3)}", $"{nameof(NeuronTestModel3.decimalValue)}", (decimal)20.50), 33.50m);
            Assert.AreEqual(client.SubtractNeuronValueWithAdd(Guid.NewGuid().ToString(), $"{nameof(NeuronTestModel3)}", $"{nameof(NeuronTestModel3.decimalValue)}", (decimal)9.50), 3.50m);
            Assert.AreEqual(client.MultiplyNeuronValueWithAdd(Guid.NewGuid().ToString(), $"{nameof(NeuronTestModel3)}", $"{nameof(NeuronTestModel3.decimalValue)}", (decimal)2.25m), 29.25m);
            Assert.AreEqual(client.DivideNeuronValueWithAdd(Guid.NewGuid().ToString(), $"{nameof(NeuronTestModel3)}", $"{nameof(NeuronTestModel3.decimalValue)}", (decimal)2.50m), 5.2m);
            Assert.AreEqual(client.ModuloNeuronValueWithAdd(Guid.NewGuid().ToString(), $"{nameof(NeuronTestModel3)}", $"{nameof(NeuronTestModel3.decimalValue)}", (decimal)3), 1);

            Assert.AreEqual(client.GetOrAddNeuronValueAsDecimal(Guid.NewGuid().ToString(), $"{nameof(NeuronTestModel3)}", $"{nameof(NeuronTestModel3.decimalValue)}"), 13m);

            string newKey = Guid.NewGuid().ToString();
            Assert.IsNull(client.GetNullableNeuronValueAsDecimal(newKey, $"{nameof(NeuronTestModel3.decimalValue)}"));
            client.SetOrAddNeuronValue(newKey, $"{nameof(NeuronTestModel3)}", $"{nameof(NeuronTestModel3.decimalValue)}", 10m);
            Assert.IsNotNull(client.GetNullableNeuronValueAsDecimal(newKey, $"{nameof(NeuronTestModel3.decimalValue)}"));

            client.Close();

            server.Close();
            Thread.Sleep(1000);
            Assert.IsTrue(stoped);
        }

        public class NeuronTestModel3 : dmuka2.CS.NeoCache.Neuron<NeuronTestModel3>
        {
            [NeuronData]
            public string stringValue = "";
            [NeuronData]
            public byte byteValue = 13;
            public void byteValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (byte* address = &byteValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public sbyte sbyteValue = 13;
            public void sbyteValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (sbyte* address = &sbyteValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public short shortValue = 13;
            public void shortValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (short* address = &shortValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public ushort ushortValue = 13;
            public void ushortValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (ushort* address = &ushortValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public int intValue = 13;
            public void intValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (int* address = &intValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public uint uintValue = 13;
            public void uintValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (uint* address = &uintValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public long longValue = 13;
            public void longValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (long* address = &longValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public ulong ulongValue = 13;
            public void ulongValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (ulong* address = &ulongValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public float floatValue = 13;
            public void floatValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (float* address = &floatValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public double doubleValue = 13;
            public void doubleValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (double* address = &doubleValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public decimal decimalValue = 13;
            public void decimalValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (decimal* address = &decimalValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
        }

        public class NeuronTestModel2 : dmuka2.CS.NeoCache.Neuron<NeuronTestModel2>
        {
            [NeuronData]
            public string stringValue = "";
            [NeuronData]
            public byte byteValue = byte.MinValue;
            public void byteValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (byte* address = &byteValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public sbyte sbyteValue = sbyte.MinValue;
            public void sbyteValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (sbyte* address = &sbyteValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public short shortValue = short.MinValue;
            public void shortValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (short* address = &shortValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public ushort ushortValue = ushort.MinValue;
            public void ushortValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (ushort* address = &ushortValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public int intValue = int.MinValue;
            public void intValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (int* address = &intValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public uint uintValue = uint.MinValue;
            public void uintValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (uint* address = &uintValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public long longValue = long.MinValue;
            public void longValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (long* address = &longValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public ulong ulongValue = ulong.MinValue;
            public void ulongValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (ulong* address = &ulongValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public float floatValue = float.MinValue;
            public void floatValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (float* address = &floatValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public double doubleValue = double.MinValue;
            public void doubleValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (double* address = &doubleValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public decimal decimalValue = decimal.MinValue;
            public void decimalValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (decimal* address = &decimalValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }

            [NeuronData]
            [NeuronList(MaxRowCount = 100)]
            public List<NeuronTestModel3> test3datas = new List<NeuronTestModel3>();
        }

        public class NeuronTestModel1 : dmuka2.CS.NeoCache.Neuron<NeuronTestModel1>
        {
            public NeuronTestModel1()
            {
                model3 = new NeuronTestModel3();
            }

            [NeuronData]
            public string stringValue = "";
            [NeuronData]
            public byte byteValue = byte.MinValue;
            public void byteValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (byte* address = &byteValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public sbyte sbyteValue = sbyte.MinValue;
            public void sbyteValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (sbyte* address = &sbyteValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public short shortValue = short.MinValue;
            public void shortValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (short* address = &shortValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public ushort ushortValue = ushort.MinValue;
            public void ushortValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (ushort* address = &ushortValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public int intValue = int.MinValue;
            public void intValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (int* address = &intValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public uint uintValue = uint.MinValue;
            public void uintValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (uint* address = &uintValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public long longValue = long.MinValue;
            public void longValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (long* address = &longValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public ulong ulongValue = ulong.MinValue;
            public void ulongValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (ulong* address = &ulongValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public float floatValue = float.MinValue;
            public void floatValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (float* address = &floatValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public double doubleValue = double.MinValue;
            public void doubleValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (double* address = &doubleValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }
            [NeuronData]
            public decimal decimalValue = decimal.MinValue;
            public void decimalValue__Pointer(Action<IntPtr> callback, bool set)
            {
                unsafe
                {
                    fixed (decimal* address = &decimalValue)
                    {
                        callback((IntPtr)address);
                    }
                }
            }

            [NeuronData]
            [NeuronList(MaxRowCount = 100)]
            public List<NeuronTestModel2> test2datas = new List<NeuronTestModel2>();
            [NeuronData]
            public NeuronTestModel3 model3 = null;
        }
    }
}