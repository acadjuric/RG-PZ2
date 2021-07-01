using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredmetniZadatak2.Models
{
    public class LineEntity
    {

        public long ID { get; set; }
        public string Name { get; set; }
        public long StartNodeID { get; set; }
        public long EndNodeID { get; set; }

        public LineEntity()
        {

        }


    }
}
