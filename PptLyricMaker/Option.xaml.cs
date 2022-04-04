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
using System.Windows.Shapes;

using Microsoft.Office.Interop.PowerPoint;

namespace PptLyricMaker
{
    /// <summary>
    /// Option.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Option : Window
    {
        private Microsoft.Office.Core.FileDialog pptFormPath;
        private Microsoft.Office.Core.FileDialog pptOutPath;
        private Module.Option optionData;

        public Option(Module.Option optionData_input)
        {
            InitializeComponent();

            pptFormPath = new Microsoft.Office.Interop.PowerPoint.Application().FileDialog[Microsoft.Office.Core.MsoFileDialogType.msoFileDialogFolderPicker];
            //pptOutPath.InitialFileName = ProgramOption.defaultPptOutPath;
            pptOutPath = new Microsoft.Office.Interop.PowerPoint.Application().FileDialog[Microsoft.Office.Core.MsoFileDialogType.msoFileDialogFolderPicker];

            OptionMainGrid.DataContext = optionData = optionData_input;

            SetDefaultPptFormPathButton.Click += SetDefaultPptFormPathButton_Cilck;
            SetDefaultPptOutPathButton.Click += SetDefaultPptOutPathButton_Cilck;
        }

        private void SetDefaultPptFormPathButton_Cilck(object sender, RoutedEventArgs e)
        {
            pptFormPath.Show();

            if (pptFormPath.SelectedItems.Count == 0)
                return;

            optionData.defaultPptFormSearchPath = pptFormPath.SelectedItems.Item(1) + "\\";
            pptFormPath.InitialFileName = pptFormPath.SelectedItems.Item(1) + "\\";
        }

        private void SetDefaultPptOutPathButton_Cilck(object sender, RoutedEventArgs e)
        {
            pptOutPath.Show();

            if (pptOutPath.SelectedItems.Count == 0)
                return;

            optionData.defaultPptOutPath = pptOutPath.SelectedItems.Item(1) + "\\";
            pptOutPath.InitialFileName = pptOutPath.SelectedItems.Item(1) + "\\";
        }
    }
}
