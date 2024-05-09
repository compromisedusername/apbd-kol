using AnimalClinicAPI.DTOs;

namespace AnimalClinicAPI.Repositories;

public interface IAnimalsRepository
{
    Task<bool> AnimalExists(int id);
    Task<GetAnimalsDTO> GetAnimal(int id);
    Task<bool> OwnerExists(int ownerId);
    Task<bool> ProcedureExists(int IdProcedure);
    Task  AddAnimal(AddAnimalDTO addAnimalDto);
}