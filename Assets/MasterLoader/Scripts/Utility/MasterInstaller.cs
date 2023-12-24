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

namespace MasterLoader
{
	public class MasterInstaller : MonoBehaviour
	{
		public CharacterMaster Character { get { return _Character; } }
		[SerializeField]
		private CharacterMaster _Character;
		public Jujutsu.JujutsuMaster Jujutsu { get { return _Jujutsu; } }
		[SerializeField]
		private Jujutsu.JujutsuMaster _Jujutsu;
#if UNITY_EDITOR
		public void SetMaster()
		{
			_Character = AssetDatabase.LoadMainAssetAtPath("Assets/MasterLoader/Master/Character.asset") as CharacterMaster;
			_Jujutsu = AssetDatabase.LoadMainAssetAtPath("Assets/MasterLoader/Master/Jujutsu.asset") as Jujutsu.JujutsuMaster;
		}
#endif
	}
}