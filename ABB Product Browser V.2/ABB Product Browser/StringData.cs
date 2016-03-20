using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProductBrowser
{
    /// <summary>
    /// A class that contains strings and values read from files.
    /// </summary>
    public class StringData
    {
        /// <summary>Variable of the type string containing the label of the line.</summary>
        public string stringTag { set; get; }
        /// <summary>Variable of the type string containing the value of the line.</summary>
        public string stringValue { set; get; }

        /// <summary>
        /// Constructor sets the initial values of the data fields.
        /// </summary>
        public StringData()
        {
            stringTag = null;
            stringValue = null;
        }

        /// <summary>
        /// Method that sets the data fields values to match input.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="d"></param>
        public StringData(string s, string d)
        {
            stringTag = s;
            stringValue = d;
        }
    }
}

