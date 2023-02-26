using DragonMaster.Contracts.Characters;

namespace DragonMaster.API.Application.UseCases.Characters;

public class CreateCharacterUseCase
{
    public void CreateCharacter(CreateCharacterRequest? request)
    {
        Console.WriteLine("Nice! Create character");
    }
}