using System;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Windows.Media.Imaging;
using System.IO;
using Autodesk.Revit.Creation;
using System.Windows.Interop;
using System.Collections.Generic;

namespace AddInChallenge
{
    public class ParameterScanner : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string ribbonName = "Parameters";
            application.CreateRibbonTab(ribbonName);
            // Create a ribbon panel
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(ribbonName, "Parameters");

            // Get the path to the current assembly
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            // Create a button
            PushButtonData buttonData = new PushButtonData("selectCommand","Parameter Scanner",thisAssemblyPath,"AddInChallenge.SelectPlugin");

            // Add the button to the ribbon panel
            PushButton pushButton = ribbonPanel.AddItem(buttonData) as PushButton;
            pushButton.ToolTip = "Add-in for select and filter elements";

            // Load the button icon
            Uri uriImage = new Uri(@"C:\RevitAddIns\filter_icon.png");
            BitmapImage bitmapImage = new BitmapImage(uriImage);
            pushButton.LargeImage = bitmapImage;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class SelectPlugin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Autodesk.Revit.DB.Document doc = uidoc.Document;
                View activeView = doc.ActiveView;

                // Check if the view is a Floor Plan, Reflected Ceiling Plan, or 3D View
                if (activeView.ViewType != ViewType.FloorPlan &&
                activeView.ViewType != ViewType.CeilingPlan &&
                activeView.ViewType != ViewType.ThreeD)
                {
                    TaskDialog.Show("Warning", "This add-in only works in Floor Plans, Reflected Ceiling Plans, and 3D Views.");
                    return Result.Failed;
                }

                //Instantiate the WPF Window
                ParamScanUI window = new ParamScanUI(uiApp);

                // Get the Revit Main Window Handle
                IntPtr revitHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

                //Attach the WPF Window to Revit's Main Window (so it stays on top)
                WindowInteropHelper helper = new WindowInteropHelper(window);
                helper.Owner = revitHandle;

                //Show the WPF UI as a dialog
                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
                        
        }
    

        public static void IsolateElements(UIApplication uiApp, string paramName, string paramValue)
        {
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            using (Transaction tx = new Transaction(doc, "Isolate Elements"))
            {
                tx.Start();

                FilteredElementCollector collector = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType();

                List<ElementId> matchingElements = new List<ElementId>();

                foreach (Element elem in collector)
                {
                    Parameter param = elem.LookupParameter(paramName);
                    if (param != null)
                    {
                        string value = param.AsString();
                        if (!string.IsNullOrEmpty(value) && value == paramValue)
                        {
                            matchingElements.Add(elem.Id);
                        }
                    }
                }

                if (matchingElements.Count > 0)
                {
                    uidoc.ActiveView.IsolateElementsTemporary(matchingElements);
                    TaskDialog.Show("Isolation Result", $"{matchingElements.Count} elements found and isolated.");
                }
                else
                {
                    TaskDialog.Show("No Elements Found", "No elements found with the specified parameter value");
                }
                    tx.Commit();
            }
        }

        public static void SelectElements(UIApplication uiApp, string paramName, string paramValue)
        {
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType();

            List<ElementId> matchingElements = new List<ElementId>();

            foreach (Element elem in collector)
            {
                Parameter param = elem.LookupParameter(paramName);
                if (param != null)
                {
                    string value = param.AsString();
                    if (!string.IsNullOrEmpty(value) && value == paramValue)
                    {
                        matchingElements.Add(elem.Id);
                    }
                }
            }

            if (matchingElements.Count > 0)
            {
                uidoc.Selection.SetElementIds(matchingElements);
                TaskDialog.Show("Selection Result", $"{matchingElements.Count} elements found and isolated.");
            }
            else
            {
                TaskDialog.Show("No Elements Found", "No elements found with the specified parameter value");
            }
        }
    }
}






