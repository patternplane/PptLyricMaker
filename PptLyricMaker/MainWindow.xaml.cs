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

using System.ComponentModel;

namespace PptLyricMaker
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Module.Lyric ly;
        private bool showModifyButton_in;
        public bool showModifyButton { get { return showModifyButton_in; } set { showModifyButton_in = value; NotifyPropertyChanged("showModifyButton"); } }
        public bool needSave;

        public MainWindow()
        {
            InitializeComponent();

            // 가사의 제목/곡 내용의 수정값 적용버튼 보이기 여부
            HideModifyButton();
            LyricModifyButton.DataContext = this;
            
            // 가사 리스트를 ComboBox에 바인딩
            ly = new Module.Lyric();
            LyricComboBox.DisplayMemberPath = "title";
            LyricComboBox.ItemsSource = ly.lyricList;

            // 가사 추가 버튼의 클릭이벤트 핸들러 등록
            LyricAddButton.Click += Event_LyricAdd;

            // 제목/곡 변경시 "수정값 적용버튼" 보이기 설정
            LyricTitleModifyTextBox.TextChanged += Event_ShowModifyButton;
            LyricContentTextBox.TextChanged += Event_ShowModifyButton;
            // 단, 다른 곡을 선택한 경우엔 취소
            LyricComboBox.SelectionChanged += Event_HideModifyButton;

            // 제목/곡 변경하기 버튼
            LyricModifyButton.Click += ModifyButtonClick;

            // 저장하지 않고 다른 곡 선택을 시도할 경우 경고하기
            LyricComboBox.DropDownOpened += NotSavedWarning;
        }

        private void ModifyButtonClick(object sender, RoutedEventArgs e)
        {
            editLyric();
        }

        private void editLyric()
        {
            Module.SingleLyric lyric = (Module.SingleLyric)LyricComboBox.SelectedItem;
            lyric.title = LyricTitleModifyTextBox.Text;
            lyric.content = LyricContentTextBox.Text;
            LyricComboBox.Items.Refresh();
            int i = LyricComboBox.SelectedIndex;
            LyricComboBox.SelectedValue = "";
            LyricComboBox.SelectedIndex = i;
            HideModifyButton();
        }

        private void NotSavedWarning(object sender, EventArgs e)
        {
            if (needSave)
            {
                MessageBoxResult r = MessageBox.Show("곡의 변경사항이 저장되지 않았습니다.\n저장하겠습니까?","곡이 저장되지 않음",MessageBoxButton.YesNoCancel,MessageBoxImage.Error);
                if (r == MessageBoxResult.Yes)
                    editLyric();
                else
                    needSave = false;
            }
        }

        private void Event_ShowModifyButton(object sender, TextChangedEventArgs e)
        {
            if (LyricComboBox.SelectedIndex != -1)
                ShowModifyButton();
        }

        private void ShowModifyButton()
        {
            needSave = true;
            showModifyButton = true;
        }

        private void Event_HideModifyButton(object sender, SelectionChangedEventArgs e)
        {
            HideModifyButton();
        }

        private void HideModifyButton()
        {
            needSave = false;
            showModifyButton = false;
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
