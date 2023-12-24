/*
* ---------------------------------------------
*this Code is Auto Generated.
* All Changes will be nothing when regenerated.
* ---------------------------------------------
* これは自動生成コードです。
* このコードに行った変更は自動生成時に破棄されます。
* ---------------------------------------------
*/

using System;
namespace WaitingForYou.Master
{
    [Serializable]
    public class Narrative
    {
        

        /// <summary>
        /// ユニークID
        /// </summary>
        public int NarrativeId;

        /// <summary>
        /// セリフタイプ
        /// </summary>
        public NarrativeType NarrativeType;

        /// <summary>
        /// キャラクタータイプ
        /// </summary>
        public CharacterType CharacterType;

        /// <summary>
        /// セリフの中身
        /// </summary>
        public string Body;
    }
}