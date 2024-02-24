using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Client
{
    internal class InfoAboutClient
    {
        public string Name { get; set; }
        public Guid ID { get; set; }
        [JsonIgnore]
        public StreamWriter Writer { get; set; } 
        public override string ToString()
        {
            return $"Name: {Name} | Id {ID}";
        }
    }
}
