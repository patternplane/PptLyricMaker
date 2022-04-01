using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PptLyricMaker.Module
{
    class StringKMP
    {
        /// <summary>
        /// 전처리 테이블
        /// </summary>
        static private int[] patternTable;

        /// <summary>
        /// 문자 비교함수의 명세
        /// 같으면 true, 다르면 false일 것
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public delegate bool StringCompare(char a, char b);

        /// <summary>
        /// KMP - 전처리 테이블 제작 함수
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="compareFunc"></param>
        /// <returns></returns>
        static private int MakeTable(string sample, StringCompare compareFunc)
        {
            int len = sample.Length;
            patternTable = new int[len];

            int setter = 0;
            int checker = 0;
            patternTable[setter++] = checker;

            while (setter < len)
            {
                if (compareFunc(sample[setter] , sample[checker]))
                    patternTable[setter++] = ++checker;
                else
                {
                    if (checker == 0)
                        patternTable[setter++] = checker;
                    else
                        checker = patternTable[checker-1];
                }
            }

            return 0;
        }


        /// <summary>
        /// KMP 검색<br/>
        /// 모든 검색 성공 위치를 반환합니다.
        /// </summary>
        /// <param name="origin">
        /// 원본 문자열
        /// </param>
        /// <param name="sample">
        /// 찾을 문자열
        /// </param>
        /// <param name="compareFunc">
        /// 비교함수
        /// </param>
        /// <returns>
        /// 모든 발견위치를 반환합니다.<br/>
        /// 오류시 null을 반환
        /// </returns>
        static public int[] FindPattern(string origin, string sample, StringCompare compareFunc)
        {
            if (MakeTable(sample, compareFunc) == -1)
                return null;

            int o_len = origin.Length;
            int s_len = sample.Length;
            int o_i = 0;
            int s_i = 0;

            List<int> positions = new List<int>(50);

            while (o_i < o_len)
            {
                if (compareFunc(origin[o_i] , sample[s_i]))
                {
                    o_i++;
                    s_i++;

                    if (s_i == s_len)
                    {
                        positions.Add(o_i - s_len);
                        s_i = patternTable[s_i - 1];
                    }
                }
                else
                {
                    if (s_i == 0)
                        o_i++;
                    else
                        s_i = patternTable[s_i - 1];
                }
            }
            return positions.ToArray();
        }
        
    }
}
