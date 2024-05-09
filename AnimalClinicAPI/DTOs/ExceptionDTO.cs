using System.Text.Json;

namespace AnimalClinicAPI.DTOs;

public class ExceptionDTO
{
    public string Message { get; init; }
    public string StatusCode { get; init; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}