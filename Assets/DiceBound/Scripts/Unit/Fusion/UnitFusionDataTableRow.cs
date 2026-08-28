using System.Collections;
using System.Collections.Generic;
using KCoreKit;

namespace DiceBound
{
    public class UnitFusionDataTableRow : DataTableRowBase
    {
        public string inputUnit1;
        public string inputUnit2;
        public string inputUnit3; // 비어있으면 2입력 조합(레전더리 전용 레시피)
        public string outputUnit; // 비어있으면 "동일 유닛 3개 -> 다음 등급 무작위 1체" 승급 레시피

        // 비어있지 않으면 "이 등급에 속하는 아무 유닛이나 requiredCount개 보유 -> 다음 등급 무작위 1체" 와일드카드 레시피.
        // inputUnit1~3/outputUnit은 이 경우 전부 무시된다 (특정 유닛에 묶이지 않음).
        public string wildcardGrade;
        public int requiredCount; // 와일드카드 레시피 전용 - 몇 개를 모아야 하는지(동일 유닛 기준). 특정 레시피(와일드카드 아님)에서는 안 쓰임.
        public string nameKey; // 와일드카드 레시피 표시용 (커먼→레어 합성 등). 특정 레시피는 outputUnit의 nameKey를 그대로 씀.

        public bool IsWildcard => !string.IsNullOrEmpty(wildcardGrade);
        public bool IsRandomGradeUp => IsWildcard || string.IsNullOrEmpty(outputUnit);

        public IEnumerable<string> GetInputs()
        {
            if (IsWildcard)
            {
                yield break;
            }

            yield return inputUnit1;
            yield return inputUnit2;
            if (!string.IsNullOrEmpty(inputUnit3))
            {
                yield return inputUnit3;
            }
        }
    }
}