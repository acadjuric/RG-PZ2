using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredmetniZadatak2.Models
{
    public class Podeok
    {
        public bool Zauzet { get; set; }
        public long CvorID { get; set; }
       
        public Podeok()
        {
            this.Zauzet = false;
            this.CvorID = 0;
        }
    }
}
