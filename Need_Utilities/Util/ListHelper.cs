// ListHelper.cs
// Copyright 2015
// 
// Project Lead: Need
// Contact:      
//     Mail:     mailto:needdragon@gmail.com 
//     Twitter: https://twitter.com/NeedDragon

using System;
using System.Collections.Generic;

namespace Need_Utilities.Util {
    public class ListHelper {
        public static Boolean ListContainsArray(List<object[]> arrays, object[] arrayToSearch) {
            foreach(object[] bytes in arrays) {
                if(ArrayEqualsArray(bytes, arrayToSearch)) return true;
            }
            return false;
        }

        public static Boolean ArrayEqualsArray(object[] first, object[] second) {
            if((first == null && second != null) || (second == null && first != null)) return false;
            if(first == null && second == null) return true;
            if(first.Length != second.Length) return false;
            for(int i = 0; i < first.Length; i++) {
                if(first[i] != second[i]) return false;
            }
            return true;
        }

        public static Boolean Helper_ByteArrayIsEmpty(byte[] array) {
            if(array == null) return true;
            for(int i = 0; i < array.Length; i++) {
                if(array[i] != 0) return false;
            }
            return true;
        } 
    }
}