
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Input;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using Autodesk.Revit.DB.Architecture;
using Document = Autodesk.Revit.DB.Document;
using System.Windows;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Autodesk.Revit.Creation;
using System.IO;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Web.UI.WebControls;

namespace Revit_Transform.ViewModel
{

    public class TransFormViewModel : BaseViewModel
    {
        public UIDocument UiDoc { get; }

        public Document Doc { get; }
        
        private static readonly Formatting formatting = Formatting.Indented;
       
       

        public bool IsConnected { get; set; } = false;
        public string documentId { get; set; }

        public JsonSerializerSettings settings { get; set; }
        public List<ElementId> previousElementIds { get; set; }

        public Options geomOptions { get; set; } = new Options();


        public ICommand OpenWebCommand { get; set; }
        public ICommand CloseWindowCommand { get; set; }
        public ICommand ConnectServerCommand { get; set; }
        public ICommand SynchronizeCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public TransFormViewModel(UIDocument uiDoc, Document doc)
        {
            UiDoc = uiDoc;
            Doc = doc;
            documentId = Doc.ActiveView.UniqueId.ToString();
            settings = new JsonSerializerSettings();
            settings.NullValueHandling
                 = NullValueHandling.Ignore;

            OpenWebCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                Process.Start(new ProcessStartInfo(StaticInfo.navigateUri));

            }); 
            CloseWindowCommand = new RelayCommand<TransFormWindow>((p) => { return true; }, (p) =>
            {
                p.Close();
            });



            ConnectServerCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
               

            });
            SynchronizeCommand = new RelayCommand<object>((p) => { return IsConnected; }, (p) =>
            {
               

            });
            UpdateCommand = new RelayCommand<object>((p) => { return IsConnected; }, (p) =>
            {
               

            });
           
        }

       
    }
    
}
