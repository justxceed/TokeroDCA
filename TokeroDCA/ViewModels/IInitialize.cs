namespace TokeroDCA.ViewModels;

public interface IInitialize
{
    Task InitializeAsync(Dictionary<string, object>  parameters=null);
}