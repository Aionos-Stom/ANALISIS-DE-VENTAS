namespace ETLPROYECTOELECT1.Interfaces
{
    public interface IDataTransformer
    {
        List<T> CleanData<T>(List<T> data);
        List<T> RemoveDuplicates<T>(List<T> data);
        bool ValidateIntegrity<T>(List<T> data);
    }
}
