/*
* ---------------------------------------------
*this Code is Auto Generated.
* All Changes will be nothing when regenerated.
* ---------------------------------------------
* これは自動生成コードです。
* このコードに行った変更は自動生成時に破棄されます。
* ---------------------------------------------
*/

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using WaitingForYou.Master;

namespace MasterLoader
{
    public class MasterFacade : MonoBehaviour
    {
        public CharacterMaster Character { get { return _character; } }
        [SerializeField]
        private CharacterMaster _character;
        public NarrativeMaster Narrative { get { return _narrative; } }
        [SerializeField]
        private NarrativeMaster _narrative;
        public TopicMaster Topic { get { return _topic; } }
        [SerializeField]
        private TopicMaster _topic;
        public TopicOptionMaster TopicOption { get { return _topicOption; } }
        [SerializeField]
        private TopicOptionMaster _topicOption;
#if UNITY_EDITOR
        public void SetMaster()
        {
            _character = AssetDatabase.LoadMainAssetAtPath("Assets/_Root/Master/Character.asset") as CharacterMaster;
            _narrative = AssetDatabase.LoadMainAssetAtPath("Assets/_Root/Master/Narrative.asset") as NarrativeMaster;
            _topic = AssetDatabase.LoadMainAssetAtPath("Assets/_Root/Master/Topic.asset") as TopicMaster;
            _topicOption = AssetDatabase.LoadMainAssetAtPath("Assets/_Root/Master/TopicOption.asset") as TopicOptionMaster;
            Debug.Log($"MasterLoader Info: Master data has injected completely.");
        }
#endif
    }
}