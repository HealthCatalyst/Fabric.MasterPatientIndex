using System;

namespace MasterPatientIndex.ProbabilisticMPI
{
   
    public static class FuzzyComparer
    {
        private const double MWeightThreshold = 0.7;
        private const int MNumChars = 4;

        //http://buddydroid.com/jarowinkler-distance-c-implementation/

        public static double JaroDistance(this string strA, string strB)
        {
            var cleanStrA = strA.ToUpper();
            var cleanStrB = strB.ToUpper();

            var lenA = cleanStrA.Length;
            var lenB = cleanStrB.Length;
            if (lenA == 0)
                return lenB == 0 ? 1.0 : 0.0;

            var lSearchRange = Math.Max(0, Math.Max(lenA, lenB) / 2 - 1);

            var lMatched1 = new bool[lenA];
            var lMatched2 = new bool[lenB];
            
            var lNumCommon = 0;
            for (var i = 0; i < lenA; ++i)
            {
                var lStart = Math.Max(0, i - lSearchRange);
                var lEnd = Math.Min(i + lSearchRange + 1, lenB);
                for (int j = lStart; j < lEnd; ++j)
                {
                    if (lMatched2[j]) continue;
                    if (cleanStrA[i] != cleanStrB[j])
                        continue;
                    lMatched1[i] = true;
                    lMatched2[j] = true;
                    ++lNumCommon;
                    break;
                }
            }
            if (lNumCommon == 0) return 0.0;

            var lNumHalfTransposed = 0;
            var k = 0;
            for (var i = 0; i < lenA; ++i)
            {
                if (!lMatched1[i]) continue;
                while (!lMatched2[k]) ++k;
                if (cleanStrA[i] != cleanStrB[k])
                    ++lNumHalfTransposed;
                ++k;
            }
            // System.Diagnostics.Debug.WriteLine("numHalfTransposed=" + numHalfTransposed);
            var lNumTransposed = lNumHalfTransposed / 2;

            // System.Diagnostics.Debug.WriteLine("numCommon=" + numCommon + " numTransposed=" + numTransposed);
            double lNumCommonD = lNumCommon;
            double lWeight = (lNumCommonD / lenA
                             + lNumCommonD / lenB
                             + (lNumCommon - lNumTransposed) / lNumCommonD) / 3.0;

            return lWeight;

            /* EQ:  winkler portion, don't need this
            if (lWeight <= MWeightThreshold) return lWeight;
            int lMax = Math.Min(MNumChars, Math.Min(aString1.Length, aString2.Length));
            int lPos = 0;
            while (lPos < lMax && aString1[lPos] == aString2[lPos])
                ++lPos;
            if (lPos == 0) return lWeight;
            var result = lWeight + 0.1 * lPos * (1.0 - lWeight);
            return result;
             * */

        }
  
    }
}
