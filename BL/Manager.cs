using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.interfaces;
using IntegratieProject.DAL.Interfaces;

namespace IntegratieProject.BL;

public class Manager : IManager
{
    
    private readonly IRepository _repository;

    public Manager( IRepository repository)
    {
        _repository = repository;
    }
    
    public void ValidateEntity(Object model)
    {
        var validationResults = new List<ValidationResult>();
        bool success = Validator.TryValidateObject(model,
            new ValidationContext(model), validationResults, true);

        if (!success)
        {
            var message = "";
            foreach (var validationResult in validationResults)
            {
                message += validationResult.ErrorMessage + " ";
            }

            throw new ValidationException(message);
        }
    }
}