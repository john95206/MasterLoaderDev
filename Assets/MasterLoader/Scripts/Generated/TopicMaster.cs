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
    public class TopicMaster : ScriptableObject
    {
        public List<Topic> TopicList { get { return _topicList; } }
        [SerializeField]
        private List<Topic> _topicList = new List<Topic>();

        public void SetData(string[] data)
        {
            var dataList = new List<Topic>();
            var obj = new Topic{};
            var doneIndex = 0;
            for (var valueIndex = 0; valueIndex < 30; valueIndex++)
            {
                var isDone = false;
                if (valueIndex == 0 || doneIndex >= 3)
                {
                    doneIndex = 0;
                    obj = new Topic{};
                }
                for (var parameterIndex = 0; parameterIndex < 3; parameterIndex++)
                {
                    if (isDone)
                    {
                        continue;
                    }
                    
                    switch (GetPrime(valueIndex, 3))
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
                            if (!Enum.TryParse<TopicType>(data[valueIndex], out var value))
                            {
                                OutputParseErrorLog(data[valueIndex], "enum");
                                break;
                            }
                            
                            obj.TopicType = value;
                            isDone = true;
                            doneIndex++;
                            continue;
                        }
                    }
                }
                if (doneIndex == 3)
                {
                    dataList.Add(obj);
                }
            }
            _topicList = dataList;
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