using Revit_Transform.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Revit_Transform
{
    public partial class TransFormWindow
    {

        public TransFormWindow(TransFormViewModel transFormViewModel)
        {
            InitializeComponent();
            DataContext = transFormViewModel;
        }
    }
}
