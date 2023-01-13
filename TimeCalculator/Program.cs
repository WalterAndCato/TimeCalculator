using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using Syncfusion.XlsIO;
using System.Linq;
using System.Net.Http.Headers;
using TimeCalculatorLib;

namespace chpd
{
    class Program
    {
        // private static int month = 11;
        // private static int year = 2022;
        private static TimeSpan Uebertrag = new TimeSpan(0,0,0);



        static void Main(string[] args)
        {

            Console.WriteLine("Bitte den Dateinamen eingeben:");
            var inputdatei = Console.ReadLine();
            
            //var path = "C:/temp/zeiten/arbeitszeiten.xlsx";
            var path = $"C:/temp/zeiten/{inputdatei}.xlsx";
            if (!File.Exists(path))
            {
                Console.WriteLine($"Die Datei {path} existiert nicht.");
                Console.ReadKey();
                return;
            }
            
            var buchungstext = ExcelImporter.GetExcelLines(path);

            var dres = Kalenderjahr.GetBuchungstage(buchungstext);
            var firstday = dres.Min(dres => dres);
            var lastday = dres.Max(dres => dres);
            var currentmonth = new DateOnly(firstday.Year, firstday.Month,firstday.Day);
            Console.WriteLine(firstday.ToLongDateString());
            Console.WriteLine(lastday.ToLongDateString());
            TimeSpan totaldiff = new TimeSpan();

            do
            {
                var inputmonth = Convert.ToInt32(currentmonth.Month);
                var inputyear = Convert.ToInt32(currentmonth.Year);
                var kalendermonat = new Kalendermonat(inputmonth, inputyear, totaldiff);
                kalendermonat.AddBuchungen(buchungstext);
                
                var header = String.Format("{0,-12}{1,12}{2,12}{3,12}{4,12}{5,12}\n",
                    "Datum", "Soll", "Ist", "Urlaub", "Diff", "Ges");
                Console.WriteLine(header);
                Console.WriteLine($"Monat: {currentmonth.Month}.{currentmonth.Year}");
                foreach (var ktag in kalendermonat.Kalendertage)
                {
                    Console.WriteLine(String.Format(@"{0,12}","----------------------------------------------------------------------------"));
                    if (ktag.Buchungen.Count % 2 != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(String.Format("{0,12:dd.MM.yyyy}{1,12}{2,12}", ktag.Id, "",$"Fehler: Ungerade Anzahl Buchungen: {ktag.Buchungen.Count}"));
                        Console.ForegroundColor = ConsoleColor.White;                        
                    }

                    if (ktag.Feiertag)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(String.Format(@"{0,12:dd.MM.yyyy}{1,12}",ktag.Id,"Feiertag"));
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }
                    
                    var time = ktag.GetTagesArbeitszeit();
                    foreach (var db in ktag.Doppelbuchungen)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(String.Format("{0,12}{1,12}", "", $"Doppelbuchung: {db.ToLongTimeString()}"));
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    
                    TimeSpan differenz = ktag.GetDifferenz();
                    totaldiff = totaldiff.Add(differenz);
                    string differenzstring = differenz.Ticks < 0
                        ? "-" + differenz.ToString(@"hh\:mm")
                        : "+" + differenz.ToString(@"hh\:mm");
                    string totaldiffstring = totaldiff.Ticks < 0
                        ? String.Format("{0:00}", totaldiff.Hours) + ":" + String.Format("{0:00}", Math.Abs(totaldiff.Minutes)) 
                        : String.Format("{0:00}", totaldiff.Hours) + ":" + String.Format("{0:00}", totaldiff.Minutes) ;
                    if (differenz.Ticks < 0) Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (ktag.Urlaub > 0) Console.ForegroundColor = ConsoleColor.Magenta;
                    else Console.ForegroundColor = ConsoleColor.White;
                    var output = String.Format(@"{0,12:dd.MM.yyyy}{1,12:hh\:mm}{2,12:hh\:mm}{3,12:n2}{4,12}{5,12}",
                        ktag.Id, new TimeSpan(8, 0, 0),time, ktag.Urlaub,differenzstring, totaldiffstring);
                    Console.WriteLine(output);
                    Console.ForegroundColor = ConsoleColor.White;
                  //  }
                }
                Console.WriteLine($"Übertrag: {totaldiff}");
                
                
                currentmonth = currentmonth.AddMonths(1);
            } while (currentmonth  < lastday);
            
        }

    }
}