namespace TokeroDCA.Models;

public class DCASetup
{
    public DateTime StartDate { get; set; }
    public int DayOfMonth { get; set; }
    public List<DCASetupItem> ItemsToInvestIn { get; set; }
}