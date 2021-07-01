using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredmetniZadatak2.Models
{
    public class Cvor
    {
        public long X { get; set; }
        public long Y { get; set; }
        public Cvor Roditelj { get; set; }

        public Cvor(long x, long y, Cvor roditelj)
        {
            this.Y = y;
            this.Roditelj = roditelj;
            this.X = x;
        }

        public Cvor()
        {

        }

        public override string ToString()
        {
            string str = "";
            str += "( " + X + ", " + Y + " )";
            return str;
        }
    }
}
