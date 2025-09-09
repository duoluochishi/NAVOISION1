using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.DmsTest.Model;
using NV.CT.FacadeProxy;

namespace NV.CT.DmsTest.ViewModel
{
    public class TestHistroyVM : ObservableObject
    {
        public TestHistroyVM()
        {


        }
        //private List<TestHistory> ?m_TestHistoryList { get; set; }

        //public List<TestHistory> ?TestHistoryList
        //{
        //    get { return m_TestHistoryList; }
        //    set { 
        //        m_TestHistoryList = value;
        //        OnPropertyChanged("TestHistoryList");
        //    }
        //}

        //private int m_TestHistoryListSelectedIndex = 0;

        //public int TestHistoryListSelectedIndex
        //{
        //    get { return m_TestHistoryListSelectedIndex;}
        //    set { 
        //        m_TestHistoryListSelectedIndex = value;
        //        OnPropertyChanged("TestHistoryListSelectedIndex");
        //    }
        //}

        //public void TestHistoryListSelectionChanged()
        //{


        //}

        //public event PropertyChangedEventHandler? PropertyChanged;
        //void OnPropertyChanged(string propertyName)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}

        //public void  InitTestHistroyList()
        //{
        //     TestHistoryList = new List<TestHistory>();
        //}
    }
}
