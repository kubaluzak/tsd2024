using GoldSavings.App.Model;
using GoldSavings.App.Services;
using System.Xml.Serialization;

namespace GoldSavings.App;

class Program
{
    static void Main(string[] args)
    {

        GoldDataService dataService = new GoldDataService();

        DateTime endDate = DateTime.Now.Date;
        DateTime startDate = endDate.AddYears(-1);

        List<GoldPrice> goldPrices = dataService
            .GetGoldPrices(startDate, endDate)
            .GetAwaiter()
            .GetResult();

        if (goldPrices.Count == 0)
        {
            Console.WriteLine("No data found. Exiting.");
            return;
        }

        Console.WriteLine($"Retrieved {goldPrices.Count} records.");

        GoldAnalysisService analysisService = new GoldAnalysisService(goldPrices);
        var avgPrice = analysisService.GetAveragePrice();
        Console.WriteLine($"\nAverage price: {Math.Round(avgPrice, 2)}");


        var highestMethod = goldPrices
            .OrderByDescending(p => p.Price)
            .Take(3)
            .ToList();

        var lowestMethod = goldPrices
            .OrderBy(p => p.Price)
            .Take(3)
            .ToList();

        var highestQuery =
            (from p in goldPrices
                orderby p.Price descending
                select p)
            .Take(3)
            .ToList();

        var lowestQuery =
            (from p in goldPrices
                orderby p.Price
                select p)
            .Take(3)
            .ToList();

        Console.WriteLine("\nTOP 3 highest prices in the last year (method syntax):");
        foreach (var p in highestMethod)
            Console.WriteLine($"{p.Date:yyyy-MM-dd} -> {p.Price}");

        Console.WriteLine("\nTOP 3 lowest prices in the last year (method syntax):");
        foreach (var p in lowestMethod)
            Console.WriteLine($"{p.Date:yyyy-MM-dd} -> {p.Price}");

        Console.WriteLine("\nTOP 3 highest prices in the last year (query syntax):");
        foreach (var p in highestQuery)
            Console.WriteLine($"{p.Date:yyyy-MM-dd} -> {p.Price}");

        Console.WriteLine("\nTOP 3 lowest prices in the last year (query syntax):");
        foreach (var p in lowestQuery)
            Console.WriteLine($"{p.Date:yyyy-MM-dd} -> {p.Price}");


        DateTime fullStartDate = new DateTime(2019, 1, 1);
        DateTime fullEndDate = DateTime.Now.Date;

        List<GoldPrice> fullGoldPrices = GetGoldPricesInChunks(dataService, fullStartDate, fullEndDate);

        if (fullGoldPrices.Count == 0)
        {
            Console.WriteLine("\nNo extended data found. Exiting.");
            return;
        }


        var january2020 = fullGoldPrices
            .Where(p => p.Date.Year == 2020 && p.Date.Month == 1)
            .ToList();

        var profitableDays = january2020
            .SelectMany(buy => fullGoldPrices
                .Where(sell => sell.Date > buy.Date && sell.Price > buy.Price * 1.05)
                .Select(sell => new
                {
                    BuyDate = buy.Date,
                    BuyPrice = buy.Price,
                    SellDate = sell.Date,
                    SellPrice = sell.Price,
                    ProfitPercent = ((sell.Price - buy.Price) / buy.Price) * 100
                }))
            .OrderBy(x => x.SellDate)
            .ToList();

        Console.WriteLine("\n2b. More than 5% profit after buying in January 2020:");

        if (profitableDays.Any())
        {
            foreach (var item in profitableDays.Take(50))
            {
                Console.WriteLine(
                    $"Buy: {item.BuyDate:yyyy-MM-dd} ({item.BuyPrice}) -> " +
                    $"Sell: {item.SellDate:yyyy-MM-dd} ({item.SellPrice}) | " +
                    $"Profit: {item.ProfitPercent:F2}%");
            }
        }
        else
        {
            Console.WriteLine("No days found with profit above 5%.");
        }


        var secondTen = fullGoldPrices
            .Where(p => p.Date.Year >= 2019 && p.Date.Year <= 2022)
            .OrderByDescending(p => p.Price)
            .Skip(10)
            .Take(3)
            .ToList();

        Console.WriteLine("\n2c. Dates that open the second ten of the ranking (places 11-13) for 2019-2022:");
        foreach (var p in secondTen)
            Console.WriteLine($"{p.Date:yyyy-MM-dd} -> {p.Price}");


        var years = new[] { 2020, 2023, 2024 };

        var averages =
            (from p in fullGoldPrices
                where years.Contains(p.Date.Year)
                group p by p.Date.Year
                into g
                orderby g.Key
                select new
                {
                    Year = g.Key,
                    Avg = g.Average(x => x.Price)
                })
            .ToList();

        Console.WriteLine("\n2d. Average prices:");
        foreach (var a in averages)
            Console.WriteLine($"{a.Year}: {a.Avg:F2}");

        var filtered = fullGoldPrices
            .Where(p => p.Date.Year >= 2020 && p.Date.Year <= 2024)
            .OrderBy(p => p.Date)
            .ToList();

        if (filtered.Count > 0)
        {
            var min = filtered[0];
            var bestBuy = min;
            var bestSell = min;
            double bestProfit = 0;

            foreach (var current in filtered)
            {
                if (current.Price - min.Price > bestProfit)
                {
                    bestProfit = current.Price - min.Price;
                    bestBuy = min;
                    bestSell = current;
                }

                if (current.Price < min.Price)
                    min = current;
            }

            var roi = (bestSell.Price - bestBuy.Price) / bestBuy.Price * 100.0;

            Console.WriteLine("\n2e. Best trade between 2020 and 2024:");
            Console.WriteLine($"Buy: {bestBuy.Date:yyyy-MM-dd} -> {bestBuy.Price}");
            Console.WriteLine($"Sell: {bestSell.Date:yyyy-MM-dd} -> {bestSell.Price}");
            Console.WriteLine($"ROI: {roi:F2}%");
        }
        else
        {
            Console.WriteLine("\n2e. No data found for years 2020-2024.");
        }


        string xmlPath = "goldPrices.xml";
        SaveToXml(fullGoldPrices, xmlPath);
        Console.WriteLine($"\nSaved {fullGoldPrices.Count} records to XML: {xmlPath}");


        var pricesFromXml = LoadFromXml(xmlPath);
        Console.WriteLine($"Read from XML: {pricesFromXml.Count} records");

        Console.WriteLine("\nDone.");
        
        
        Console.WriteLine("Chapter 2");
        Console.WriteLine("Task 1");
            
        Func<int, bool> isLeapYear = year =>
            (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);

        Console.WriteLine($"2024 -> {isLeapYear(2024)}");
        Console.WriteLine($"2023 -> {isLeapYear(2023)}");
        Console.WriteLine($"2000 -> {isLeapYear(2000)}");
        Console.WriteLine($"1900 -> {isLeapYear(1900)}");

        Console.WriteLine();
        Console.WriteLine("Task 2");

        RandomizedList<int> numbers = new RandomizedList<int>();

        Console.WriteLine($"Is empty: {numbers.IsEmpty()}");

        numbers.Add(10);
        numbers.Add(20);
        numbers.Add(30);
        numbers.Add(40);
        numbers.Add(50);

        Console.WriteLine($"Is empty after adding elements: {numbers.IsEmpty()}");
        Console.WriteLine($"Collection: {numbers}");

        Console.WriteLine($"Get(0): {numbers.Get(0)}");
        Console.WriteLine($"Get(2): {numbers.Get(2)}");
        Console.WriteLine($"Get(4): {numbers.Get(4)}");

        Console.WriteLine();
        Console.WriteLine("Done.");
    }

    static List<GoldPrice> GetGoldPricesInChunks(GoldDataService dataService, DateTime startDate, DateTime endDate)
    {
        var allPrices = new List<GoldPrice>();
        const int chunkSize = 360;

        DateTime currentStart = startDate;

        while (currentStart <= endDate)
        {
            DateTime currentEnd = currentStart.AddDays(chunkSize - 1);
            if (currentEnd > endDate)
                currentEnd = endDate;

            var chunk = dataService
                .GetGoldPrices(currentStart, currentEnd)
                .GetAwaiter()
                .GetResult();

            if (chunk != null && chunk.Count > 0)
                allPrices.AddRange(chunk);

            currentStart = currentEnd.AddDays(1);
        }

        return allPrices
            .OrderBy(p => p.Date)
            .ToList();
    }

    static void SaveToXml(List<GoldPrice> prices, string filePath)
    {
        var serializer = new XmlSerializer(typeof(List<GoldPrice>));
        using var fs = new FileStream(filePath, FileMode.Create);
        serializer.Serialize(fs, prices);
    }

    static List<GoldPrice> LoadFromXml(string filePath) =>
        (List<GoldPrice>)new XmlSerializer(typeof(List<GoldPrice>)).Deserialize(new FileStream(filePath, FileMode.Open))
        !;
}