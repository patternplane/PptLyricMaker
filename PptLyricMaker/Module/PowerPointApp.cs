using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using mspp = Microsoft.Office.Interop.PowerPoint;
using System.Windows;

namespace PptLyricMaker.Module
{
    class PowerPointApp
    {
        private mspp.Application App;
        public PowerPointApp()
        {
            App = new mspp.Application();
        }

        public void SavePptFile(String FormFile, String OutputPath , String content, int lineNumber)
        {
            try
            {
                mspp.Presentation form = App.Presentations.Open(FormFile, WithWindow: Microsoft.Office.Core.MsoTriState.msoFalse);
                mspp.Presentation ppt = App.Presentations.Add(Microsoft.Office.Core.MsoTriState.msoFalse);

                ppt.PageSetup.SlideOrientation = form.PageSetup.SlideOrientation;
                ppt.PageSetup.SlideSize = form.PageSetup.SlideSize;
                ppt.PageSetup.SlideHeight = form.PageSetup.SlideHeight;
                ppt.PageSetup.SlideWidth = form.PageSetup.SlideWidth;

                makeSlides(form, ppt, content, lineNumber);

                form.Close();
                ppt.SaveAs(OutputPath);
                ppt.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("1. 버튼을 너무 여러번 눌렀거나\n2. 출력하려는 위치가 관리자 권한이 필요한 위치일 수 있습니다.\n(다른 위치에 저장하거나, 관리자 권한으로 실행해주세요.)","파일 저장 오류",MessageBoxButton.OK,MessageBoxImage.Error);
                MessageBox.Show("에러 정보 : \n" + e.Message, "error");
            }
        }

        public void GeneratePpt(String FormFile, String content, int lineNumber)
        {
            mspp.Presentation form = App.Presentations.Open(FormFile, WithWindow: Microsoft.Office.Core.MsoTriState.msoFalse);
            mspp.Presentation ppt = App.Presentations.Add();
            ppt.PageSetup.SlideOrientation = form.PageSetup.SlideOrientation;
            ppt.PageSetup.SlideSize = form.PageSetup.SlideSize;
            ppt.PageSetup.SlideHeight = form.PageSetup.SlideHeight;
            ppt.PageSetup.SlideWidth = form.PageSetup.SlideWidth;

            makeSlides(form, ppt, content, lineNumber);
            form.Close();
        }

        private void makeSlides(mspp.Presentation form, mspp.Presentation ppt, String content, int lineNumber)
        {
            String[] lines = content.Split(new[] { (char)0xd, (char)0xa },StringSplitOptions.RemoveEmptyEntries);
            int lastSlide = 1;
            mspp.Slide CurrentSlide;
            StringBuilder lyric = new StringBuilder("");
            int currentLine = 0;
            foreach (String line in lines)
            {
                lyric.Append(line);
                currentLine++;
                // 라인 수가 모였다면
                if (currentLine == lineNumber)
                {
                    // 슬라이드 하나 채우기
                    form.Slides[1].Copy();
                    ppt.Slides.Paste();
                    CurrentSlide = ppt.Slides[lastSlide++];

                    // 모든 text가능 shape마다 작업
                    foreach (mspp.Shape s in CurrentSlide.Shapes)
                        if (s.HasTextFrame != Microsoft.Office.Core.MsoTriState.msoFalse)
                            s.TextFrame.TextRange.Text = s.TextFrame.TextRange.Text.Replace("/가사", lyric.ToString());

                    lyric.Clear();

                    currentLine = 0;
                }
                else
                    lyric.Append("\n");
            }
            if (lyric.Length != 0)
            {
                // 슬라이드 하나 채우기
                form.Slides[1].Copy();
                ppt.Slides.Paste();
                CurrentSlide = ppt.Slides[lastSlide++];

                // 모든 text가능 shape마다 작업
                foreach (mspp.Shape s in CurrentSlide.Shapes)
                    if (s.HasTextFrame != Microsoft.Office.Core.MsoTriState.msoFalse)
                        s.TextFrame.TextRange.Text = s.TextFrame.TextRange.Text.Replace("/가사", lyric.ToString());
            }
        }
    }
}
