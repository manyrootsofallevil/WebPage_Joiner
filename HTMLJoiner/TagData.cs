using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTMLJoiner
{
    public class TagData
    {
        string name, tag, type, content;

        public TagData() { }

        public TagData(string name, string tag, string type, string content)
        {
            this.name = name;
            this.tag = tag;
            this.type = type;
            this.content = content;
        }


        public string Name { get; set; }
        public string Tag { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
    }
}
