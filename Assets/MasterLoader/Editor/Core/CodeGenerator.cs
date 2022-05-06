using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace MasterLoader
{
    public class CodeGenerator
    {
        static CodeGenerator() { }

        private class EnumValue
        {
            public string Parameter;
            public List<string> ValueList = new List<string>();
        }

        public const string CS_PATH = "Assets/MasterLoader/Scripts/Generated/";
        private static List<EnumValue> _enumValues = new List<EnumValue>();

        private const string _NAMESPACE = "MasterLoader";
        public const string CS = ".cs";
        private const string _TAB = "\t";
        private const string _LINE = "\n";

        private static int GetPrime(int value, int length)
        {
            var _value = value;
            while (_value >= length)
            {
                _value -= length;
            }
            return _value;
        }

        public static bool Generate(string masterName, string masterPath, string Master, Base result)
        {
            var nameSpace = _NAMESPACE;
            var typeList = result.Type;
            var commentList = result.Comment;
            var parameterList = result.Parameter;
            var valueList = result.ValueList;

            var masterProperty = $"{ masterName }List";

            if (!Directory.Exists(CS_PATH))
            {
                Directory.CreateDirectory(CS_PATH);
            }
            if (!Directory.Exists(masterPath))
            {
                Directory.CreateDirectory(masterPath);
            }
            var body = string.Empty;
            for (var i = 0; i < typeList.Length; i++)
            {
                var parameter = string.Empty;
                var comment = string.Empty;
                if (!string.IsNullOrEmpty(commentList[i]))
                {
                    var comments = commentList[i].Split('\n');
                    for (var row = 0; row < comments.Length; row++)
                    {
                        comment += $"{GetBaseIndent(2)}/// {comments[row]}";
                    }
                    comment = 
                        $"{_TAB}{_TAB}/// <summary>" +
                        $"{comment}" +
                        $"{GetBaseIndent(2)}/// </summary>{_LINE}";
                }

                var parameterString = string.Empty;
                if (typeList[i].Equals("enum"))
                {
                    parameterString = $"{parameterList[i].ToUpper()} {parameterList[i].ToLower()}";
                }
                else
                {
                    parameterString = $"{ typeList[i]} { parameterList[i]}";
                }
                parameter =
                    _LINE +
                    $"{comment}" +
                    $"{_TAB}{_TAB}public {parameterString};";
                body += parameter;
            }

            var rowCode =
                $"using System;{_LINE}" +
                $"namespace {_NAMESPACE}{_LINE}" +
                $"{{" +
                $"{GetBaseIndent(1)}[Serializable]" +
                $"{GetBaseIndent(1)}public class {masterName}" +
                $"{GetBaseIndent(1)}{{" +
                $"{body}" +
                $"{GetBaseIndent(1)}}}{_LINE}" +
                $"}}";

            var parameterCode = string.Empty;

            try
            {
                var switchCode = GenerateSwitchCode(parameterList, typeList, masterProperty, out var enumIndexList);

                if (string.IsNullOrEmpty(switchCode))
                {
                    throw new Exception($"MasterLoader Info: MasterLoader supports only 'int', 'float', 'double', 'bool', 'string', 'enum' type.\n check your master sheet's type or value row.");
                }

                parameterCode = GenerateParameterCode(typeList, switchCode);

                AddEnumList(valueList, enumIndexList, typeList, parameterList);

                rowCode =
                $"using System;{_LINE}" +
                $"namespace {_NAMESPACE}{_LINE}" +
                $"{{" +
                $"{GetBaseIndent(1)}[Serializable]" +
                $"{GetBaseIndent(1)}public class {masterName}" +
                $"{GetBaseIndent(1)}{{" +
                $"{GetBaseIndent(1)}{_TAB}{body}" +
                $"{GetBaseIndent(1)}}}" +
                $"{GenerateEnumCode()}{_LINE}" +
                $"}}";
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }

            try
            {
                var length = parameterList.Length - _enumValues.Count;
                var setDataCode = GenerateMasterFunctionCode(masterName, masterProperty, valueList, length, parameterCode);
                var masterCode = GenerateMasterCode(masterName, Master, masterProperty, nameSpace, setDataCode);

                var rowCsPath = $"{CS_PATH}{masterName}{CS}";
                var masterCsPath = $"{CS_PATH}{masterName}{Master}{CS}";

                using (var sw = new StreamWriter(rowCsPath, false, Encoding.UTF8))
                {
                    sw.Write(rowCode);
                }
                using (var sw = new StreamWriter(masterCsPath, false, Encoding.UTF8))
                {
                    sw.Write(masterCode);
                }
                AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"MasterLoader Info: {e.Message}");
                Debug.LogError("MasterLoader Info: MasterLoader successed loading master data, but couldn't get argument successfuly.\n please check your master sheet's 'type row' or 'sheet name'");
                return false;
            }
        }

        private static void AddEnumList(string[] valueList, List<int> enumIndexList, string[] typeList, string[] parameterList)
        {
            if(valueList.Length < 1)
            {
                return;
            }
            for (var i = 0; i < valueList.Length; i++)
            {
                foreach (var enumIndex in enumIndexList)
                {
                    if (GetPrime(i, typeList.Length) == enumIndex)
                    {
                        var value = valueList[i];
                        Debug.Log(value);
                        var hasExisted = false;
                        if (_enumValues.Count > 0)
                        {
                            foreach (var ev in _enumValues)
                            {
                                hasExisted = ev.Parameter.Equals(parameterList[enumIndex]);
                                if (!hasExisted)
                                {
                                    continue;
                                }
                                if (ev.ValueList.Contains(value))
                                {
                                    Debug.Log($"MasterLoaderInfo: {parameterList[enumIndex]} and {value} has existed");
                                    break;
                                }
                                ev.ValueList.Add(value);
                                break;
                            }
                        }
                        if (!hasExisted)
                        {
                            _enumValues.Add(new EnumValue { Parameter = parameterList[enumIndex], ValueList = new List<string>() { value } });
                        }
                    }
                }
            }
        }

        private static string GenerateSwitchCode(string[] parameterList, string[] typeList, string masterProperty, out List<int> enumIndexList)
        {
            enumIndexList = new List<int>();
            var code = string.Empty;
            for (var parameterIndex = 0; parameterIndex < parameterList.Length; parameterIndex++)
            {
                var parameter = parameterList[parameterIndex];
                var type = typeList[parameterIndex];
                switch (type)
                {
                    case "string":
                        code +=
                        $"{GetBaseIndent(6)}case {parameterIndex}:" +
                        $"{GetBaseIndent(6)}{{" +
                        $"{GetBaseIndent(6)}{_TAB}var value = data[valueIndex];" +
                        $"{GetBaseIndent(6)}{_TAB}{GetInputCode(masterProperty, parameter)}" +
                        $"{GetBaseIndent(6)}}}";
                        break;
                    case "int":
                    case "float":
                    case "double":
                    case "bool":
                        code += GetSwitchCode(type, masterProperty, parameter, parameterIndex);
                        break;
                    case "enum":
                        code += $"{GetBaseIndent(6)}case {parameterIndex}:" +
                        $"{GetBaseIndent(6)}{{" +
                        $"{GetBaseIndent(6)}{_TAB}if(!Enum.TryParse<{parameter.ToUpper()}>(data[valueIndex], out var value))" +
                        $"{GetBaseIndent(6)}{_TAB}{{" +
                        $"{GetBaseIndent(6)}{_TAB}{_TAB}OutputParseErrorLog(data[valueIndex], \"{type}\");" +
                        $"{GetBaseIndent(6)}{_TAB}{_TAB}break;" +
                        $"{GetBaseIndent(6)}{_TAB}}}" +
                        $"{GetBaseIndent(6)}{_TAB}{GetInputCode(masterProperty, parameter)}" +
                        $"{GetBaseIndent(6)}}}";
                        enumIndexList.Add(parameterIndex);
                        Debug.Log($"{parameterIndex} is enumIndex");
                        break;
                    default:
                        Debug.LogError($"MasterLoader Info: unexpected parameter: {parameterList[parameterIndex]}. MasterLoader supports only 'int', 'float', 'double', 'bool', 'string', 'enum' type.\n check your master sheet's type or value row.");
                        break;
                }
            }
            return code;
        }

        private static string GenerateParameterCode(string[] typeList, string switchCode)
        {
            return
            $"{GetBaseIndent(5)}switch(GetPrime(valueIndex, {typeList.Length}))" +
            $"{GetBaseIndent(5)}{{" +
                                    switchCode +
            $"{GetBaseIndent(5)}}}";
        }

        private static string GenerateEnumCode()
        {
            var code = string.Empty;
            if (_enumValues.Count < 1)
            {
                return code;
            }
            foreach (var ev in _enumValues)
            {
                Debug.Log($"MasterLoaderInfo: {ev.Parameter} enum generatable.");
                var valuesString = string.Empty;
                for (var vIndex = 0; vIndex < ev.ValueList.Count; vIndex++)
                {
                    valuesString +=
                    $"{GetBaseIndent(2)}{ev.ValueList[vIndex]},";
                    Debug.Log($"{ev.ValueList[vIndex]}");
                }
                code += $"{_LINE}" +
                $"{GetBaseIndent(1)}public enum {ev.Parameter.ToUpper()}" +
                $"{GetBaseIndent(1)}{{{valuesString}" +
                $"{GetBaseIndent(1)}}}";
            }
            return code;
        }

        private static string GenerateMasterFunctionCode(string masterName, string masterProperty, string[] valueList, int length, string parameterCode)
        {
            return
            $"{GetBaseIndent(3)}var dataList = new List<{masterName}>();" +
            $"{GetBaseIndent(3)}var {masterProperty} = new {masterName}{{}};" +
            $"{GetBaseIndent(3)}var doneIndex = 0;" +
            $"{GetBaseIndent(3)}for(var valueIndex = 0; valueIndex < {valueList.Length}; valueIndex++)" +
            $"{GetBaseIndent(3)}{{" +
            $"{GetBaseIndent(3)}{_TAB}var isDone = false;" +
            $"{GetBaseIndent(3)}{_TAB}if(valueIndex == 0 || doneIndex >= {length})" +
            $"{GetBaseIndent(3)}{_TAB}{{" +
            $"{GetBaseIndent(3)}{_TAB}{_TAB}Debug.Log(\"new Instance\");" +
            $"{GetBaseIndent(3)}{_TAB}{_TAB}Debug.Log(valueIndex);" +
            $"{GetBaseIndent(3)}{_TAB}{_TAB}doneIndex = 0;" +
            $"{GetBaseIndent(3)}{_TAB}{_TAB}{masterProperty} = new {masterName}{{}};" +
            $"{GetBaseIndent(3)}{_TAB}}}" +
            $"{GetBaseIndent(3)}{_TAB}for(var parameterIndex = 0; parameterIndex < {length}; parameterIndex++)" +
            $"{GetBaseIndent(3)}{_TAB}{{" +
            $"{GetBaseIndent(3)}{_TAB}{_TAB}if(isDone)" +
            $"{GetBaseIndent(3)}{_TAB}{_TAB}{{" +
            $"{GetBaseIndent(3)}{_TAB}{_TAB}{_TAB}continue;" +
            $"{GetBaseIndent(3)}{_TAB}{_TAB}}}" +
            $"{GetBaseIndent(3)}{_TAB}{_TAB}{parameterCode}" +
            $"{GetBaseIndent(3)}{_TAB}}}" +
            $"{GetBaseIndent(3)}{_TAB}if(doneIndex == {length} - 1)" +
            $"{GetBaseIndent(3)}{_TAB}{{" +
            $"{GetBaseIndent(3)}{_TAB}{_TAB}dataList.Add({masterProperty});" +
            $"{GetBaseIndent(3)}{_TAB}}}" +
            $"{GetBaseIndent(3)}}}" +
            $"{GetBaseIndent(3)}_{masterProperty} = dataList;";
        }

        private static string GenerateMasterCode(string masterName, string Master, string masterProperty, string nameSpace, string setDataCode)
        {
            return
            $"using UnityEngine;{_LINE}" +
            $"using System.Collections.Generic;{_LINE}" +
            $"using System;{_LINE}{_LINE}" +
            $"namespace {nameSpace}{_LINE}" +
            $"{{" +
            $"{GetBaseIndent(1)}[CreateAssetMenu]" +
            $"{GetBaseIndent(1)}public class {masterName}{Master} : ScriptableObject" +
            $"{GetBaseIndent(1)}{{" +
            $"{GetBaseIndent(1)}{_TAB}public List<{masterName}> {masterProperty} => _{masterProperty};" +
            $"{GetBaseIndent(1)}{_TAB}[SerializeField]" +
            $"{GetBaseIndent(1)}{_TAB}private List<{masterName}> _{masterProperty} = new List<{masterName}>();{_LINE}" +

            $"{GetBaseIndent(1)}{_TAB}public void SetData(string[] data)" +
            $"{GetBaseIndent(1)}{_TAB}{{" +
            $"{_TAB}{_TAB}{_TAB}{setDataCode}" +
            $"{GetBaseIndent(1)}{_TAB}}}{_LINE}" +

            $"{GetBaseIndent(1)}{_TAB}private int GetPrime(int value, int length)" +
            $"{GetBaseIndent(1)}{_TAB}{{" +
            $"{GetBaseIndent(1)}{_TAB}{_TAB}var _value = value;" +
            $"{GetBaseIndent(1)}{_TAB}{_TAB}while (_value >= length)" +
            $"{GetBaseIndent(1)}{_TAB}{_TAB}{{" +
            $"{GetBaseIndent(1)}{_TAB}{_TAB}_value -= length;" +
            $"{GetBaseIndent(1)}{_TAB}{_TAB}}}" +
            $"{GetBaseIndent(1)}{_TAB}{_TAB}return _value;" +
            $"{GetBaseIndent(1)}{_TAB}}}{_LINE}" +

            $"{GetBaseIndent(1)}{_TAB}private void OutputParseErrorLog(string s, string type)" +
            $"{GetBaseIndent(1)}{_TAB}{{" +
            $"{GetBaseIndent(1)}{_TAB}{_TAB}Debug.LogError(($\"MasterLoaderInfo: could not cast {{s}} to {{type}}.\"));" +
            $"{GetBaseIndent(1)}{_TAB}}}" +
            $"{GetBaseIndent(1)}}}" +
            $"{_LINE}}}";
        }

        private static string GetBaseIndent(int num = 4)
        {
            var value = _LINE;
            for(var i = 0; i < num; i++)
            {
                value += _TAB;
            }
            return value;
        }

        private static string GetInputCode(string masterProperty, string parameter)
        {
            return
            $"{GetBaseIndent(7)}{masterProperty}.{parameter} = value;" +
            $"{GetBaseIndent(7)}isDone = true;" +
            $"{GetBaseIndent(7)}doneIndex++;" +
            $"{GetBaseIndent(7)}continue;";
        }

        private static string GetSwitchCode(string type, string masterProperty, string parameter, int parameterIndex)
        {
            return
            $"{GetBaseIndent(6)}case {parameterIndex}:" +
            $"{GetBaseIndent(6)}{{" +
            $"{GetBaseIndent(6)}{_TAB}if(!{type}.TryParse(data[valueIndex], out var value))" +
            $"{GetBaseIndent(6)}{_TAB}{{" +
            $"{GetBaseIndent(6)}{_TAB}{_TAB}OutputParseErrorLog(data[valueIndex], \"{type}\");" +
            $"{GetBaseIndent(6)}{_TAB}{_TAB}break;" +
            $"{GetBaseIndent(6)}{_TAB}}}" +
            $"{GetBaseIndent(6)}{_TAB}{GetInputCode(masterProperty, parameter)}" +
            $"{GetBaseIndent(6)}}}";
        }
    }
}