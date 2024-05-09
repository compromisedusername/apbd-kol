using System.Buffers;

namespace AnimalClinicAPI.DTOs;

public class GetAnimalsDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime AdmissionDate { get; set; }
    public OwnerDTO Owner { get; set; } = null!;
    public List<GetProcedureDTO> Procedures { get; set; } = new();
}