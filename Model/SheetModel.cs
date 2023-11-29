using Autodesk.Revit.DB;
using Revit_Transform.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Transform
{
    public class SheetModel  :BaseViewModel
    {

        private bool _IsChecked=true;

        public bool IsChecked
        {
            get { return _IsChecked; }
            set { _IsChecked = value; OnPropertyChanged(); }
        }
        private ViewSheet _Sheet;

        public ViewSheet Sheet
        {
            get { return _Sheet; }
            set { _Sheet = value; }
        }

        public SheetModel(ViewSheet sheet)
        {
            Sheet = sheet; 
        }
    }
}
