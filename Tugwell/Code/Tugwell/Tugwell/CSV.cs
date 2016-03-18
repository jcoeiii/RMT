using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics; // to start processes

namespace Tugwell
{
    public class CSV
    {
        public CSV()
        {
            this._sb = new StringBuilder();
            this.IsStarting = true;
        }

        private StringBuilder _sb;
        private bool IsStarting;

        public void StartCol()
        {
            if (!this.IsStarting)
                this._sb.Append(Environment.NewLine);

            this.IsStarting = true;
        }

        public void AddCol(string colText)
        {
            string col = colText.Replace("\"", "\"\"");
            if (!this.IsStarting)
                this._sb.Append(',');

            this._sb.Append('"');
            this._sb.Append(col);
            this._sb.Append('"');

            this.IsStarting = false;
        }

        public bool Save(string filename)
        {
            try
            {
                File.WriteAllText(filename, _sb.ToString());

                this._sb = new StringBuilder();
                this.IsStarting = true;
            }
            catch { return false; }
            return true;
        }

        public bool ShowCSV(string filename)
        {
            try
            {
                Process.Start(filename);
            }
            catch { return false; }
            return true;
        }

        static public List<string> Parse(string input)
        {
            List<string> sb = new List<string>();

            //bool foundComma = false;
            bool foundStartQuote = false;
            char prevChar = ' ';
            char nextChar;
            string elem = "";
            int index = 1;
            //int count = 0;
            foreach (char c in input)
            {
                try { nextChar = input[index]; }
                catch { nextChar = ' '; }

                if (prevChar == '"' && c == '"')
                {
                    prevChar = c;
                    index++;
                    continue;
                }



                if (c == ',')
                {
                    if (!foundStartQuote)
                    {
                        //count++;
                        if (prevChar != '"')
                            sb.Add(elem);
                        elem = "";
                    }
                    else
                    {
                        elem += c;
                    }
                }
                else if (c == '"')
                {
                    if (!foundStartQuote)
                        foundStartQuote = true;
                    //else if (prevChar == '"' && nextChar != ',')
                    //{ }
                    else if (nextChar == '"')
                        elem += c;
                    else
                    {
                        sb.Add(elem);
                        elem = "";
                        //count = 0;
                        foundStartQuote = false;
                    }
                }
                else
                    elem += c;

                prevChar = c;
                index++;
            }
            sb.Add(elem);

            return sb;
        }
    }
}
