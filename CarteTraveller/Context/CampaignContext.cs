
public interface ICampaignContext
{
    string CurrentCampaignPath { get; set; }
    // Tu pourrais plus tard ajouter un événement ici, ex: event Action OnCampaignChanged;
}

public class CampaignContext : ICampaignContext
{
    public string CurrentCampaignPath { get; set; } = string.Empty;
}