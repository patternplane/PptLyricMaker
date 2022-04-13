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
        public Module.Option ProgramOption;
        private Module.PowerPointApp powerpoint;

        private bool showModifyButton_in;
        public bool showModifyButton { get { return showModifyButton_in; } set { showModifyButton_in = value; NotifyPropertyChanged("showModifyButton"); } }
        public bool needSave;
        private bool showDeleteButton_in;
        public bool showDeleteButton { get { return showDeleteButton_in; } set { showDeleteButton_in = value; NotifyPropertyChanged("showDeleteButton"); } }
        private  String pptFormPath_in;
        public String pptFormPath { get { return pptFormPath_in; } set { pptFormPath_in = value; NotifyPropertyChanged("pptFormPath"); } }

        private String linePerSlide_in;
        public String linePerSlide { get { return linePerSlide_in; } set { linePerSlide_in = value; NotifyPropertyChanged("linePerSlide"); } }

        private String OutputFileName_in;
        public String OutputFileName { get { return OutputFileName_in; } set { OutputFileName_in = value; NotifyPropertyChanged("OutputPath"); } }

        private String OutputPptPath_in;
        public String OutputPptPath { get { return OutputPptPath_in; } set { OutputPptPath_in = value; NotifyPropertyChanged("OutputPptPath"); } }

        // ppt 출력 경로찾기 윈도우
        private Microsoft.Office.Core.FileDialog pptOutPath;
        // ppt 틀 파일 찾기 윈도우
        System.Windows.Forms.OpenFileDialog pptFile;

        string lastSearchPattern = null;
        const string DEFAUL_SEARCH_TEXT = "(가사 또는 제목으로 검색)";

        public MainWindow()
        {
            InitializeComponent();

            // 필요 객체 세팅
            ly = new Module.Lyric();
            ProgramOption = new Module.Option();
            powerpoint = new Module.PowerPointApp();


            // 변수 초기화
            pptFormPath = "";
            linePerSlide = "";
            OutputFileName = "";
            OutputPptPath = ProgramOption.defaultPptOutPath;
            pptOutPath = new Microsoft.Office.Interop.PowerPoint.Application().FileDialog[Microsoft.Office.Core.MsoFileDialogType.msoFileDialogFolderPicker];
            pptOutPath.InitialFileName = ProgramOption.defaultPptOutPath;

            pptFile = new System.Windows.Forms.OpenFileDialog();
            pptFile.InitialDirectory = ProgramOption.defaultPptFormSearchPath;
            pptFile.Multiselect = false;
            pptFile.Filter = "PowerPoint파일(*.ppt,*.pptx,*.pptm)|*.ppt;*.pptx;*.pptm";

            // 데이터 바인딩 :

            // 가사의 제목/곡 내용의 수정값 적용버튼 보이기 여부
            HideModifyButton();
            LyricModifyButton.DataContext = this;

            // 곡 삭제버튼 보이기 여부
            HideDeleteButton();
            LyricDeleteButton.DataContext = this;

            // 가사 리스트를 ComboBox에 바인딩
            LyricComboBox.DisplayMemberPath = "title";
            LyricComboBox.ItemsSource = ly.lyricList;

            // 가사 추가 버튼의 클릭이벤트 핸들러 등록
            LyricAddButton.Click += Event_LyricAdd;

            // 제목/곡 변경시 "수정값 적용버튼" 보이기 설정
            LyricTitleModifyTextBox.TextChanged += Event_LyricTitleTextBox_TextChanged;
            LyricContentTextBox.TextChanged += Event_LyricContentTextBox_TextChanged;
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

            // ppt 파일 출력 경로 텍스트 및 버튼 바인딩
            pptFileOutPath.DataContext = this;
            pptFileOutPathButton.Click += pptFileOutButtonClick;

            // 옵션 버튼
            OptionButton.Click += OptionButtonClick;

            // 검색창 기본글귀
            SearchComboBox.Text = DEFAUL_SEARCH_TEXT;
            // 검색 시작 이벤트
            SearchButton.Click += searchStartEvent;
            // 검색값 선택 이벤트
            SearchComboBox.SelectionChanged += searchSelectEvent;
            // 검색 콤보박스 데이터 연결
            SearchComboBox.DisplayMemberPath = "display";



            // 마지막 처리함수 등록
            this.Closed += finalProcess;
        }

        private void searchSelectEvent(object sender, SelectionChangedEventArgs e)
        {
            if (SearchComboBox.SelectedIndex != -1)
                LyricComboBox.SelectedIndex = ((Module.searchPair)SearchComboBox.SelectedItem).index;
        }

        private void searchStartEvent(object sender, RoutedEventArgs e)
        {
            searchStart(false);
        }

        private void searchStart(bool useLastRequest)
        {
            if (useLastRequest)
            {
                if (lastSearchPattern != null)
                {
                    SearchComboBox.ItemsSource = ly.search(lastSearchPattern);
                }
            }
            else
            {
                if (SearchComboBox.Text.Length > 0 && SearchComboBox.Text.CompareTo(DEFAUL_SEARCH_TEXT) != 0)
                {
                    SearchComboBox.ItemsSource = ly.search(SearchComboBox.Text);
                    lastSearchPattern = SearchComboBox.Text;
                }
                else
                {
                    MessageBox.Show("검색할 내용을 입력하세요!","검색값",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            }
        }

        private void OptionButtonClick(object sender, RoutedEventArgs e)
        {
            new Option(ProgramOption).ShowDialog();
        }

        private void pptFileOutButtonClick(object sender, RoutedEventArgs e)
        {
            pptOutPath.Show();
            if (pptOutPath.SelectedItems.Count == 0)
                return;

            OutputPptPath = pptOutPath.SelectedItems.Item(1) + "\\";
            pptOutPath.InitialFileName = pptOutPath.SelectedItems.Item(1) + "\\";
        }

        private void finalProcess(object sender, EventArgs e)
        {
            ly.SaveAll();
            ProgramOption.saveOptionData();
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
            if (OutputFileName.Length == 0)
            {
                MessageBox.Show("출력할 파일 이름이 입력되지 않았습니다.", "파일명", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (OutputPptPath.Length == 0)
            {
                MessageBox.Show("출력할 파일의 저장위치가 입력되지 않았습니다.", "파일 저장위치", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            powerpoint.SavePptFile(pptFormPath, OutputPptPath + OutputFileName, ((Module.SingleLyric)LyricComboBox.SelectedItem).content, Convert.ToInt32(linePerSlide));
        }

        private void pptFormPathButtonClick(object sender, RoutedEventArgs e)
        {
            if (pptFile.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            pptFile.InitialDirectory = System.IO.Path.GetDirectoryName(pptFile.FileName) + "\\";
            pptFormPath = pptFile.FileName;
        }

        private void LyricDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = MessageBox.Show("곡 \"" + ((Module.SingleLyric)LyricComboBox.SelectedItem).title + "\" 을 삭제합니다.","곡 삭제",MessageBoxButton.OKCancel,MessageBoxImage.Warning);
            if (r == MessageBoxResult.OK)
            {
                ly.deleteLyric(LyricComboBox.SelectedIndex);
                HideDeleteButton();

                searchStart(true);
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
            lyric.content = Module.StringCorrector.makeCorrectNewline(LyricContentTextBox.Text);
            LyricComboBox.Items.Refresh();
            int i = LyricComboBox.SelectedIndex;
            LyricComboBox.SelectedValue = "";
            LyricComboBox.SelectedIndex = i;
            HideModifyButton();

            searchStart(true);
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

        private void Event_LyricTitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LyricComboBox.SelectedIndex != -1)
                ShowModifyButton();
        }

        private void Event_LyricContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
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
                        ly.addLyric(LyricAddTitle.Text, Module.StringCorrector.makeCorrectNewline(LyricAddContent.Text));
                        searchStart(true);
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
