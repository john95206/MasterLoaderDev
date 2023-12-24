using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MasterLoader.Example
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField]
        private List<SpriteRenderer> _sprites = new List<SpriteRenderer>();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CharacterController))]
    public class CharacterControllerEditor : Editor
    {
        private void OnEnable()
        {
            
        }
    }
#endif
}