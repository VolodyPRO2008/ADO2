namespace CountriesLinq
{
    public class City
    {
        public int    Id         { get; set; }
        public string Name       { get; set; }
        public int    Population { get; set; }
        public bool   IsCapital  { get; set; }
    }

    public class Country
    {
        public int    Id         { get; set; }
        public string Name       { get; set; }
        public string Continent  { get; set; }
        public double AreaKm2    { get; set; }
        public long   Population { get; set; }
        public int?   CapitalId  { get; set; }
    }
}