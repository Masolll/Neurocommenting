namespace Neurocommenting.IONet;

public class MessageDto
{
    public string Role { get; init; }
    public string Content { get; init; }
    public MessageDto(string role, string content)
    {
        Role = role;
        Content = content;
    }
}
