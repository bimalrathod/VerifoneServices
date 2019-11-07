using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace VerifoneServices
{
    public static class TypeExtensions
    {
        public static string Encript(this string value)
        {
            return Encriptions.EncryptSHA(value);
        }

        public static string Encript(this int value)
        {
            return Encriptions.EncryptSHA(value.ToString());
        }

        public static string Encript(this long value)
        {
            return Encriptions.EncryptSHA(value.ToString());
        }

        public static string Decript(this string value)
        {
            return Encriptions.DecryptSHA(value);
        }
        public static string DecriptIOS(this string value)
        {
            return Encriptions.DecryptIOS(value);
        }
        public static string EncryptIOS(this string value)
        {
            return Encriptions.EncryptIOS(value);
        }
        public static int DecriptToInt(this string value)
        {
            return int.Parse(Encriptions.DecryptSHA(value));
        }

        public static long DecriptToLong(this string value)
        {
            return long.Parse(Encriptions.DecryptSHA(value));
        }



    }




}