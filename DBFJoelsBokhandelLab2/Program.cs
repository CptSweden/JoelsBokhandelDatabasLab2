using DBFJoelsBokhandelLab2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.Common;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DBFJoelsBokhandelLab2
{
    public class Program
    {
        private static string ConnectionString;
        private static IConfiguration configuration;

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

            IConfiguration configuration = builder.Build();
            ConnectionString = configuration.GetConnectionString("BokhandelDatabase");

            bool körProgram = true;

            while (körProgram)
            {
                Console.WriteLine("-- Bokhandel - Lagerhantering --");
                Console.WriteLine("1. Visa hela lagersaldot per butik.");
                Console.WriteLine("2. Lägg till exemplar i lageret.");
                Console.WriteLine("3. Ta bort exemplar från lageret.");
                Console.WriteLine("4. Avsluta.");

                Console.Write("Välj ett alternativ (1-4): "); 
                
                string val = Console.ReadLine();
                
                switch (val)
                {
                    case "1":
                        VisaLager();
                        Console.WriteLine("\nTryck på Enter för att återgå till huvudmenyn.");
                        Console.ReadLine();
                        break;

                    case "2":
                        ÖkaLagerSaldo();
                        Console.WriteLine("\nTryck på Enter för att återgå till huvudmenyn.");
                        Console.ReadLine();
                        break;

                    case "3":
                        MinskaLagerSaldo();
                        Console.WriteLine("\nTryck på Enter för att återgå till huvudmenyn.");
                        Console.ReadLine();
                        break;

                    case "4":
                        körProgram = false;
                        Console.WriteLine("Avslutar programmet. Tack för att du använde lagerhanteringssystemet!");
                        break;

                    default:
                        Console.WriteLine("Ogiltigt val. Vänligen välj ett alternativ mellan 1 och 4.");
                        break;
                }
            }
        }

        public static void VisaLager() 
        {
            var optionsBuilder = new DbContextOptionsBuilder<BokhandelContext>();
            optionsBuilder.UseSqlServer(Program.ConnectionString);

            using (var context = new BokhandelContext(optionsBuilder.Options))

            {
                Console.WriteLine("---Lagersaldo per butik---");

                var lagersaldo = context.LagerSaldos
                    .Include(ls => ls.Butik)
                    .Include(ls => ls.IsbnNavigation)
                    .OrderBy(ls => ls.Butik.ButiksNamn)
                    .ToList();

                string aktuellButik = "";

                foreach (var ls in lagersaldo)
                {
                    if (ls.Butik.ButiksNamn != aktuellButik)
                    {
                        Console.WriteLine($"\n========================================");
                        Console.WriteLine($"-- BUTIK: { ls.Butik.ButiksNamn} --");
                        Console.WriteLine($"========================================");
                        aktuellButik = ls.Butik.ButiksNamn;
                    }

                    Console.WriteLine($"-- {ls.IsbnNavigation.Titel} | ISBN: {ls.Isbn} | Antal: {ls.Antal}");
                }
            }       
        }

        public static void ÖkaLagerSaldo()
        {
            var optionsBuilder = new DbContextOptionsBuilder<BokhandelContext>();
            optionsBuilder.UseSqlServer(Program.ConnectionString);

            using (var context = new BokhandelContext(optionsBuilder.Options))

                Console.Clear();
            Console.WriteLine("-- Lägg till exemplar --");

            Console.Write("Ange ButikID: ");
            if (!int.TryParse(Console.ReadLine(), out int butikId))
            {
                Console.WriteLine("Felaktig inmatning för ButikID. Återgår till huvudmenyn.");
                return;
            }

            Console.Write("Ange ISBN (13 tecken): ");
            string isbn = Console.ReadLine().Trim();

            Console.Write("Ange antal exemplar att lägga till: ");
            if (!int.TryParse(Console.ReadLine(), out int antalAttLäggaTill) || antalAttLäggaTill <= 0)
            {
                Console.WriteLine("Felaktig inmatning för antal. Återgår till huvudmenyn.");
                return;
            }

            try
            {

                using (var context = new BokhandelContext(optionsBuilder.Options))
                {
                    var lagerPost = context.LagerSaldos
                        .FirstOrDefault(ls => ls.ButikId == butikId && ls.Isbn == isbn);

                    if (lagerPost != null)
                    {
                        lagerPost.Antal += antalAttLäggaTill;
                        Console.WriteLine($"Lagersaldo uppdaterat! Nytt antal för {isbn} i butik {butikId} är nu: {lagerPost.Antal}");
                    }
                    else
                    {
                        var nyLagerPost = new LagerSaldo
                        {
                            ButikId = butikId,
                            Isbn = isbn,
                            Antal = antalAttLäggaTill
                        };

                        context.LagerSaldos.Add(nyLagerPost);
                        Console.WriteLine($"Nytt lagersaldo skapat: {antalAttLäggaTill} exemplar av {isbn} har lagts till i butik {butikId}.");
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel uppstod: {ex.Message}");
            }
            Console.WriteLine("Tryck på Enter för att återgå till huvudmenyn.");
            Console.ReadLine();
        }

        public static void MinskaLagerSaldo()
        {
            var optionsBuilder = new DbContextOptionsBuilder<BokhandelContext>();
            optionsBuilder.UseSqlServer(Program.ConnectionString);

            using (var context = new BokhandelContext(optionsBuilder.Options))

                Console.Clear();
            Console.WriteLine("-- Ta bort exemplar --");

            Console.Write("Ange ButikID: ");
            if (!int.TryParse(Console.ReadLine(), out int butikId))
            {
                Console.WriteLine("Felaktig inmatning för ButikID. Återgår till huvudmenyn.");
                return;
            }

            Console.Write("Ange ISBN (13 tecken): ");
            string isbn = Console.ReadLine().Trim();

            Console.Write("Ange antal exemplar att ta bort: ");
            if (!int.TryParse(Console.ReadLine(), out int antalAttTaBort) || antalAttTaBort <= 0)
            {
                Console.WriteLine("Felaktig inmatning för antal. Återgår till huvudmenyn.");
                return;
            }


            try
            {
                using (var context = new BokhandelContext(optionsBuilder.Options))
                {
                    var lagerPost = context.LagerSaldos
                        .FirstOrDefault(ls => ls.ButikId == butikId && ls.Isbn == isbn);

                    if (lagerPost == null)
                    {
                        Console.WriteLine($"Fel: Boken (ISBN: {isbn}) hittades inte i butik {butikId}, ingen återgärd utförd.");
                        return;
                    }

                    if (lagerPost.Antal < antalAttTaBort)
                    {
                        Console.WriteLine($"Varning: Otillräckligt lagersaldo för boken (ISBN: {isbn}) i butik {butikId}. Nuvarande saldo: {lagerPost.Antal}, begärt borttagning: {antalAttTaBort}. Ingen återgärd utförd.");
                        return;
                    }

                    lagerPost.Antal -= antalAttTaBort;

                    if (lagerPost.Antal == 0)
                    {
                        context.LagerSaldos.Remove(lagerPost);
                        Console.WriteLine($"Lagersaldo raderat: Alla exemplar av {isbn} har tagits bort från butik {butikId}.");
                    }
                    else
                    {
                        lagerPost.Antal -= antalAttTaBort;
                        context.SaveChanges();
                        lagerPost.Antal += antalAttTaBort;
                        Console.WriteLine($"Lagersaldo uppdaterat! {antalAttTaBort} har tagits bort. Nytt antal för {isbn} i butik {butikId} är nu: {lagerPost.Antal}");
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel uppstod: {ex.Message}");
            }
            Console.WriteLine("Tryck på Enter för att återgå till huvudmenyn.");
            Console.ReadLine();
        }
    }
}
