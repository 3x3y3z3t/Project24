/*  Model/Syncable.cs
 *  Version: v1.1 (2023.10.02)
 *  
 *  Author
 *      Arime-chan
 */

using System.Security.Cryptography;
using System.Text;

namespace Project24.Model
{
    /// <summary>
    /// <para>
    /// Defines the general method <c>VersionUp()</c> for a <c>Syncable</c> object,
    /// which is an object that can be synchronized across different data source.
    /// </para>
    ///
    /// <para>
    /// A proper <c>Syncable</c> object should contains a field to store its version information.<br />
    /// Optionally, it should contains a field to store its hash in order to determine if
    /// the object has been modified or not.
    /// </para>
    ///
    /// <para>
    /// <c>VersionUp()</c> method should do neccesary processing and increase version accordingly.<br />
    /// This method is expected to return <c>true</c> if the version has been increased,
    /// and <c>false</c> if the version stay the same.
    /// </para>
    ///
    /// <para>
    /// You can implement your own logic for <c>VersionUp()</c> method, or use the built-in
    /// <see cref="Syncable.VersionUp()"/> and pass in the necessary data.
    /// </para>
    ///
    /// <para>
    /// Your class should derive from <see cref="Syncable"/> as much as possible.<br />
    /// You should only implement this interface when it is not possible to derive from <see cref="Syncable"/>,
    /// or if you want to implement <c>VersionUp()</c> method yourself.
    /// </para>
    /// </summary>
    public interface ISyncable
    {
        public bool VersionUp();
    }

    public abstract class Syncable : ISyncable
    {
        public ulong Version { get => m_Version; set => m_Version = value; }
        public string Hash { get => m_Hash; set => m_Hash = value; }


        public static bool VersionUp(ref ulong _version, ref string _hash, string _objectFieldsValueConcatenatedString)
        {
            byte[] data = Encoding.UTF8.GetBytes(_objectFieldsValueConcatenatedString);

            SHA256 hasher = SHA256.Create();
            byte[] newHashAsBytesArray = hasher.ComputeHash(data);
            string newHash = "";

            foreach (byte b in newHashAsBytesArray)
            {
                newHash += b.ToString("x2");
            }

            if (newHash != _hash)
            {
                ++_version;
                _hash = newHash;

                return true;
            }

            return false;
        }

        public bool VersionUp() => Syncable.VersionUp(ref m_Version, ref m_Hash, GetFieldsValuesConcatenatedInternal());


        protected abstract string GetFieldsValuesConcatenatedInternal();


        private ulong m_Version = 0UL;
        private string m_Hash = null;
    }

}


