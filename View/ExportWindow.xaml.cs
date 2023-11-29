using Revit_Transform.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Revit_Transform
{
    public partial class ExportWindow
    {
        public ExportWindow(ExportViewModel ExportViewModel)
        {
            InitializeComponent();
            DataContext = ExportViewModel;
        }
    }
}
