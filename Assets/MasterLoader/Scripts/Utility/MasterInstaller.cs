using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using MasterLoader;
using Dev;

namespace MasterLoader
{
	public class MasterInstaller : MonoBehaviour
	{
		public testMasterMaster testMaster;
		public secondMaster second;
#if UNITY_EDITOR
		public void SetMaster()
		{
			testMaster = AssetDatabase.LoadMainAssetAtPath("Assets/MasterLoader/Master/testMaster.asset") as testMasterMaster;
			second = AssetDatabase.LoadMainAssetAtPath("Assets/MasterLoader/Master/second.asset") as secondMaster;
		}
#endif
	}
}