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
        private Module.Lyric ly;
        private ModifyButtonVisibility ModifyButtonVisible;

        public MainWindow()
        {
            InitializeComponent();

            // 가사의 제목/곡 내용의 수정값 적용버튼 보이기 여부
            ModifyButtonVisible = new ModifyButtonVisibility() { showButton = false };
            ModifyButtonGrid.DataContext = ModifyButtonVisible;

            // 가사 리스트를 ComboBox에 바인딩
            ly = new Module.Lyric();
            LyricComboBox.DisplayMemberPath = "title";
            LyricComboBox.ItemsSource = ly.lyricList;

            // 가사 추가 버튼의 클릭이벤트 핸들러 등록
            LyricAddButton.Click += Event_LyricAdd;
        }

        /// <summary>
        /// 곡 추가 이벤트핸들러
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Event_LyricAdd(object sender, RoutedEventArgs e) {
            foreach (char c in LyricAddTitle.Text) {
                if (!char.IsWhiteSpace(c))
                {
                    // 유효한 제목이 입력된 경우
                    try
                    {
                        ly.addLyric(LyricAddTitle.Text, LyricAddContent.Text);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message, "곡 추가 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    return;
                }
            }
            // 유효하지 않은 제목이 입력된 경우
            MessageBox.Show("제목을 입력해주세요!","제목 오류",MessageBoxButton.OK,MessageBoxImage.Error);
        }
    }
}
