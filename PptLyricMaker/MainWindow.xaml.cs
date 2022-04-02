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

namespace PptLyricMaker
{
    class ModifyButtonVisibility
    {
        public bool showButton { get; set; }
    }
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();

            ModifyButtonVisibility ModifyButtonVisible = new ModifyButtonVisibility() {showButton = false};
            ModifyButtonGrid.DataContext = ModifyButtonVisible;

            Module.Lyric ly = new Module.Lyric();
            LyricComboBox.DisplayMemberPath = "title";
            LyricComboBox.ItemsSource = ly.lyricList;
        }
    }
}
