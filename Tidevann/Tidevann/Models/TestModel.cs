
public class Rootobject
{
    public string skrivemåte { get; set; }
    public string skrivemåtestatus { get; set; }
    public string navnestatus { get; set; }
    public string språk { get; set; }
    public string navneobjekttype { get; set; }
    public int stedsnummer { get; set; }
    public string stedstatus { get; set; }
    public Representasjonspunkt representasjonspunkt { get; set; }
    public Fylker[] fylker { get; set; }
    public Kommuner[] kommuner { get; set; }
}

public class Representasjonspunkt
{
    public float øst { get; set; }
    public float nord { get; set; }
    public int koordsys { get; set; }
}

public class Fylker
{
    public string fylkesnummer { get; set; }
    public string fylkesnavn { get; set; }
}

public class Kommuner
{
    public string kommunenummer { get; set; }
    public string kommunenavn { get; set; }
}
