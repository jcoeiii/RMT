using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace Tugwell
{
    public class logFile
    {
        private FileStream _file = null;

        public logFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    _file = File.Open(path, FileMode.OpenOrCreate | FileMode.Truncate, FileAccess.ReadWrite);
                else
                    _file = File.Open(path, FileMode.Create, FileAccess.ReadWrite);
            }
            catch { }
        }

        ~logFile()
        {
            if (_file != null)
                _file.Close();
        }

        private void write_to_file(string line)
        {
            if (_file != null)
            {
                string data = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt | ") + line + Environment.NewLine;
                _file.Write(Encoding.ASCII.GetBytes(data), 0, data.Length);
            }
        }

        public void append(string line)
        {
            write_to_file(line);
        }

        public void append(params string[] strings)
        {
            foreach (string line in strings)
                write_to_file(line);
        }
    }
}
