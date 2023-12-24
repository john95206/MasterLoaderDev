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
using System.Collections.Generic;
using System;

namespace WaitingForYou.Master
{
    public class NarrativeMaster : ScriptableObject
    {
        public List<Narrative> NarrativeList { get { return _narrativeList; } }
        [SerializeField]
        private List<Narrative> _narrativeList = new List<Narrative>();

        public void SetData(string[] data)
        {
            var dataList = new List<Narrative>();
            var obj = new Narrative{};
            var doneIndex = 0;
            for (var valueIndex = 0; valueIndex < 24; valueIndex++)
            {
                var isDone = false;
                if (valueIndex == 0 || doneIndex >= 4)
                {
                    doneIndex = 0;
                    obj = new Narrative{};
                }
                for (var parameterIndex = 0; parameterIndex < 4; parameterIndex++)
                {
                    if (isDone)
                    {
                        continue;
                    }
                    
                    switch (GetPrime(valueIndex, 4))
                    {
                        case 0:
                        {
                            if (!int.TryParse(data[valueIndex], out var value))
                            {
                                OutputParseErrorLog(data[valueIndex], "int");
                                break;
                            }
                            
                            obj.NarrativeId = value;
                            isDone = true;
                            doneIndex++;
                            continue;
                        }
                        case 1:
                        {
                            if (!Enum.TryParse<NarrativeType>(data[valueIndex], out var value))
                            {
                                OutputParseErrorLog(data[valueIndex], "enum");
                                break;
                            }
                            
                            obj.NarrativeType = value;
                            isDone = true;
                            doneIndex++;
                            continue;
                        }
                        case 2:
                        {
                            if (!Enum.TryParse<CharacterType>(data[valueIndex], out var value))
                            {
                                OutputParseErrorLog(data[valueIndex], "enum");
                                break;
                            }
                            
                            obj.CharacterType = value;
                            isDone = true;
                            doneIndex++;
                            continue;
                        }
                        case 3:
                        {
                            var value = data[valueIndex];
                            
                            obj.Body = value;
                            isDone = true;
                            doneIndex++;
                            continue;
                        }
                    }
                }
                if (doneIndex == 4)
                {
                    dataList.Add(obj);
                }
            }
            _narrativeList = dataList;
        }

        private int GetPrime(int value, int length)
        {
            var _value = value;
            while (_value >= length)
            {
                _value -= length;
            }
            return _value;
        }

        private void OutputParseErrorLog(string s, string type)
        {
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            Debug.LogError(($"MasterLoaderInfo: could not cast {s} to {type}."));
        }
    }
}