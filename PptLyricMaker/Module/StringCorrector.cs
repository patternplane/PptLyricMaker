using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PptLyricMaker.Module
{
    class StringCorrector
    {
        /// <summary>
        /// 잘못된 개행문자를 윈도우 표준에 맞게 바꿔줍니다.
        /// </summary>
        /// <param name="original">
        /// 원본 문자열
        /// </param>
        /// <returns>
        /// 고쳐진 문자열
        /// </returns>
        static public string makeCorrectNewline(string original)
        {
            StringBuilder str = new StringBuilder(original);
            for (int i = 0, j = 0; i < original.Length; i++, j++)
            {
                if (original[i] == '\r')
                {
                    if ((i + 1 == original.Length) || (original[i + 1] != '\n'))
                    {
                        str = str.Insert(j + 1, '\n');
                        j++;
                    }
                    else
                    {
                        i++;
                        j++;
                    }
                }
                else if (original[i] == '\n')
                {
                    str = str.Insert(j, '\r');
                    j++;
                }
                else if (original[i] == '\v')
                {
                    str = str.Replace("\v", "\r\n", j, 1);
                    j++;
                }
            }
            return str.ToString();
        }

        /// <summary>
        /// 주어진 문자열에서 숫자만 남도록 재단합니다.
        /// </summary>
        /// <param name="origin">원본 문자열</param>
        /// <returns></returns>
        static public string makeOnlyNum(string origin)
        {
            StringBuilder output = new StringBuilder(origin);
            for (int i = 0; i < output.Length; i++)
                if (!char.IsDigit(output[i]))
                    output.Remove(i--, 1);

            return output.ToString();
        }
    }
}
