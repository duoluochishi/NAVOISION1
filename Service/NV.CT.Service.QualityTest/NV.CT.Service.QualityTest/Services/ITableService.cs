using NV.CT.Service.QualityTest.Models;

namespace NV.CT.Service.QualityTest.Services
{
    public interface ITableService
    {
        TablePosition GetSavedTablePosition();
        void SaveCurrentTablePosition();
    }
}