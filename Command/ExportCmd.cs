#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Revit_Transform.ViewModel;
using RvtVa3c;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;

#endregion

namespace Revit_Transform
{
    [Transaction(TransactionMode.Manual)]
    public class ExportCmd : IExternalCommand
    {

        System.Reflection.Assembly
         CurrentDomain_AssemblyResolve(
           object sender,
           ResolveEventArgs args)
        {
            if (args.Name.Contains("Newtonsoft"))
            {
                string filename = Path.GetDirectoryName(
                  System.Reflection.Assembly
                    .GetExecutingAssembly().Location);

                filename = Path.Combine(filename,
                  "Newtonsoft.Json.dll");

                if (File.Exists(filename))
                {
                    return System.Reflection.Assembly
                      .LoadFrom(filename);
                }
            }
            return null;
        }

        public void ExportView3D(
         View3D view3d,
         string filename,string outputFolder,  List<ViewSheet> sheets)
        {
            AppDomain.CurrentDomain.AssemblyResolve
              += CurrentDomain_AssemblyResolve;
            Document doc = view3d.Document;

            ExportContext context
              = new ExportContext(doc, filename, outputFolder,  sheets);
            CustomExporter exporter = new CustomExporter(
              doc, context);

            exporter.ShouldStopOnError = false;
            exporter.Export(view3d);
        }

        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            View3D view = doc.ActiveView as View3D;
            if (null == view)
            {
                Util.ErrorMsg(
                  "You must be in a 3D view to export.");

                return Result.Failed;
            }

            ExportViewModel viewModel = new ExportViewModel(uidoc, doc);
             ExportWindow window
                  = new ExportWindow(viewModel);
            bool? showDialog = window.ShowDialog();
            if (showDialog == null || showDialog == false)
            {
                if(viewModel.IsOK)
                {
                    try
                    {
                        List<ViewSheet> sheets = viewModel.AllSheets.Where(s => s.IsChecked).Select(s => s.Sheet).ToList();
                        ExportView3D(view, viewModel.OutputFile,viewModel.OutputFolder, sheets);
                        return Result.Succeeded;
                    }
                    catch (Exception e)
                    {

                        TaskDialog.Show("Error",
                            e.Message);
                        return Result.Failed;
                    }
                }
                return Result.Cancelled;
            }
            return Result.Succeeded;

        }
        
    }
}
