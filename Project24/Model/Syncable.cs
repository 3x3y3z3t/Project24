/*  Model/Syncable.cs
 *  Version: v1.0 (2023.09.28)
 *  
 *  Author
 *      Arime-chan
 */

using System.Security.Cryptography;
using System.Text;

namespace Project24.Model
{
    public abstract class Syncable
    {
        public ulong Version { get; private set; } = 0UL;
        public string Hash { get; private set; } = null;


        public bool VersionUp()
        {
            byte[] data = Encoding.UTF8.GetBytes(GetFieldsValuesConcatenatedInternal());

            SHA256 hasher = SHA256.Create();
            byte[] newHashAsBytesArray = hasher.ComputeHash(data);
            string newHash = "";

            foreach(byte b in newHashAsBytesArray)
            {
                newHash += b.ToString("x2");
            }

            if (newHash != Hash)
            {
                ++Version;
                Hash = newHash;

                return true;
            }
            
            return false;
        }

        protected abstract string GetFieldsValuesConcatenatedInternal();
    }
}
