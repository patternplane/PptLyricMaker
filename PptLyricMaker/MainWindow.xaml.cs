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
        private bool showDeleteButton_in;
        public bool showDeleteButton { get { return showDeleteButton_in; } set { showDeleteButton_in = value; NotifyPropertyChanged("showDeleteButton"); } }
        private  String pptFormPath_in;
        public String pptFormPath { get { return pptFormPath_in; } set { pptFormPath_in = value; NotifyPropertyChanged("pptFormPath"); } }

        private String linePerSlide_in;
        public String linePerSlide { get { return linePerSlide_in; } set { linePerSlide_in = value; NotifyPropertyChanged("linePerSlide"); } }

        private String OutputPath_in;
        public String OutputPath { get { return OutputPath_in; } set { OutputPath_in = value; NotifyPropertyChanged("OutputPath"); } }

        public String OutputPptPath;

        private Module.PowerPointApp powerpoint;

        public MainWindow()
        {
            InitializeComponent();

            // 변수 초기화
            pptFormPath = "";
            linePerSlide = "";
            OutputPath = "";


            // 데이터 바인딩 :

            // 가사의 제목/곡 내용의 수정값 적용버튼 보이기 여부
            HideModifyButton();
            LyricModifyButton.DataContext = this;

            // 곡 삭제버튼 보이기 여부
            HideDeleteButton();
            LyricDeleteButton.DataContext = this;

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
            LyricComboBox.SelectionChanged += Event_SelectChanged;

            // 제목/곡 변경하기 버튼
            LyricModifyButton.Click += ModifyButtonClick;

            // 저장하지 않고 다른 곡 선택을 시도할 경우 경고하기
            LyricComboBox.DropDownOpened += NotSavedWarning;

            // 곡 삭제하기 버튼
            LyricDeleteButton.Click += LyricDeleteButtonClick;

            // ppt 틀 경로 텍스트 및 버튼 바인딩
            pptFormPathTextBox.DataContext = this;
            pptFormPathButton.Click += pptFormPathButtonClick;

            // 슬라이드별 줄 수 텍스트 바인딩
            LinePerSlideTextBox.DataContext = this;
            // 숫자만 입력받기
            LinePerSlideTextBox.KeyDown += LinePerSlideTextBox_KeyDown;

            // ppt 출력파일의 이름 텍스트 바인딩
            pptOutputFileNameTextBox.DataContext = this;

            // ppt 파일로 출력하기 버튼
            pptFileOutButton.Click += pptOutputButtonClick;

            // ppt 생성하기 버튼
            pptGenerateButton.Click += pptGenerateButtonClick;




            // ppt 연결
            powerpoint = new Module.PowerPointApp();


            // 마지막 처리
            this.Closed += finalProcess;
        }

        private void finalProcess(object sender, EventArgs e)
        {
            ly.SaveAll();
        }

        private void LinePerSlideTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Back || e.Key == Key.Delete)
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void pptGenerateButtonClick(object sender, RoutedEventArgs e)
        {
            linePerSlide = linePerSlide.Replace(" ", "");
            if (LyricComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("출력할 곡을 선택하지 않았습니다.", "곡이 선택되지 않음", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (pptFormPath.Length == 0)
            {
                MessageBox.Show("ppt 틀 파일이 입력되지 않았습니다.", "ppt 프레임", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (linePerSlide.Length == 0)
            {
                MessageBox.Show("슬라이드별 줄 수가 입력되지 않았습니다.", "슬라이드별 줄 수", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            powerpoint.GeneratePpt(pptFormPath, ((Module.SingleLyric)LyricComboBox.SelectedItem).content, Convert.ToInt32(linePerSlide));
        }

        private void pptOutputButtonClick(object sender, RoutedEventArgs e)
        {
            linePerSlide = linePerSlide.Replace(" ", "");
            if (LyricComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("출력할 곡을 선택하지 않았습니다.", "곡이 선택되지 않음", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (pptFormPath.Length == 0)
            {
                MessageBox.Show("ppt 틀 파일이 입력되지 않았습니다.", "ppt 프레임", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (linePerSlide.Length == 0)
            {
                MessageBox.Show("슬라이드별 줄 수가 입력되지 않았습니다.", "슬라이드별 줄 수",MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            if (OutputPath.Length == 0)
            {
                MessageBox.Show("출력할 파일 이름이 입력되지 않았습니다.", "파일명", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            System.Windows.Forms.FolderBrowserDialog pptFile = new System.Windows.Forms.FolderBrowserDialog();

            if (pptFile.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            OutputPptPath = pptFile.SelectedPath;

            powerpoint.SavePptFile(pptFormPath, OutputPptPath + "\\" + OutputPath, ((Module.SingleLyric)LyricComboBox.SelectedItem).content, Convert.ToInt32(linePerSlide));
        }

        private void pptFormPathButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog pptFile = new System.Windows.Forms.OpenFileDialog();
            pptFile.Multiselect = false;
            pptFile.Filter = "PowerPoint파일(*.ppt,*.pptx,*.pptm)|*.ppt;*.pptx;*.pptm";

            if (pptFile.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            pptFormPath = pptFile.FileName;
        }

        private void LyricDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = MessageBox.Show("곡 \"" + ((Module.SingleLyric)LyricComboBox.SelectedItem).title + "\" 을 삭제합니다.","곡 삭제",MessageBoxButton.OKCancel,MessageBoxImage.Warning);
            if (r == MessageBoxResult.OK)
            {
                ly.deleteLyric(LyricComboBox.SelectedIndex);
                HideDeleteButton();
            }
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

        private void ShowDeleteButton()
        {
            showDeleteButton = true;
        }

        private void Event_SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            HideModifyButton();
            ShowDeleteButton();
        }

        private void HideModifyButton()
        {
            needSave = false;
            showModifyButton = false;
        }

        private void HideDeleteButton()
        {
            showDeleteButton = false;
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
