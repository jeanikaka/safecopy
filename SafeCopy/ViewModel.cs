using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace SafeCopy
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ICommand AcceptCommand => new RelayCommandWithoutParameter(Accept);

        private ModelService _modelService;
        public event PropertyChangedEventHandler PropertyChanged;
        public ViewModel(ModelService modelService)
        {
            _modelService = modelService;
        }
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }        
        private bool isLinear;
        public bool IsLinear
        {
            get { return isLinear; }
            set { isLinear = value; OnPropertyChanged("IsLinear"); }
        }
        public void Accept()
        {   
            _modelService.SafeCopy(IsLinear, Step, Count);
        }        
        private bool isRadial;
        public bool IsRadial
        {
            get { return isRadial; }
            set { isRadial = value; OnPropertyChanged("IsRadial");}
        }
        private int step;
        public int Step
        {
            get { return step; }
            set { step = value; OnPropertyChanged("Step"); }
        }
        private int count;
        public int Count
        {
            get { return count; }
            set { count = value; OnPropertyChanged("Count"); }
        }
    }
}
