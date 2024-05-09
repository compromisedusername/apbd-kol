using System.Data;
using System.Data.SqlClient;
using AnimalClinicAPI.DTOs;

namespace AnimalClinicAPI.Repositories;

public class AnimalsRepository : IAnimalsRepository
{
    private readonly IConfiguration _configuration;

    public AnimalsRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> AnimalExists(int id)
    {
        var query = @"SELECT 1 FROM Animal WHERE ID = @IdAnimal";
        
        await using var  connection =  new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();
        
        command.CommandText = query;
        command.Connection = connection;
        command.Parameters.AddWithValue("IdAnimal", id);

        await connection.OpenAsync();
        
        var  res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<GetAnimalsDTO> GetAnimal(int id)
    {
        var query = 
                  @"SELECT ID as AnimalId, Name as AnimalName, AdmissionDate as AnimalDate,
                    O.LastName as OwnerLastName, O.FirstName as OwnerFirstName,
                    P.Name as ProcedureName, P.Description as ProcedureDescription
                    FROM Animal 
                    JOIN dbo.Procedure_Animal PA on Animal.ID = PA.Animal_ID
                    JOIN dbo.[Procedure] P on P.ID = PA.Procedure_ID
                    JOIN dbo.Owner O on O.ID = Animal.Owner_ID
                    WHERE ID = @IdAnimal";
        
        await using var  connection =  new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();
        command.CommandText = query;
        command.Connection = connection;
        command.Parameters.AddWithValue("IdAnimal", id);

        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();

        var animalId = reader.GetOrdinal("AnimalId");
        var animalName = reader.GetOrdinal("AnimalName");
        var animalDate = reader.GetOrdinal("AnimalDate");
        var OwnerLastName = reader.GetOrdinal("OwnerLastName");
        var OwnerFirstName = reader.GetOrdinal("OwnerFirstName");
        var ProcedureName = reader.GetOrdinal("ProcedureName");
        var ProcedureDescription = reader.GetOrdinal("ProcedureDescription");
        
        

        GetAnimalsDTO animal = null;
        
        while (await reader.ReadAsync())
        {
            if (animal is not null)
            {
                animal.Procedures.Add(new GetProcedureDTO()
                {
                    Description = reader.GetString(ProcedureDescription),
                    Name = reader.GetString(ProcedureName)
                });
            }
            else
            {
                animal = new GetAnimalsDTO()
                {
                    Id = reader.GetInt32(animalId),
                    Name = reader.GetString(animalName),
                    AdmissionDate = reader.GetDateTime(animalDate),
                    Owner = new OwnerDTO()
                    {
                        FirstName = reader.GetString(OwnerFirstName),
                        LastName = reader.GetString(OwnerLastName),
                    },
                    Procedures = new List<GetProcedureDTO>()
                };
            }
        }

        if (animal is null) throw new Exception();

        return animal;

    }

    public async Task<bool> OwnerExists(int ownerId)
    {
        var query = @"SELECT 1 FROM Owner WHERE ID = @IdOwner";
        
        await using var  connection =  new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();
        
        command.CommandText = query;
        command.Connection = connection;
        command.Parameters.AddWithValue("IdOwner", ownerId);

        await connection.OpenAsync();
        
        var  res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> ProcedureExists(int IdProcedure)
    {
        var query = @"SELECT 1 FROM [Procedure] WHERE ID = @IdProcedure";
        
        await using var  connection =  new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();
        
        command.CommandText = query;
        command.Connection = connection;
        command.Parameters.AddWithValue("IdProcedure", IdProcedure);

        await connection.OpenAsync();
        
        var  res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task AddAnimal(AddAnimalDTO addAnimalDto)
    {
        var insert = @"INSERT INTO Animal VALUES (@Name, @AdmissionDate, @OwnerId) SELECT @@IDENTITY AS ID;";

        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = insert;
        
        command.Parameters.AddWithValue("Name", addAnimalDto.Name);
        command.Parameters.AddWithValue("AdmissionDate", addAnimalDto.Date);
        command.Parameters.AddWithValue("OwnerId", addAnimalDto.OwnerId);

        await connection.OpenAsync();

        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;


        try
        {
            var id = await command.ExecuteScalarAsync();
            foreach (var procedure in addAnimalDto.Procedures)
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO Procedure_Animal VALUES (@ProcedureId,@AnimalId,@ProcedureDate)";
                command.Parameters.AddWithValue("ProcedureId", procedure.ProcedureId);
                command.Parameters.AddWithValue("AnimalId", id);
                command.Parameters.AddWithValue("ProcedureDate", procedure.Date);
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();

        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(e);
            throw;
        }
        
        

    }
}