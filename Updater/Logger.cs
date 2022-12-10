/*  Logger.cs
 *  Version: 1.0 (2022.12.10)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.IO;

namespace Updater
{
    class Logger : IDisposable
    {
        public Logger(string _fullname, bool _append = false)
        {
            if (!_append)
            {
                if (File.Exists(_fullname))
                    File.Delete(_fullname);
            }

            m_Writer = new StreamWriter(File.Open(_fullname, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite));

            if (_append)
            {
                m_Writer.BaseStream.Seek(m_Writer.BaseStream.Length, SeekOrigin.Begin);
                //m_Writer.WriteLine();
            }
        }

        // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources;
        ~Logger()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(_disposing: false);
        }


        public void Close()
        {
            if (m_Writer != null)
            {
                m_Writer.Close();
            }
        }

        public void WriteLine(string _message)
        {
            if (m_Writer != null)
            {
                string datetime = string.Format("[{0:yyyy}.{0:MM}.{0:dd} {0:HH}:{0:mm}:{0:ss}.{0:fff}] ", DateTime.Now);
                m_Writer.WriteLine(datetime + _message);
                m_Writer.Flush();
            }

            Console.WriteLine(_message);
        }

        protected virtual void Dispose(bool _disposing)
        {
            if (m_IsDisposed)
                return;

            if (_disposing)
            {
                // dispose managed state (managed objects);
            }

            // free unmanaged resources (unmanaged objects) and override finalizer;
            // set large fields to null;

            if (m_Writer != null)
            {
                m_Writer.Close();
                m_Writer = null;
            }

            m_IsDisposed = true;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(_disposing: true);
            GC.SuppressFinalize(this);
        }


        private StreamWriter m_Writer = null;
        private bool m_IsDisposed = false;
    }

}
