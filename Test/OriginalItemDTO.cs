namespace Test
{
    public class OriginalItemDTO
    {
        public string ItemCode { get; set; }
        public List<AlternativeItem> AlternativeItems { get; set; }

    }

    public class AlternativeItem
    {
        public string AlternativeItemCode { get; set; }
        public double MatchFactor { get; set; }
        public string Remarks { get; set; }

    }
}
