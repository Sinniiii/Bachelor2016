using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows;

namespace ProductBrowser
{
    public class ImportModule
    {
        [ImportMany(typeof(ResourceDictionary))]
        public IEnumerable<Lazy<ResourceDictionary, IDictionary<string, object>>> ResourceDictionaryList { get; set; }
    }
}
