using System.Globalization;
using ProjecteSOS_Grup03API.Models;

namespace ProjecteSOS_Grup03API.Tools
{
    public class CsvSustainabilityService
    {
        private readonly string _filePath = "Data/sostenibilitat.csv";

        public CsvSustainabilityService()
        {
            Directory.CreateDirectory("Data");
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "Mes,ComandesPaper,FullsPerComanda,FullsEstalviats,KgPaper,KgCO2\n");
            }
        }

        public List<SustainabilityRecord> GetRecords()
        {
            var lines = File.ReadAllLines(_filePath).Skip(1);
            var records = new List<SustainabilityRecord>();

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                var record = new SustainabilityRecord
                {
                    Month = DateTime.ParseExact(parts[0], "yyyy-MM", CultureInfo.InvariantCulture),
                    PaperOrders = int.Parse(parts[1]),
                    SheetsPerOrder = int.Parse(parts[2])
                };
                records.Add(record);
            }

            return records.OrderByDescending(r => r.Month).ToList();
        }

        public void AddRecord(SustainabilityRecord record)
        {
            var line = $"{record.Month:yyyy-MM},{record.PaperOrders},{record.SheetsPerOrder},{record.SheetsSaved},{record.KgPaperSaved:F2},{record.KgCO2Saved:F2}";
            File.AppendAllText(_filePath, line + "\n");
        }
    }
}
