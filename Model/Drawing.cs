using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Document = Autodesk.Revit.DB.Document;
using Autodesk.Revit.DB;

namespace Revit_Transform
{
    

    public class Drawing
    {
        private static readonly BuiltInCategory TitleBlockCategory = (BuiltInCategory)(-2000280);


        public class ConvertPosition
        {
            public double x { get; set; }
            public double y { get; set; }
            public double z { get; set; }
            public ConvertPosition(XYZ pos)
            {
                x = pos.X;
                y = pos.Y;
                z = pos.Z;
            }
            public ConvertPosition(XYZ pos, UnitProject unit)
            {
                x = unit.Convert(pos.X);
                y = unit.Convert(pos.Y);
                z = unit.Convert(pos.Z);
            }
        }
        public class DrawingView
        {
            public string Name { get; set; }
            public string uuid { get; set; }
            public bool isViewPlan { get; set; } = false;
            public bool CropView { get; set; } = false;
            public double Scale { get; set; } = 0;
            public double Elevation { get; set; } = 0;
            public double CutPlane { get; set; } = 0;
            public double TopClipPlane { get; set; } = 0;
            public double BottomClipPlane { get; set; } = 0;
            public ConvertPosition OutlineCenter { get; set; }
            public ConvertPosition OutlineMax { get; set; }
            public ConvertPosition OutlineMin { get; set; }
            public ConvertPosition CropBoxMax { get; set; }
            public ConvertPosition CropBoxMin { get; set; }
            public DrawingView(Document doc, Viewport viewport, View view, UnitProject unit)
            {
                Name = view.Name;
                uuid = view.UniqueId;
                GetElevation(doc, view, unit);
                XYZ Center = viewport.GetBoxCenter();

                OutlineCenter = new ConvertPosition(Center, unit);
                Outline outline = viewport.GetBoxOutline();

                OutlineMax = new ConvertPosition(outline.MaximumPoint, unit);
                OutlineMin = new ConvertPosition(outline.MinimumPoint, unit);
                BoundingBoxXYZ bounding = view.CropBox;
                CropBoxMax = new ConvertPosition(bounding.Max, unit);
                CropBoxMin = new ConvertPosition(bounding.Min, unit);
            }
            private void GetElevation(Document doc, View view, UnitProject unit)
            {
                Scale = view.Scale;
                if (view.ViewType != ViewType.FloorPlan) return;
                PlanViewRange planViewRange = (view as ViewPlan).GetViewRange();
                if (planViewRange == null) return;
                Level level = doc.GetElement(planViewRange.GetLevelId(PlanViewPlane.CutPlane)) as Level;
                if (level == null) return;
                isViewPlan = true;
                Elevation = unit.Convert(level.Elevation);
                CutPlane = unit.Convert(planViewRange.GetOffset(PlanViewPlane.CutPlane));
                TopClipPlane = unit.Convert(planViewRange.GetOffset(PlanViewPlane.TopClipPlane));
                BottomClipPlane = unit.Convert(planViewRange.GetOffset(PlanViewPlane.BottomClipPlane));
                CropView = view.CropBoxActive;
            }

        }
        [DataMember]
        public string Name { get; set; } = "";
        [DataMember]
        public string uuid { get; set; } = "";
        [DataMember]
        public ConvertPosition origin { get; set; }
        [DataMember]
        public List<DrawingView> views { get; set; } = new List<DrawingView>();
        [DataMember]
        public bool IsPlaneView { get; set; }
        public Drawing(Document doc, ViewSheet sheet, UnitProject unit)
        {
            InitialOrigin(doc, sheet, unit);
            GetViewInSheet(doc, sheet, unit);
        }


        private void InitialOrigin(Document doc, ViewSheet sheet, UnitProject unit)
        {
            uuid = sheet.UniqueId;
            Name = sheet.SheetNumber + "-" + sheet.Name;
            Element TitleBlock = new FilteredElementCollector(doc).OwnedByView(sheet.Id).WhereElementIsNotElementType().OfCategory(TitleBlockCategory).Cast<Element>().FirstOrDefault();
            LocationPoint point = TitleBlock.Location as LocationPoint;
            origin = new ConvertPosition(point.Point, unit);
        }
        private void GetViewInSheet(Document doc, ViewSheet sheet, UnitProject unit)
        {
            List<ElementId> allViewIds = new List<ElementId>(sheet.GetAllPlacedViews());

            IsPlaneView = allViewIds.Count == 1 && doc.GetElement(allViewIds[0]) is ViewPlan;

            foreach (Viewport viewport in new FilteredElementCollector(doc, sheet.Id).OfClass(typeof(Viewport)).Cast<Viewport>())
            {
                View view = doc.GetElement(viewport.ViewId) as View;
                if (view != null)
                {
                    views.Add(new DrawingView(doc, viewport, view, unit));
                }

            }
        }

        public static List<Drawing> ExportSheets(Document document, UnitProject unit, List<ViewSheet> viewSheets, string OutputFolder, out string pathFile)
        {
            List<Drawing> drawings = new List<Drawing>();
            string outputDirectory = Path.Combine(OutputFolder, "tempImage");
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            PDFExportOptions pDFExportOptions = new PDFExportOptions();
            pDFExportOptions.OriginOffsetX = 0;
            pDFExportOptions.OriginOffsetY = 0;
            pDFExportOptions.PaperFormat = ExportPaperFormat.ISO_A2;
            pDFExportOptions.PaperPlacement = PaperPlacementType.LowerLeft;
            string name = document.Title;
            pDFExportOptions.FileName = name;
            pathFile = Path.Combine(outputDirectory, name) + ".pdf";
            if (viewSheets.Count == 0) return drawings;
            List<ElementId> list = new List<ElementId>();
            foreach (ViewSheet sheet in viewSheets)
            {
                list.Add(sheet.Id);
                Drawing drawing = new Drawing(document, sheet, unit);
                drawings.Add(drawing);
            }
            IList<ElementId> listId = list.ToList();
            try
            {
                bool a = document.Export(outputDirectory, listId, pDFExportOptions);
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error Export", e.Message);

            }

            pDFExportOptions.Dispose();
            return drawings;
        }
    }

}
