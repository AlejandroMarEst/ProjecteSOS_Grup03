namespace ProjecteSOS_Grup03API.Models
{
    public class SustainabilityRecord
    {
        public DateTime Month { get; set; }
        public int PaperOrders { get; set; }
        public int SheetsPerOrder { get; set; }

        public int SheetsSaved => PaperOrders * SheetsPerOrder;
        public double KgPaperSaved => SheetsSaved / 500.0 * 2.5;
        public double KgCO2Saved => KgPaperSaved * 1.3;
    }
}
