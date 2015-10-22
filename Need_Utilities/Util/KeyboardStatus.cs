// KeyboardStatus.cs
// Copyright 2015
// 
// Project Lead: Need
// Contact:      
//     Mail:     mailto:needdragon@gmail.com 
//     Twitter: https://twitter.com/NeedDragon

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Need_Utilities.Util {
    public static class KeyboardStatus {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        public static byte[] GetStatus(Boolean keyCurrentActive) {
            byte[] array = new byte[256];
            for(int i = 0; i < array.Length; i++) {
                char character = (char)i;
                short status = GetAsyncKeyState(character);
                byte leastSignificantBit = (byte)(status & 0xFF);
                byte mostSignificantBit = (byte)((status & 0xFF00) >> 15);
                if(!keyCurrentActive)
                    array[i] = mostSignificantBit;
                else 
                    array[i] = leastSignificantBit;
            }

            return array;
        }

        public static byte GetVirtualKeyCode(Keys key) {
            int value = (int)key;
            return (byte)(value & 0xFF);
        }
    }
}