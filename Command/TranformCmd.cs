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

namespace Revit_Transform.Command
{
    //Check availability
    public class AvailabilityNoOpenDocument : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            if (a.ActiveUIDocument == null)
            {
                return true;
            }
            return false;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class TransformCmd : IExternalCommand
    {

        private TransFormWindow transFormWindow;
        private bool transactionCompleted = false;
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            //UIApplication uiapp = commandData.Application;
            //UIDocument uidoc = uiapp.ActiveUIDocument;
            //Application app = uiapp.Application;
            //Document doc = uidoc.Document;


            //TransFormViewModel viewModel = new TransFormViewModel(uidoc, doc);
            //transFormWindow
            //     = new TransFormWindow(viewModel);
            //transFormWindow.Closed += TransFormWindow_Closed;
            //transFormWindow.Show();

            //// Chờ đến khi cửa sổ WPF đóng
            //while (transFormWindow.IsVisible)
            //{
            //    // Thực hiện xử lý các hành động trong giao dịch ở đây (ví dụ: kiểm tra nút bấm, trạng thái hành động, vv.)

            //    // Nếu giao dịch đã hoàn thành, thoát khỏi vòng lặp
            //    if (transactionCompleted)
            //        break;

            //    // Tiếp tục chờ
            //    System.Windows.Forms.Application.DoEvents();
            //}




            //return Result.Succeeded;



            DockablePaneId dpid = new DockablePaneId(new Guid("{D7C963CE-B3CA-426A-8D51-6E8254D21158}"));
            DockablePane dp = commandData.Application.GetDockablePane(dpid);
            if (dp.IsShown())
            {
                dp.Hide();
            }
            else
            {
                dp.Show();
            }
            return Result.Succeeded;


        }
        private void TransFormWindow_Closed(object sender, EventArgs e)
        {
            // Xóa đăng ký sự kiện Closed
            transFormWindow.Closed -= TransFormWindow_Closed;

            // Đánh dấu giao dịch đã hoàn thành
            transactionCompleted = true;

            // Đóng cửa sổ WPF
        }
    }
}
