using System.Globalization;

namespace TimeCalculatorLib;

public class Kalenderjahr
{
    public static List<(DateTime,float)> Urlaubstage = new List<(DateTime,float)>{
        (new DateTime(2022,09,19), 1f),
        (new DateTime(2022,10,10), 0.5f),
        (new DateTime(2022,10,17), 1f),
        (new DateTime(2022,10,31), 1f),
        (new DateTime(2022,11,18), 1f),
        (new DateTime(2022,11,21), 1f)
    };
    
    public static List<DateOnly> GetBuchungstage(string buchungstext)
    {
        var result = new List<DateOnly>();
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
            result.Add(new DateOnly(aktuellertag.Year,aktuellertag.Month,aktuellertag.Day));
        }

        return result;
    }
    
}