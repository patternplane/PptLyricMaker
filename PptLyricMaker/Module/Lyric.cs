using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Windows;
using System.ComponentModel;

namespace PptLyricMaker.Module
{
    /// <summary>
    /// 가사 관련 작업을 담당합니다.
    /// </summary>
    public class Lyric
    {
        private BindingList<SingleLyric> lyricArray;
        public BindingList<SingleLyric> lyricList { get { return lyricArray; } }
        private BindingList<searchPair> searchResult = null;

        public Lyric()
        {
            lyricArray = LyricFile.getLyricInfo();

            if (lyricArray == null)
                MessageBox.Show("가사 불러오기 실패!\n프로그램을 종료 후 다시 시작해주세요.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void addLyric(String title, String content)
        {
            foreach (SingleLyric t in lyricArray)
                if (t.title.CompareTo(title) == 0)
                {
                    // 동일한 제목의 곡 존재
                    throw new Exception("동일한 제목의 곡이 있습니다!");
                }
            // 동일한 제목의 곡이 없다면 추가
            lyricArray.Add(new SingleLyric() {title=title, content=content});
        }

        public void deleteLyric(int index)
        {
            if ((index >= 0) && (index < lyricArray.Count))
                lyricArray.Remove(lyricArray[index]);
        }

        /// <summary>
        /// 전체 곡에서 주어진 패턴으로 제목 또는 가사를 검색합니다.
        /// </summary>
        /// <param name="pattern">
        /// 찾을 패턴
        /// </param>
        /// <returns>
        /// 찾은 패턴값과 곡의 인덱스 정보가 담긴 searchPair 리스트
        /// </returns>
        public BindingList<searchPair> search(String pattern)
        {
            if (searchResult == null)
                searchResult = new BindingList<searchPair>();
            else
                searchResult.Clear();

            int[] findPos;
            String content;
            for (int i = 0; i < lyricArray.Count; i++) 
            {
                if (StringKMP.FindPattern(lyricArray[i].title.Replace(" ","").Replace("\r\n",""), pattern.Replace(" ", "").Replace("\r\n", ""), delegate (char a, char b) { return a == b; }).Length > 0 )
                    searchResult.Add(new searchPair() {display = lyricArray[i].title, index = i});

                content = lyricArray[i].content.Replace("\r\n", "");
                findPos = StringKMP.FindPattern(content.Replace(" ", "").Replace("\r\n", ""), pattern.Replace(" ", "").Replace("\r\n", ""), delegate (char a, char b) { return a == b; });
                if (findPos.Length > 0)
                {
                    findPos = getRealIndex(content,findPos);
                    foreach (String s in makeResultPreview(findPos, content, pattern.Length, lyricArray[i].title))
                        searchResult.Add(new searchPair() { display = s, index = i });
                }
            }

            return searchResult;
        }

        /// <summary>
        /// 한 문자열에서 찾은 패턴들을 주변 문맥을 덧붙여 가공하여 문자열로 출력합니다.<br/>
        /// 여러 패턴이 가까이 붙어있다면 한 문자열로 이어서 출력합니다.
        /// </summary>
        /// <param name="startPos">
        /// KMP 검색결과, 문자열 상에서 발견된 패턴들의 시작위치입니다.
        /// </param>
        /// <param name="content">
        /// 원본 문자열입니다.
        /// </param>
        /// <param name="patternLength">
        /// 검색한 패턴의 길이입니다.
        /// </param>
        /// <param name="title">
        /// 곡의 제목입니다.
        /// </param>
        /// <returns>
        /// 가공된 패턴들을 담은 String 배열
        /// </returns>
        private String[] makeResultPreview(int[] startPos, String content, int patternLength, String title)
        {
            int INTERVAL = 6;

            List<String> result = new List<string>(10);
            int startIndex = ((startPos[0] - INTERVAL > 0)? startPos[0] - INTERVAL : 0);
            int endPos;
            StringBuilder displayString = new StringBuilder(200);
            for (int i = 0; i < startPos.Length; i++)
            {
                endPos = startPos[i] + patternLength;
                if (i + 1 == startPos.Length)
                {
                    endPos += INTERVAL;
                    if (endPos > content.Length)
                        endPos = content.Length;
                    displayString.Clear();
                    displayString.Append("(");
                    displayString.Append(title);
                    displayString.Append(") ");
                    displayString.Append("...");
                    displayString.Append(content.Substring(startIndex, endPos - startIndex));
                    displayString.Append("...");
                    result.Add(displayString.ToString());
                }
                else if (endPos + INTERVAL < startPos[i + 1] - INTERVAL)
                {
                    endPos += INTERVAL;
                    displayString.Clear();
                    displayString.Append("(");
                    displayString.Append(title);
                    displayString.Append(") ");
                    displayString.Append("...");
                    displayString.Append(content.Substring(startIndex, endPos - startIndex));
                    displayString.Append("...");
                    result.Add(displayString.ToString());

                    startIndex = startPos[i + 1] - INTERVAL;
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 공백을 제외하고 검색된 KMP 검색 결과의 인덱스를 실제 문자열에서의 검색위치로 변경해줍니다.
        /// </summary>
        /// <param name="originalStr">원본 문자열</param>
        /// <param name="findPos">공백을 제외하여 검색한 KMP 검색결과</param>
        /// <returns>실제 원본 문자열에서 검색된 위치들</returns>
        private int[] getRealIndex(String originalStr, int[] findPos)
        {
            List<int> result = new List<int>(20);
            int posIndex = 0;
            int realIndex = 0;
            int nonBlankIndex = 0;
            while (posIndex < findPos.Length)
            {
                while (nonBlankIndex != findPos[posIndex]) 
                {
                    while (char.IsWhiteSpace(originalStr[++realIndex]));
                    nonBlankIndex++;
                }
                result.Add(realIndex);
                posIndex++;
            }
            return result.ToArray();
        }

        public void SaveAll()
        {
            try
            {
                LyricFile.SaveAll(lyricArray);
            }
            catch (Exception e)
            {
                MessageBox.Show("가사 저장 실패!\n오류 : " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ■ 테스트용 함수
        /// lyric 생성 결과를 출력해주는 함수
        /// </summary>
        /// <returns>
        /// lyric 가사를 String[] 형식으로 반환합니다.<br/>
        /// 결과 문자열은 [제목 + 줄바꿈 + 본문] 꼴로 출력됩니다.
        /// </returns>
        public String[] testOutput()
        {
            int cnt = lyricArray.Count;
            String[] lyrics = new String[cnt];
            for (int i = 0; i < cnt;i++)
                lyrics[i] = (lyricArray[i].title.Length == 0 ? "[null]" : lyricArray[i].title) + (char)0xd + (char)0xa + (lyricArray[i].content.Length == 0 ? "[null]" : lyricArray[i].content);

            return lyrics;
        }
    }

    /// <summary>
    /// 검색 결과의 한 단위를 나타냅니다.
    /// </summary>
    public class searchPair { 
        public String display { get; set; } 
        public int index { get; set; }
    }

    /// <summary>
    /// 곡의 한 단위를 나타냅니다.
    /// </summary>
    public class SingleLyric
    {
        public String title { get; set; }
        public String content { get; set; }
    }

    /// <summary>
    /// Lyrics 저장 파일의 입출력에 관여합니다.
    /// </summary>
    class LyricFile
    {
        const char SEPARATOR = '∂';
        const char CR = (char)0xd;

        /// <summary>
        /// 파일로부터 전체 가사를 읽어 리스트로 변환합니다.
        /// </summary>
        /// <returns>
        /// 가사 리스트.<br/>
        /// 읽기에 실패하면 null을 반환합니다.
        /// </returns>
        public static BindingList<SingleLyric> getLyricInfo()
        {
            List<SingleLyric> lyricArray;

            try
            {
                // 파일 존재 확인
                FileInfo fi = new FileInfo(Module.FilePath.LYRIC_PATH);
                if (!fi.Exists)
                {
                    DirectoryInfo di = new DirectoryInfo(Module.FilePath.DATA_PATH);
                    if (!di.Exists)
                    {
                        di.Create();
                    }

                    lyricArray = new List<SingleLyric>();
                }
                else
                {
                    // 파일이 존재한다면 불러오기
                    lyricArray = new List<SingleLyric>();

                    // 파일의 내용을 버퍼 단위로 읽어옴
                    StreamReader file = new StreamReader(Module.FilePath.LYRIC_PATH);
                    int BUF_LEN = 4096;
                    char[] buffer = new char[BUF_LEN];
                    int readLen;

                    StringBuilder content = new StringBuilder("");
                    int startPos; // 버퍼에서 읽어올 첫 인덱스
                    int endPos; // 버퍼에서 읽어올 마지막 인덱스 + 1

                    int titleEndPosition;

                    while ((readLen = file.Read(buffer, 0, BUF_LEN)) > 0)
                    {
                        // 버퍼를 읽어가면서 SEPARATOR 문자로 구분하여 곡을 저장한다.
                        startPos = 0;
                        for (endPos = startPos; endPos < readLen; endPos++)
                            // SEPARATOR 발견
                            if (buffer[endPos] == SEPARATOR)
                            {
                                // SEPARATOR 발견 전까지를 전부 가져오기
                                content.Append(buffer, startPos, endPos - startPos);

                                // 한 곡으로 생성
                                for (titleEndPosition = 0; (titleEndPosition < content.Length) && (content[titleEndPosition] != CR); titleEndPosition++);
                                if (titleEndPosition == content.Length)
                                    lyricArray.Add(
                                        new SingleLyric()
                                        {
                                            title = content.ToString(0, titleEndPosition),
                                            content = ""
                                        });
                                else
                                    lyricArray.Add(
                                        new SingleLyric()
                                        {
                                            title = content.ToString(0, titleEndPosition),
                                            content = content.ToString(titleEndPosition + 2, content.Length - (titleEndPosition + 2))
                                        });

                                content = new StringBuilder("");
                                // SEPARATOR 뒤 공백문자는 모두 건너뜀
                                for (startPos = endPos + 1; (startPos < readLen) && char.IsWhiteSpace(buffer[startPos]); startPos++);
                                endPos = startPos - 1;
                            }
                        if (startPos < readLen)
                            content.Append(buffer, startPos, endPos - startPos);
                    }

                    // 정렬
                    lyricArray.Sort(delegate (SingleLyric a, SingleLyric b) { return a.title.CompareTo(b.title); });

                    file.Close();
                }
            }
            catch
            {
                lyricArray = null;
            }

            return new BindingList<SingleLyric>(lyricArray);
        }

        /// <summary>
        /// 전체 가사를 파일로 저장합니다.
        /// </summary>
        /// <param name="lyricArray">
        /// 전체 가사 리스트
        /// </param>
        public static void SaveAll(BindingList<SingleLyric> lyricArray)
        {
            StreamWriter file = new StreamWriter(Module.FilePath.LYRIC_PATH,false);

            try
            {
                foreach (SingleLyric lyric in lyricArray)
                {
                    file.WriteLine(lyric.title);
                    file.Write(lyric.content);
                    file.WriteLine(SEPARATOR);
                }
            }
            catch (Exception e)
            {
                file.Close();
                throw e;
            }
            file.Close();
        }
    }
}

