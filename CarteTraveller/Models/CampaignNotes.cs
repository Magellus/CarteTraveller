namespace CarteTraveller.Models
{
    public class CampaignNotes
    {
        public bool IsVisited { get; set; } = false;
        public string GmNotes { get; set; } = string.Empty;
        public string PlayerKnownInfo { get; set; } = string.Empty;

        // Pratique pour ajouter des rumeurs au fil de la campagne
        public List<string> Rumors { get; set; } = new();
    }
}
