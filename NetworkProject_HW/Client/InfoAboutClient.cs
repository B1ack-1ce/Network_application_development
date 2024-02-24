using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Client
{
    internal class InfoAboutClient
    {
        public string Name { get; set; } = string.Empty;
        public Guid ID { get; set; } = Guid.NewGuid();
        public override string ToString()
        {
            return $"Name: {Name} | Id {ID}";
        }
    }
}
