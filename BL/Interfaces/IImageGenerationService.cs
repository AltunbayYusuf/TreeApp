namespace IntegratieProject.BL.Interfaces;

public interface IImageGenerationService
{
    Task<byte[]> GenerateProjectImageAsync(string title, string description, int? subPlatformId = null);}