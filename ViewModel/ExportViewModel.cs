
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

    public class ExportViewModel : BaseViewModel
    {
        public UIDocument UiDoc { get; }

        public Document Doc { get; }
        
        private string _OutputFile;

        public string OutputFile
        {
            get { return _OutputFile; }
            set { _OutputFile = value; OnPropertyChanged(); }
        } 
        private string _OutputFolder;

        public string OutputFolder
        {
            get { return _OutputFolder; }
            set { _OutputFolder = value; OnPropertyChanged(); }
        }
        

        private bool _CheckAllSheets=true;

        public bool CheckAllSheets
        {
            get { return _CheckAllSheets; }
            set { 
                _CheckAllSheets = value;
                OnPropertyChanged();
                if (AllSheets != null && AllSheets.Count > 0)
                {
                    foreach (var sheet in AllSheets)
                    {
                        sheet.IsChecked = value;
                    }
                }
            }
        }

        private ObservableCollection<SheetModel> _AllSheets;

        public ObservableCollection<SheetModel> AllSheets
        {
            get { return _AllSheets; }
            set { _AllSheets = value; OnPropertyChanged(); }
        }



        public bool IsConnected { get; set; } = false;
        public string documentId { get; set; }
        public JsonSerializerSettings settings { get; set; }
        public List<ElementId> previousElementIds { get; set; }

        public Options geomOptions { get; set; } = new Options();

      



        public bool IsOK { get; set; }


        public ICommand OpenWebCommand { get; set; }
        public ICommand CloseWindowCommand { get; set; }
        public ICommand BrowseFileCommand { get; set; }
        public ICommand ApplyCommand { get; set; }
        public ExportViewModel(UIDocument uiDoc, Document doc)
        {
            UiDoc = uiDoc;
            Doc = doc;
            AllSheets = GetAllSheetProject();
            documentId = Doc.ActiveView.UniqueId.ToString();
            settings = new JsonSerializerSettings();
            settings.NullValueHandling
                 = NullValueHandling.Ignore;


            OutputFile = GetSaveFileName();


            BrowseFileCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                BrowseFile();

            });
            OpenWebCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                Process.Start(new ProcessStartInfo(StaticInfo.navigateUri));

            }); 
            CloseWindowCommand = new RelayCommand<ExportWindow>((p) => { return true; }, (p) =>
            {
                IsOK = false; 
                p.DialogResult = false;

            });
            ApplyCommand = new RelayCommand<ExportWindow>((p) => { return true; }, (p) =>
            {
                IsOK = true;
                p.DialogResult = false;
            });
        }

        
        private string GetSaveFileName()
        {
            string filename = Doc.PathName;

            if (0 == filename.Length)
            {
                filename = Doc.Title;
            }
            if (null == OutputFolder)
            {

                try
                {
                    OutputFolder = Path.GetDirectoryName(
                   filename);
                }
                catch
                {
                    return string.Empty;
                }
            }

            filename = Path.GetFileNameWithoutExtension(filename) + ".gz";

            filename = Path.Combine(OutputFolder,
              filename);
            return filename;
        }

        private ObservableCollection<SheetModel> GetAllSheetProject()
        {
            ObservableCollection<SheetModel> sheetModels = new ObservableCollection<SheetModel>();
            List<ViewSheet> viewSheets 
                = new FilteredElementCollector(Doc).OfClass(typeof(ViewSheet)).Cast<ViewSheet>().ToList();
            foreach (var item in viewSheets)
            {
                sheetModels.Add(new SheetModel(item));
            }
            return sheetModels;
        }

        private void BrowseFile()
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "Select  Output File";
            dlg.Filter = "GZip files|*.gz";
            //dlg.Filter = "JSON files|*.json";

            if (null != OutputFolder
              && 0 < OutputFolder.Length)
            {
                dlg.InitialDirectory = OutputFolder;
            }

            dlg.FileName = OutputFile;

            bool rc = DialogResult.OK == dlg.ShowDialog();

            if (rc)
            {
                OutputFile = Path.Combine(dlg.InitialDirectory,
                  dlg.FileName);

                OutputFolder = Path.GetDirectoryName(
                  OutputFile);
            }
        }
    }
    
}
