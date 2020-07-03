using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2灵魂小助手
{
    public class DotaItems
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string Price { get; set; }
        public string Info { get; set; }
        public string Tips { get; set; }
        public string Attributes { get; set; }
        public string Background { get; set; }

        public string Mana { get; set; }
        public string CoolDown { get; set; }

        public string[] Components { get; set; }

        public string Pic { get; set; }

        public DotaItems() { }

        public DotaItems(string name, string id)
        {
            this.Name = name;
            this.ID = id;
            this.Pic = String.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", id);
            //http://cdn.dota2.com/apps/dota2/images/items/{0}_lg.png
        }
    }
}
