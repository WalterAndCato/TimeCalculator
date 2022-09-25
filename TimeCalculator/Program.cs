using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Syncfusion.XlsIO;

namespace chpd
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = "D:/Arbeitszeiten/1.xlsx";
            var engine = new ExcelEngine();
            var result = "";

            using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var fileValues = engine.Excel.Workbooks.Open(fs);
                if (fileValues == null || fileValues.Worksheets.Count <= 0)
                    return; // File corrupted
                
                fileValues.Worksheets[0].EnableSheetCalculations();
                using (Stream s = new MemoryStream())
                {
                    fileValues.Worksheets[0].SaveAs(s, ";");
                    s.Seek(0, SeekOrigin.Begin);
                    
                    using (var sr = new StreamReader(s))
                    {
                        //This allows you to do one Read operation.
                        result = sr.ReadToEnd();
                    }
                }
            }

            var dict = new Dictionary<DateTime, List<DateTime>>();
            var first = true;

            foreach (var line in result.Replace("\r", "").Split("\n"))
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

                var changed = DateTime.Parse(fields[11], CultureInfo.CurrentCulture);
                var key = new DateTime(changed.Year, changed.Month, changed.Day);
                
                if (!dict.ContainsKey(key))
                    dict.Add(key, new List<DateTime>{changed});
                else
                    dict[key].Add(changed);
            }

            foreach (var item in dict)
            {
                if (item.Value.Count % 2 != 0)
                    Console.WriteLine($"Fehler bei Tag {item.Key:dd.MM.yyyy}: Ungerade Zahl Änderungen: {item.Value.Count}");
                else
                {
                    var time = TimeSpan.Zero; 
                    var isStart = false;
                    var tempStart = DateTime.Now;

                    foreach (var val in item.Value)
                    {
                        // ReSharper disable once AssignmentInConditionalExpression
                        if (isStart = !isStart)
                        {
                            tempStart = val;
                        }
                        else
                        {
                            time += (val - tempStart);
                        }
                    }
                    
                    Console.WriteLine($"{item.Key:dd.MM.yyyy}: {time}");
                }
            }
        }
    }
}