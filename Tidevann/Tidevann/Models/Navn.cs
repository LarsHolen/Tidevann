using System;
using System.Collections.Generic;
using System.Text;

namespace Tidevann.Models
{
    public class Navn
    {
        public string Skrivemåte { get; set; }
        public string Navneobjekttype { get; set; }
        public Fylker[] Fylker { get; set; }
        public Representasjonspunkt Representasjonspunkt { get; set; }
    }
}
