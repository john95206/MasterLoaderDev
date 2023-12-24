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
    public class Character
    {
        

        /// <summary>
        /// ユニークID
        /// </summary>
        public int CharacterId;

        /// <summary>
        /// キャラクターの種類
        /// </summary>
        public CharacterType CharacterType;

        /// <summary>
        /// キャラクター名
        /// </summary>
        public string Name;

        /// <summary>
        /// お気に入りの話題
        /// </summary>
        public TopicType TopicType;
    }
}