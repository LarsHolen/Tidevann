using System;
using System.Globalization;
using Xamarin.Forms;

namespace Tidevann.Models
{
    public class TidevannModel
    {
        private string verdi;
        private string flag;
        private string tid;
        private DateTime datoTid;

        private string dag;
        private string klokke;

        private Color bColor = Color.GhostWhite;

        public Color BColor
        {
            get { return bColor; }
            set { bColor = value; }
        }

        public string Klokke
        {
            get { return klokke; }
        }


        public string Dag
        {
            get { return dag; }
        }



        public string Tid
        {
            get { return tid; }
            set 
            {
                if(value == "Out")
                {
                    tid = "";
                    dag = "GPS er utenfor kartverkets dekning";
                    flag = "";
                    verdi = "";
                    return;
                } else if(value == "Loading")
                {
                    tid = "";
                    dag = "Laster inn tidevanns data";
                    flag = "";
                    verdi = "";
                    return;
                }
                datoTid = DateTime.Parse(value);
                dag = datoTid.ToString("dddd", new CultureInfo("nb-No"));
                klokke = datoTid.ToString("HH:mm");
                if(datoTid < DateTime.Now)
                {
                    dag = "Det var " + Flag.ToLower() + ", " + dag + " den " + datoTid.ToString("M", new CultureInfo("nb-No")) + " Klokken: " + klokke;
                    BColor = Color.IndianRed;
                } else if(datoTid.Day == DateTime.Today.Day && datoTid >= DateTime.Now)
                {
                    dag = Flag + " i dag/" + dag + " den " + datoTid.ToString("M", new CultureInfo("nb-No")) + " Klokken: "  + klokke;
                    BColor = Color.LightSeaGreen;
                }
                else
                {
                    if(!string.IsNullOrEmpty(dag))
                    dag = "Det blir " + Flag.ToLower() + ", " + dag + " den " + datoTid.ToString("M", new CultureInfo("nb-No")) + " Klokken: " + klokke;
                    BColor = Color.LightSkyBlue;
                }
                tid = value; 
            }
        }


        public string Flag
        {
            get { return flag; }
            set 
            {
                if (value == "low")
                {
                    flag = "Fjære";
                } else if(value == "high")
                {
                    flag = "Flo";
                } else
                {
                    flag = "Feil ved innlasting av 'flag'.";
                }
            }
        }

        // Vanstann i cm over/under middelmål
        public string Verdi
        {
            get { return verdi; }
            set 
            { 
                if(value.Contains("-"))
                {
                    verdi = "Vannstand under middelnivå: " + value; 
                }
                else
                {
                    verdi = "Vannstand over middelnivå: " + value;
                }
                
            }
        }
    }
}
