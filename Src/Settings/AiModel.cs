namespace Neurocommenting.Settings;

public class AiModel
{
    public required int Priority { get; set; }
    public required string Id { get; set; }
    public bool IsBlocked => BlockUntill > DateTime.Now;
    
    public DateTime BlockUntill { get; set; }
}