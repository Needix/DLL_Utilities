// ByteTransformHelper.cs
// Copyright 2015
// 
// Project Lead: Need
// Contact:      
//     Mail:     mailto:needdragon@gmail.com 
//     Twitter: https://twitter.com/NeedDragon

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Need_Utilities.Util.RAM {
    public class ByteTransformHelper {
        public static byte[] Transform_HexStringTOByteArray(String arrayOfBytes) {
            string[] split = arrayOfBytes.Split(' ');
            List<Byte> bytes = new List<byte>();
            if(split.Length == 0) {
                for(int i = 0; i < arrayOfBytes.Length; i += 2) {
                    string byteString = arrayOfBytes.Substring(i, 2);
                    Int64 intParse = Int64.Parse(byteString, NumberStyles.HexNumber);
                    if(intParse > Byte.MaxValue) {
                        byte[] range = Transform_HexAddressTOByteArray(byteString, false);
                        bytes.AddRange(range);
                    } else {
                        bytes.Add(Convert.ToByte(int.Parse(byteString, NumberStyles.HexNumber)));
                    }
                }
            } else {
                for(int i = 0; i < split.Length; i++) {
                    Int64 intParse = Int64.Parse(split[i], NumberStyles.HexNumber);
                    if(intParse > Byte.MaxValue) {
                        byte[] range = Transform_HexAddressTOByteArray(split[i], false);
                        bytes.AddRange(range);
                    } else {
                        bytes.Add(Convert.ToByte(int.Parse(split[i], NumberStyles.HexNumber)));
                    }
                }
            }

            return bytes.ToArray();
        }

        public static byte[] Transform_StringTOByteArray(string text) {
            byte[] chars = new byte[text.Length];
            for (int i = 0; i < text.Length; i++) {
                chars[i] = (Byte)text[i];
            }
            return chars;
        }

        public static Byte[] Transform_HexAddressTOByteArray(string hexAddress, bool reverseAddress) {
            if(hexAddress.Length%2 == 1) hexAddress = "0" + hexAddress;
            Byte[] bytes = new byte[hexAddress.Length/2];
            for(int i = 0; i < hexAddress.Length; i+=2) {
                String curByte = hexAddress.Substring(i, 2);
                bytes[i/2] = Convert.ToByte(int.Parse(curByte, NumberStyles.HexNumber));
            }
            return reverseAddress ? ReverseArray(bytes) : bytes;
        }

        public static string Transform_ByteArrayTOHexString(byte[] bytes) {
            String result = "";
            for(int i = 0; i < bytes.Length; i++) {
                String curString = ((int)bytes[i]).ToString("X");
                if(curString.Length == 1) curString = "0"+curString;
                if(i + 1 < bytes.Length) curString += " ";
                result += curString;
            }
            return result;
        }
        public static IntPtr Transform_ByteArrayTOPointer(byte[] bytes) {
            bytes = ReverseArray(bytes);
            String resultString = "";
            for(int i = 0; i < bytes.Length; i++) {
                String curString = bytes[i].ToString("X");
                if(curString.Length == 1) curString = "0"+curString;
                resultString += curString;
            }
            return new IntPtr(Int64.Parse(resultString, NumberStyles.HexNumber));
        }

        public static byte[] Transform_ValueTOByteArray(object p, Type type) {
            byte[] bytes;
            if(type == typeof(Int32)) {
                int value = Convert.ToInt32(p);
                bytes = BitConverter.GetBytes(value);
            } else if(type == typeof(Int64)) {
                Int64 value = Convert.ToInt64(p);
                bytes = BitConverter.GetBytes(value);
            } else if(type == typeof(long)) {
                long value = Convert.ToInt64(p);
                bytes = BitConverter.GetBytes(value);
            } else if(type == typeof(float)) {
                float value = Convert.ToSingle(p);
                bytes = BitConverter.GetBytes(value);
            } else if(type == typeof(byte)) {
                byte value = Convert.ToByte(p);
                bytes = BitConverter.GetBytes(value);
            } else {
                throw new InvalidOperationException("Could not transform "+p+" to "+type);
            }
            return bytes;
        }
        public static object Transform_ByteArrayTOType(byte[] array, Type type) {
            object result;
            if(type == typeof(Int32)) {
                result = BitConverter.ToInt32(array, 0);
            } else if(type == typeof(Int64)) {
                result = BitConverter.ToInt64(array, 0);
            } else if(type == typeof(float)) {
                result = BitConverter.ToSingle(array, 0);
            } else if(type == typeof(IntPtr)) {
                result = new IntPtr(BitConverter.ToInt32(array, 0));
            } else {
                result = "Error: no representation possible for " + Transform_ByteArrayTOHexString(array)+" into type "+type;
            }
            return result;
        }

        public static String ReverseAddress(IntPtr ptr) { return ReverseAddress(ptr.ToInt32().ToString("X")); }
        public static String ReverseAddress(string address) {
            byte[] addrByteArray = Transform_HexStringTOByteArray(address);
            byte[] reversedArray = ReverseArray(addrByteArray);
            string result = Transform_ByteArrayTOHexString(reversedArray);
            return result;
        }

        public static byte[] ReverseArray(byte[] org) { 
            if(org == null) return null;
            byte[] result = new byte[org.Length];
            for(int i = 0; i < result.Length; i++) {
                result[i] = org[result.Length - 1 - i];
            }
            return result;
        }

        public static byte[] ConnectByteArrays(List<Byte[]> arrays) { return ConnectByteArrays(arrays.ToArray()); }
        public static byte[] ConnectByteArrays(Byte[][] arrays) {
            String completeArray = "";
            foreach(byte[] array in arrays) { completeArray += Transform_ByteArrayTOHexString(array)+" "; }
            completeArray = completeArray.Substring(0, completeArray.Length - 1);
            return Transform_HexStringTOByteArray(completeArray);
        }

        public static IntPtr Helper_SearchArrayOfBytesInProcess(IntPtr processPointer, string stringBytes) {
            byte[] arrayOfBytes = Transform_HexStringTOByteArray(stringBytes);

            //Min and Max Address of process
            Kernel32Import.SYSTEM_INFO sys_info = new Kernel32Import.SYSTEM_INFO();
            Kernel32Import.GetSystemInfo(out sys_info);
            IntPtr procMinAddress = sys_info.minimumApplicationAddress;
            IntPtr procMaxAddress = sys_info.maximumApplicationAddress;
            long procMinAddressL = (long)procMinAddress;
            long procMaxAddressL = (long)procMaxAddress;

            while(procMinAddressL < procMaxAddressL && processPointer!=IntPtr.Zero) {
                //Find next memory region
                Kernel32Import.MEMORY_BASIC_INFORMATION mem_basic_info = new Kernel32Import.MEMORY_BASIC_INFORMATION();
                int success = Kernel32Import.VirtualQueryEx(processPointer, procMinAddress, out mem_basic_info, 28);
                if(success == 0) {
                    Debug.WriteLine("Injecter| SearchArrayOfBytes| VirtualQueryEx Error: "+new Win32Exception(Kernel32Import.GetLastError()).Message);
                    break;
                }

                //Check if memory page is readable & reserved to target
                if((mem_basic_info.Protect == Kernel32Import.PAGE_READONLY || mem_basic_info.Protect==Kernel32Import.PAGE_EXECUTE_READ) 
                    && mem_basic_info.State == Kernel32Import.MEM_COMMIT) {

                    byte[] bytes = RAMAccess.Memory_ReadProcessMemory(processPointer,
                        new IntPtr(mem_basic_info.BaseAddress), mem_basic_info.RegionSize); //Read complete region
                    if(bytes == null) continue;

                    //Debug.WriteLine("Injecter| PageAccess: "+(proc_min_address_l < 0x7723FF)+"/"+(proc_min_address_l + mem_basic_info.RegionSize > 0x7723FF)+": "+(proc_min_address_l).ToString("X"));
                    for(int i = 0; i < bytes.Length; i++) {
                        int readByteInc = i;
                        for(int j = 0; j < arrayOfBytes.Length; j++) {
                            byte curReadByte = bytes[readByteInc];
                            byte curFindByte = arrayOfBytes[j];
                            if(curReadByte == curFindByte) {
                                readByteInc++;
                                if(j + 1 >= arrayOfBytes.Length)
                                    return new IntPtr(mem_basic_info.BaseAddress + i);
                            } else
                                break;
                        }
                    }
                }

                //Move to the next memory chunk
                procMinAddressL += mem_basic_info.RegionSize;
                procMinAddress = new IntPtr(procMinAddressL);
            }
            Debug.WriteLine("Injecter| Could not find array of bytes!");
            return IntPtr.Zero;
        }
    }
}