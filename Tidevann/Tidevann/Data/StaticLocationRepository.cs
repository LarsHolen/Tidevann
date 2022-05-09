using System;
using System.Collections.Generic;
using System.Text;
using Tidevann.Models;

namespace Tidevann.Data
{
    public static  class StaticLocationRepository
    {
        private static List<Location> locList = new List<Location>()
        {
            new Location("Vadsø", 70.08, 29.73),
            new Location("Honningsvåg", 70.98, 25.97),
            new Location("Tromsø", 69.69, 18.98),
            new Location("Andøya", 69.08, 15.76),
            new Location("Narvik", 68.44, 17.48),
            new Location("Røst", 67.52, 12.11),
            new Location("Bodø", 67.27, 14.44),
            new Location("Sandnessjøen", 66.02, 12.62),
            new Location("Brønnøysund", 65.48, 12.19),
            new Location("Namsos", 64.46, 11.51),
            new Location("Hitra", 63.56, 8.73),
            new Location("Verdal", 63.79, 11.44),
            new Location("Kristiansund", 63.11, 7.73),
            new Location("Sunndalsøra", 62.68, 8.55),
            new Location("Ålesund", 62.48, 6.14),
            new Location("Måløy", 61.93, 5.10),
            new Location("Florø", 61.59, 5.04),
            new Location("Høyanger", 61.21, 6.07),
            new Location("Gulen", 60.98, 5.08),
            new Location("Bergen", 60.40, 5.31),
            new Location("Bømlo", 59.80, 5.24),
            new Location("Haugesund", 59.40, 5.27),
            new Location("Stavanger", 58.97, 5.74),
            new Location("Lindesnes", 57.98, 7.05),
            new Location("Kristiansand", 58.15, 8.03),
            new Location("Arendal", 58.46, 8.78),
            new Location("Kragerø", 58.86, 9.41),
            new Location("Sandefjord", 59.13, 10.22),
            new Location("Horten", 59.41, 10.48),
            new Location("Oslo", 59.91, 10.73),
            new Location("Fredrikstad", 59.21, 10.91)
        };
        public static List<Location> LocList 
        {
            get { return locList; } 
         
        }
    }
}
