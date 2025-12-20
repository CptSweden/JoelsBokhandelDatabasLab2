using DBFJoelsBokhandelLab2.Models;
using DBFJoelsBokhandelLab2.Models.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
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
        private static string? MongoConnectionString;
        private static IMongoDatabase MongoDatabase;

        private static IConfiguration configuration;

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

            configuration = builder.Build();

            ConnectionString = configuration.GetConnectionString("BokhandelDatabase");
            MongoConnectionString = configuration.GetConnectionString("MongoDbConnection");

            try
            {
                var client = new MongoClient(MongoConnectionString);
                MongoDatabase = client.GetDatabase("JoelsBokhandel");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kunde inte ansluta till MongoDB: {ex.Message}");
            }

            bool körProgram = true;

            while (körProgram)
            {
                Console.WriteLine("-- Bokhandel - Lagerhantering --");
                Console.WriteLine("1. (SQL) Visa hela lagersaldot per butik.");
                Console.WriteLine("2. (MongoDb) Visa hela lagersaldot per butik.");
                Console.WriteLine("3. (SQL) Lägg till exemplar i lageret.");
                Console.WriteLine("4. (MongoDb) Lägg till exemplar i lageret.");
                Console.WriteLine("5. (SQL) Ta bort exemplar från lageret.");
                Console.WriteLine("6. (MongoDb) Ta bort exemplar från lageret.");
                Console.WriteLine("7. (MongoDb) Hantera författare.");
                Console.WriteLine("8. Avsluta.");

                Console.Write("Välj ett alternativ (1-8): ");

                string val = Console.ReadLine();

                switch (val)
                {
                    case "1":
                        VisaLagerSQL();
                        Console.WriteLine("\nTryck på Enter för att återgå till huvudmenyn.");
                        Console.ReadLine();
                        break;

                    case "2":
                        VisaLagerMongo();
                        Console.WriteLine("\nTryck på Enter för att återgå till huvudmenyn.");
                        Console.ReadLine();
                        break;

                    case "3":
                        ÖkaLagerSaldoSQL();
                        Console.WriteLine("\nTryck på Enter för att återgå till huvudmenyn.");
                        Console.ReadLine();
                        break;

                    case "4":
                        ÖkaLagerSaldoMongo();
                        Console.WriteLine("\nTryck på Enter för att återgå till huvudmenyn.");
                        Console.ReadLine();
                        break;

                    case "5":
                        MinskaLagerSaldoSQL();
                        Console.WriteLine("\nTryck på Enter för att återgå till huvudmenyn.");
                        Console.ReadLine();
                        break;

                    case "6":
                        MinskaLagerSaldoMongo();
                        Console.WriteLine("\nTryck på Enter för att återgå till huvudmenyn.");
                        Console.ReadLine();
                        break;

                    case "7":
                        HanteraFörfattareMongo();
                        Console.WriteLine("\nTryck på Enter för att återgå till huvudmenyn.");
                        Console.ReadLine();
                        break;

                    case "8":
                        körProgram = false;
                        Console.WriteLine("Avslutar programmet. Tack för att du använde lagerhanteringssystemet!");
                        break;

                    default:
                        Console.WriteLine("Ogiltigt val. Vänligen välj ett alternativ mellan 1 och 4.");
                        break;
                }
            }
        }

        public static void VisaLagerSQL()
        {
            var optionsBuilder = new DbContextOptionsBuilder<BokhandelContext>();
            optionsBuilder.UseSqlServer(Program.ConnectionString);

            using (var context = new BokhandelContext(optionsBuilder.Options))

            {
                Console.WriteLine("---Lagersaldo per butik (SQL)---");

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
                        Console.WriteLine($"-- BUTIK: {ls.Butik.ButiksNamn} --");
                        Console.WriteLine($"========================================");
                        aktuellButik = ls.Butik.ButiksNamn;
                    }

                    Console.WriteLine($"-- {ls.IsbnNavigation.Titel} | ISBN: {ls.Isbn} | Antal: {ls.Antal}");
                }
            }
        }

        public static void VisaLagerMongo()
        {
            Console.WriteLine("\n --- Aktuellt lagersaldo (MongoDb) --- \n");

            try
            {
                var bookCollection = MongoDatabase.GetCollection<Book>("Books");

                var allaBöcker = bookCollection.Find(Builders<Book>.Filter.Empty).ToList();

                if (allaBöcker.Count == 0)
                {
                    Console.WriteLine("Inga böcker hittades i lagersaldot.");
                    return;
                }

                foreach (var bok in allaBöcker)
                {
                    Console.WriteLine($"Titel: {bok.Title}");
                    Console.WriteLine($"Författare: {bok.Author}");
                    Console.WriteLine($"Lagersaldo: {bok.Stock}");
                    Console.WriteLine($"ISBN: {bok.ISBN}");
                    Console.WriteLine($"Pris i SEK: {bok.Price}");
                    Console.WriteLine(new string('-', 20));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel uppstod vid hämtning av lagersaldo från MongoDB: {ex.Message}");
            }
        }

        public static void ÖkaLagerSaldoSQL()
        {
            var optionsBuilder = new DbContextOptionsBuilder<BokhandelContext>();
            optionsBuilder.UseSqlServer(Program.ConnectionString);

            using (var context = new BokhandelContext(optionsBuilder.Options))

                Console.Clear();
            Console.WriteLine("-- Lägg till ny bok (SQL) --");

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

        public static void ÖkaLagerSaldoMongo()
        {
            if (MongoDatabase == null)
            {
                Console.WriteLine("FEL: Databasanslutningen är inte initierad!");
                Console.WriteLine($"Anslutningsträng: {MongoConnectionString ?? "Saknas"}");
                return;
            }
            Console.WriteLine("\n --- Lägg till ny bok (MongoDb) ---");

            Console.Write("Titel: ");
            string titel = Console.ReadLine() ?? "";
            Console.Write("Författare: ");
            string författare = Console.ReadLine() ?? "";
            Console.Write("ISBN: ");
            string isbn = Console.ReadLine() ?? "";
            Console.Write("Pris i SEK: ");
            decimal.TryParse(Console.ReadLine(), out decimal price);
            Console.Write("Lagersaldo: ");
            int.TryParse(Console.ReadLine(), out int stock);

            var nyBok = new Book
            {
                Title = titel,
                Author = författare,
                ISBN = isbn,
                Price = price,
                Stock = stock
            };

            try
            {
                var bookCollection = MongoDatabase.GetCollection<Book>("Books");

                bookCollection.InsertOne(nyBok);

                Console.WriteLine("\nBoken har sparats i MongoDb Atlas.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel uppstod vid sparande av boken i MongoDB: {ex.Message}");
            }
        }

        public static void MinskaLagerSaldoSQL()
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

        public static void MinskaLagerSaldoMongo()
        {
            Console.WriteLine("\n --- Ta bort exemplar ---");
            Console.Write("Ange ISBN: ");
            string isbn = Console.ReadLine()?.Trim() ?? "";
            Console.Write("Ange antal att ta bort: ");
            if (!int.TryParse(Console.ReadLine(), out int antalAttTaBort) || antalAttTaBort <= 0)
            {
                Console.WriteLine("Felaktig inmatning för antal. Återgår till huvudmenyn.");
                return;
            }

            try
            {
                var bookCollection = MongoDatabase.GetCollection<Book>("Books");

                var filter = Builders<Book>.Filter.Eq(b => b.ISBN, isbn);
                var bok = bookCollection.Find(filter).FirstOrDefault();

                if (bok == null)
                {
                    Console.WriteLine($"Fel: Boken med ISBN {isbn} hittades inte i databasen.");
                    return;
                }

                if (bok.Stock < antalAttTaBort)
                {
                    Console.WriteLine($"Varning: Otillräckligt lagersaldo för boken med ISBN {isbn}. Nuvarande saldo: {bok.Stock}, begärt borttagning: {antalAttTaBort}. Ingen åtgärd utförd.");
                    return;
                }

                int nyttSaldo = bok.Stock - antalAttTaBort;

                if (nyttSaldo == 0)
                {
                    bookCollection.DeleteOne(filter);
                    Console.WriteLine($"Lagersaldo raderat: Alla exemplar av boken med ISBN {isbn} har tagits bort från databasen.");
                }
                else
                {
                    var update = Builders<Book>.Update.Set(b => b.Stock, nyttSaldo);
                    bookCollection.UpdateOne(filter, update);

                    Console.WriteLine($"Lagersaldo uppdaterat! {antalAttTaBort} exemplar har tagits bort. Nytt saldo för boken med ISBN {isbn} är nu: {nyttSaldo}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel uppstod vid uppdatering av lagersaldo i MongoDB: {ex.Message}");
            }
        }

        public static void HanteraFörfattareMongo()
        {
            var authorCollection = MongoDatabase.GetCollection<Author>("Authors");

            Console.WriteLine("\n --- Lägg till ny författare (MongoDb) ---");
            Console.WriteLine("1. Visa alla författare");
            Console.WriteLine("2. Lägg till ny författare");
            string val = Console.ReadLine()!;

            if (val == "1")
            {
                var författare = authorCollection.Find(_=> true).ToList();
                foreach (var a in författare)
                {
                    Console.WriteLine($"Namn: {a.Name}, Land: {a.Country}, Född: {a.BirthYear}");
                }
            }
            else if (val == "2")
            {
                Console.Write("Namn: ");
                string name = Console.ReadLine() ?? "";
                Console.Write("Land: ");
                string country = Console.ReadLine() ?? "";
                Console.Write("Födelseår: ");
                int.TryParse(Console.ReadLine(), out int birthYear);

                var nyFörfattare = new Author
                {
                    Name = name,
                    Country = country,
                    BirthYear = birthYear
                };

                authorCollection.InsertOne(nyFörfattare);
                Console.WriteLine("\n Författaren har sparats i MongoDb Atlas.");
            }
        }
    }
}
