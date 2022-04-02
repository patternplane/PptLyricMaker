using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.PowerPoint;

namespace PptLyricMaker.Module
{
    class PowerPointApp
    {
        private Application App;
        public PowerPointApp()
        {
            App = new Application();
        }

        public void SavePptFile(String FormFile, String OutputPath , String content, int lineNumber)
        {
            Presentation form = App.Presentations.Open(FormFile, WithWindow: Microsoft.Office.Core.MsoTriState.msoFalse);
            Presentation ppt = App.Presentations.Add(Microsoft.Office.Core.MsoTriState.msoFalse);

            makeSlides(form, ppt, content, lineNumber);

            form.Close();
            ppt.SaveAs(OutputPath);
            ppt.Close();
        }

        public void GeneratePpt(String FormFile, String content, int lineNumber)
        {
            Presentation form = App.Presentations.Open(FormFile, WithWindow: Microsoft.Office.Core.MsoTriState.msoFalse);
            Presentation ppt = App.Presentations.Add();

            makeSlides(form, ppt, content, lineNumber);
            form.Close();
        }

        private void makeSlides(Presentation form,Presentation ppt, String content, int lineNumber)
        {
            String[] lines = content.Split(new[] { (char)0xd, (char)0xa },StringSplitOptions.RemoveEmptyEntries);
            int lastSlide = 1;
            Slide CurrentSlide;
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
                    foreach (Shape s in CurrentSlide.Shapes)
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
                foreach (Shape s in CurrentSlide.Shapes)
                    if (s.HasTextFrame != Microsoft.Office.Core.MsoTriState.msoFalse)
                        s.TextFrame.TextRange.Text = s.TextFrame.TextRange.Text.Replace("/가사", lyric.ToString());
            }
        }
    }
}
