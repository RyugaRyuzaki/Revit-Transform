using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
using static Revit_Transform.BitmapSourceConverter;
namespace Revit_Transform
{
    public class Startup : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication app)
        {
            CreateRibbonPanel(app.CreateRibbonPanel("OBC Export"));
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication app)
        {
            return Result.Succeeded;
        }

        private void CreateRibbonPanel(RibbonPanel ribbonPanel)
        {
            string path = Assembly.GetExecutingAssembly().Location;

            var pulldownButtonData = new PulldownButtonData("Options", "Add-in Manager");
            var pulldownButton = (PulldownButton)ribbonPanel.AddItem(pulldownButtonData);
        }
    }
}
