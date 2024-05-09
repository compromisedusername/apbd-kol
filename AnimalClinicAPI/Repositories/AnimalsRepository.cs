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
                  @"SELECT A.ID as AnimalId, A.Name as AnimalName, A.AdmissionDate as AnimalDate,
                    O.LastName as OwnerLastName, O.FirstName as OwnerFirstName, O.ID as OwnerId,
                    P.Name as ProcedureName, P.Description as ProcedureDescription,
                    PA.Date as ProcedureAnimalDate
                    FROM Animal A
                    JOIN dbo.Procedure_Animal PA on A.ID = PA.Animal_ID
                    JOIN dbo.[Procedure] P on P.ID = PA.Procedure_ID
                    JOIN dbo.Owner O on O.ID = A.Owner_ID
                    WHERE A.ID = @IdAnimal";
        
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
        int  ownerLastName = reader.GetOrdinal("OwnerLastName");
        var ownerFirstName = reader.GetOrdinal("OwnerFirstName");
        var procedureName = reader.GetOrdinal("ProcedureName");
        var procedureDescription = reader.GetOrdinal("ProcedureDescription");
        var ownerId = reader.GetOrdinal("OwnerId");
        var procedureDate = reader.GetOrdinal("ProcedureAnimalDate");
        
        

        GetAnimalsDTO animal = null;
        
        while (await reader.ReadAsync())
        {
            if (animal is not null)
            {
                animal.Procedures.Add(new GetProcedureDTO()
                {
                    Description = reader.GetString(procedureDescription),
                    Name = reader.GetString(procedureName)
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
                        Id = reader.GetInt32(ownerId),
                        FirstName = reader.GetString(ownerFirstName),
                        LastName = reader.GetString(ownerLastName),
                    },
                    Procedures = new List<GetProcedureDTO>
                    {
                        new()
                        {
                            Description = reader.GetString(procedureDescription),
                            Name = reader.GetString(procedureName),
                            Date = reader.GetDateTime(procedureDate)
                        }
                    }
                };
            }
        }


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
        var insert = @"INSERT INTO Animal VALUES (@Name, @AdmissionDate, @OwnerId, 1) SELECT @@IDENTITY AS ID;";

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