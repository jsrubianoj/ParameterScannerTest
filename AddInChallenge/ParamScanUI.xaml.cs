using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.UI;

namespace AddInChallenge
{

    public partial class ParamScanUI : Window
    {
        private UIApplication _uiApp;

        public ParamScanUI(UIApplication uiApp)
        {
            InitializeComponent();
            _uiApp = uiApp;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            string paramName = ParameterName.Text;
            string paramValue = ParameterValue.Text;

            SelectPlugin.SelectElements(_uiApp, paramName, paramValue);
        }

        private void IsolateButton_Click(object sender, RoutedEventArgs e)
        {
            string paramName = ParameterName.Text;
            string paramValue = ParameterValue.Text;

            SelectPlugin.IsolateElements(_uiApp, paramName, paramValue);
        }
    }

}
