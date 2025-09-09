using NV.CT.NP.Tools.DataTransfer.Model;
using NV.CT.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.NP.Tools.DataTransfer.ViewModel
{
    public class DummyMainWindowViewModel : BaseViewModel
    {
        private ObservableCollection<VStudyModel> _vStudies = new();
        private VStudyModel? _selectedItem;

        public ObservableCollection<VStudyModel> VStudies
        {
            get => _vStudies;
            set
            {
                _vStudies = value;
                RaisePropertyChanged();
            }
        }

        public VStudyModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
            }
        }

        public DummyMainWindowViewModel()
        {
            CreateDummyData();
        }

        private void CreateDummyData()
        {
            for (int i = 0; i < 20; i++)
            {
                VStudies.Add(new VStudyModel()
                {
                    PatientName = $"TestPatien{i}",
                    PatientId = $"PID_0215{i}",
                    StudyTime = DateTime.Now.AddHours(i),
                    BodyPart = "Chest",
                    ExportStatus = ExportStatus.Success
                });
            }
        }
    }
}
