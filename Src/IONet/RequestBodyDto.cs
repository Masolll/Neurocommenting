namespace Neurocommenting.IONet;

public class RequestBodyDto
{
    public string Model { get; init; }
    public List<MessageDto> Messages { get; init; }

    public RequestBodyDto(string model, List<MessageDto> messages)
    {
        Model = model;
        Messages = messages;
    }
}
