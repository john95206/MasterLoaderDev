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
    public class TopicOptionMaster : ScriptableObject
    {
        public List<TopicOption> TopicOptionList { get { return _topicOptionList; } }
        [SerializeField]
        private List<TopicOption> _topicOptionList = new List<TopicOption>();

        public void SetData(string[] data)
        {
            var dataList = new List<TopicOption>();
            var obj = new TopicOption{};
            var doneIndex = 0;
            for (var valueIndex = 0; valueIndex < 35; valueIndex++)
            {
                var isDone = false;
                if (valueIndex == 0 || doneIndex >= 5)
                {
                    doneIndex = 0;
                    obj = new TopicOption{};
                }
                for (var parameterIndex = 0; parameterIndex < 5; parameterIndex++)
                {
                    if (isDone)
                    {
                        continue;
                    }
                    
                    switch (GetPrime(valueIndex, 5))
                    {
                        case 0:
                        {
                            if (!int.TryParse(data[valueIndex], out var value))
                            {
                                OutputParseErrorLog(data[valueIndex], "int");
                                break;
                            }
                            
                            obj.Id = value;
                            isDone = true;
                            doneIndex++;
                            continue;
                        }
                        case 1:
                        {
                            var value = data[valueIndex];
                            
                            obj.Body = value;
                            isDone = true;
                            doneIndex++;
                            continue;
                        }
                        case 2:
                        {
                            if (!int.TryParse(data[valueIndex], out var value))
                            {
                                OutputParseErrorLog(data[valueIndex], "int");
                                break;
                            }
                            
                            obj.Interestingness = value;
                            isDone = true;
                            doneIndex++;
                            continue;
                        }
                        case 3:
                        {
                            if (!int.TryParse(data[valueIndex], out var value))
                            {
                                OutputParseErrorLog(data[valueIndex], "int");
                                break;
                            }
                            
                            obj.Witness = value;
                            isDone = true;
                            doneIndex++;
                            continue;
                        }
                        case 4:
                        {
                            if (!int.TryParse(data[valueIndex], out var value))
                            {
                                OutputParseErrorLog(data[valueIndex], "int");
                                break;
                            }
                            
                            obj.Friendlieness = value;
                            isDone = true;
                            doneIndex++;
                            continue;
                        }
                    }
                }
                if (doneIndex == 5)
                {
                    dataList.Add(obj);
                }
            }
            _topicOptionList = dataList;
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