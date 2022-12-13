using System.Globalization;

namespace TimeCalculatorLib;

public class Kalendermonat
{
    public TimeSpan Uebertrag { get; private set; }
    public List<Kalendertag> Kalendertage { get; private set; }
    public int Monat { get; private set; }
    public int Jahr { get; private set; }
    
    public Kalendermonat(int month, int year,TimeSpan _uebertrag)
    {
        Uebertrag = _uebertrag;
        Monat = month;
        Jahr = year;
        
        Kalendertage = new List<Kalendertag>();
        float Urlaubswert = 0f;
        var wochentage = Kalendertag.GetDates(year,month);
        foreach (var tag in wochentage)
        {
            Urlaubswert = 0f;
            if (tag.Ticks > DateTime.Now.Ticks) break;
            if (Kalenderjahr.Urlaubstage.Exists(x =>
                    x.Item1.Day == tag.Day && x.Item1.Month == tag.Month && x.Item1.Year == tag.Year))
            {
                Urlaubswert = Kalenderjahr.Urlaubstage.First(x =>
                    x.Item1.Day == tag.Day && x.Item1.Month == tag.Month && x.Item1.Year == tag.Year).Item2;

            }

            Kalendertage.Add(new Kalendertag(tag,Urlaubswert));
        }
    }

    public void AddBuchungen(string buchungstext)
    {
        var first = true;
        foreach (var line in buchungstext.Replace("\r", "").Split("\n"))
        {
            if (line.Replace(";", "") == "")
                continue;
                
            if (first)
            {
                first = false;
                continue;
            }
                
            var fields = line.Split(";");
                
            if (fields.Length < 2)
                continue;
                
            if (!fields[2].StartsWith("K", StringComparison.InvariantCultureIgnoreCase))
                continue;

            var buchung = DateTime.Parse(fields[11], CultureInfo.CurrentCulture);
            var aktuellertag = new DateTime(buchung.Year, buchung.Month, buchung.Day);

            if (buchung > new DateTime(Jahr, Monat, 1).AddMonths(1)) continue;

            // var kalendertagfirst = Kalendertage.First(x =>
            //     x.Id.Month == aktuellertag.Month && x.Id.Day == aktuellertag.Day &&
            //     x.Id.Year == aktuellertag.Year);
            Kalendertage.First(x => (x.Id.Month == aktuellertag.Month && x.Id.Day == aktuellertag.Day && x.Id.Year == aktuellertag.Year)).Buchungen.Add(buchung);
        }
    }

}