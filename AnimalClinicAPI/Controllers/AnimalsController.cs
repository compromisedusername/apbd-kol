using AnimalClinicAPI.DTOs;
using AnimalClinicAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AnimalClinicAPI.Controllers;

[Route("api/animals")]
[ApiController]
public class AnimalsController : ControllerBase
{
    private readonly IAnimalsRepository _animalsRepository;

    public AnimalsController(IAnimalsRepository animalsRepository)
    {
        _animalsRepository = animalsRepository;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnimal(int id)
    {
        if (!await _animalsRepository.AnimalExists(id))
        {
            return NotFound("Animal for id: ("+id+") not found!");
        }

        var result = await _animalsRepository.GetAnimal(id);

        return Ok(result);

    }

    [HttpPost]
    public async Task<IActionResult> AddAnimal(AddAnimalDTO addAnimalDto)
    {
        if (!await _animalsRepository.OwnerExists(addAnimalDto.OwnerId)) return NotFound("Owner for id: (" + addAnimalDto.OwnerId + ") not found!");
        

        var procedures = addAnimalDto.Procedures;
        foreach (var procedure in procedures)
        {
            if (!await _animalsRepository.ProcedureExists(procedure.ProcedureId)) return NotFound("Procedure for id: (" + procedure.ProcedureId + ") not found!");
        }

        await _animalsRepository.AddAnimal(addAnimalDto);
        
        return Created();
    }
}