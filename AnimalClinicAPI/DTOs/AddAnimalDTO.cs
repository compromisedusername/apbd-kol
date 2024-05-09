namespace AnimalClinicAPI.DTOs;

public class AddAnimalDTO
{
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public int OwnerId { get; set; }
    public List<AddProcedureDTO> Procedures { get; set; }
}