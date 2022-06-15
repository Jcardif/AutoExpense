using System.Security.Cryptography;
using System.Text;

namespace AutoExpense.Android.Extensions
{
    public static class ExtensionMethods
    {
        public static string ToHashedStringValue(this string source)
        {
            //Create a byte array from source data.
            var tmpSource = Encoding.ASCII.GetBytes(source);

            //Compute hash based on source data.
            var tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);

            //The tmpHash byte array now holds the computed hash value (128-bit value=16 bytes) for the source data.
            //return the value like this as a hexadecimal string

            return tmpHash.ByteArrayToString();
        }


        private static string ByteArrayToString(this byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i < arrInput.Length; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
    }
}