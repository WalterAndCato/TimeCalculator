namespace TimeCalculatorLib;
using Syncfusion.XlsIO;

public class ExcelImporter
{
    public static string GetExcelLines(string path)
    {
        var engine = new ExcelEngine();
        var result = string.Empty;
        using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var fileValues = engine.Excel.Workbooks.Open(fs);
            if (fileValues == null || fileValues.Worksheets.Count <= 0)
                return string.Empty; // File corrupted
                
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

        return result;
    }
}