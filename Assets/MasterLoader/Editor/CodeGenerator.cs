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

        private static List<EnumValue> EnumValues = new List<EnumValue>();

        private const string cs = ".cs";

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
            var typeList = result.Type;
            var commentList = result.Comment;
            var parameterList = result.Parameter;
            var valueList = result.ValueList;

            var csPath = "Assets/MasterLoader/Scripts/Generated/";
            masterPath = "Assets/MasterLoader/Resources/Master/";
            var masterProperty = $"{ masterName }List";

            if (!Directory.Exists("Assets/MasterLoader/Scripts/Generated"))
            {
                Directory.CreateDirectory("Assets/MasterLoader/Scripts/Generated");
            }
            if (!Directory.Exists(csPath))
            {
                Directory.CreateDirectory(csPath);
            }
            if (!Directory.Exists("Assets/MasterLoader/Resources"))
            {
                Directory.CreateDirectory("Assets/MasterLoader/Resources");
            }
            if (!Directory.Exists(masterPath))
            {
                Directory.CreateDirectory(masterPath);
            }
            var body = string.Empty;
            for (var i = 0; i < typeList.Length; i++)
            {
                var comment = string.Empty;
                if (commentList[i] != string.Empty)
                {
                    var comments = commentList[i].Split('\n');
                    for (var row = 0; row < comments.Length; row++)
                    {
                        comment += $@"    /// {comments[row]}
";
                    }
                    comment = $@"/// <summary>
{comment}    /// </summary>";
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
                body += $@"
    {comment}
    public {parameterString};";
            }

            var rowCode =
                $@"using System;

[Serializable]
public class {masterName}
{{{body}
}}";

            try
            {
                var parameterCode = string.Empty;
                try
                {
                    var enumIndexList = new List<int>();
                    var switchCode = string.Empty;
                    for (var parameterIndex = 0; parameterIndex < parameterList.Length; parameterIndex++)
                    {
                        var parameter = parameterList[parameterIndex];
                        switch (typeList[parameterIndex])
                        {
                            case "string":
                                switchCode += $@"
                    case {parameterIndex}:
                    {{
                        {masterProperty}.{parameter} = data[valueIndex];
                        Debug.Log($""{masterProperty}.{parameter} = {{data[valueIndex]}}"");
                        Debug.Log($""doneIndex = {{doneIndex}}"");
                        isDone = true;
                        doneIndex++;
                        continue;
                    }}";
                                break;
                            case "int":
                                switchCode += $@"
                    case {parameterIndex}:
                    {{
                        if(!int.TryParse(data[valueIndex], out var number))
                        {{
                            OutputParseErrorLog(data[valueIndex], ""int"");
                            break;
                        }}
                        {masterProperty}.{parameter} = number;
                        Debug.Log($""{masterProperty}.{parameter} = {{data[valueIndex]}}"");
                        Debug.Log($""doneIndex = {{doneIndex}}"");
                        isDone = true;
                        doneIndex++;
                        continue;
                    }}";
                                break;
                            case "float":
                                switchCode += $@"
                    case {parameterIndex}:
                    {{
                        if(!float.TryParse(data[valueIndex], out var number))
                        {{
                            OutputParseErrorLog(data[valueIndex], ""float"");
                            break;
                        }}
                        {masterProperty}.{parameter} = number;
                        Debug.Log($""{masterProperty}.{parameter} = {{data[valueIndex]}}"");
                        Debug.Log($""doneIndex = {{doneIndex}}"");
                        isDone = true;
                        doneIndex++;
                        continue;
                    }}";
                                break;
                            case "double":
                                switchCode += $@"
                    case {parameterIndex}:
                    {{
                        if(!double.TryParse(data[valueIndex], out var number))
                        {{
                            OutputParseErrorLog(data[valueIndex], ""double"");
                            break;
                        }}
                        {masterProperty}.{parameter} = number;
                        Debug.Log($""{masterProperty}.{parameter} = number"");
                        Debug.Log($""doneIndex = {{doneIndex}}"");
                        isDone = true;
                        doneIndex++;
                        continue;
                    }}";
                                break;
                            case "bool":
                                switchCode += $@"
                    case {parameterIndex}:
                    {{
                        if(!bool.TryParse(data[valueIndex], out var value))
                        {{
                            OutputParseErrorLog(data[valueIndex], ""bool"");
                            break;
                        }}
                        {masterProperty}.{parameter} = value;
                        Debug.Log($""{masterProperty}.{parameter} = {{data[valueIndex]}}"");
                        Debug.Log($""doneIndex = {{doneIndex}}"");
                        isDone = true;
                        doneIndex++;
                        continue;
                    }}";
                                break;
                            case "enum":
                                switchCode += $@"
                    case {parameterIndex}:
                    {{
                        if(!Enum.TryParse<{parameter.ToUpper()}>(data[valueIndex], out var value))
                        {{
                            OutputParseErrorLog(data[valueIndex], ""enum"");
                            break;
                        }}
                        {masterProperty}.{parameter} = value;
                        Debug.Log($""{masterProperty}.{parameter} = {{data[valueIndex]}}"");
                        Debug.Log($""doneIndex = {{doneIndex}}"");
                        isDone = true;
                        doneIndex++;
                        continue;
                    }}";
                                enumIndexList.Add(parameterIndex);
                                Debug.Log($"{parameterIndex} is enumIndex");
                                break;
                            default:
                                Debug.LogError($"MasterLoader Info: unexpected parameter: {parameterList[parameterIndex]}. MasterLoader supports only 'int', 'float', 'double', 'bool', 'string', 'enum' type.\n check your master sheet's type or value row.");
                                break;
                        }
                    }

                    parameterCode = $@"
                switch(GetPrime(valueIndex, {typeList.Length}))
                {{
                    {switchCode}
                }}";

                    for(var i = 0; i < valueList.Length; i++)
                    {
                        foreach(var enumIndex in enumIndexList)
                        {
                            if (GetPrime(i, typeList.Length) == enumIndex)
                            {
                                var value = valueList[i];
                                Debug.Log(value);
                                var hasExisted = false;
                                if (EnumValues.Count > 0)
                                {
                                    foreach (var ev in EnumValues)
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
                                    EnumValues.Add(new EnumValue { Parameter = parameterList[enumIndex], ValueList = new List<string>() { value } });
                                }
                            }
                        }
                    }

                    if(EnumValues.Count > 0)
                    {
                        foreach(var ev in EnumValues)
                        {
                            Debug.Log($"MasterLoaderInfo: {ev.Parameter} enum generatable.");
                            var valuesString = string.Empty;
                            for(var vIndex = 0; vIndex < ev.ValueList.Count; vIndex++)
                            {
                                valuesString += $@"
    {ev.ValueList[vIndex]},";
                                Debug.Log($"{ev.ValueList[vIndex]}");
                            }
                            rowCode += $@"

public enum {ev.Parameter.ToUpper()}
{{{valuesString}
}}";
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    Debug.LogError($"MasterLoader Info: MasterLoader supports only 'int', 'float', 'double', 'bool', 'string', 'enum' type.\n check your master sheet's type or value row.");
                    return false;
                }

                var length = parameterList.Length - EnumValues.Count;
                var setDataCode = $@"var dataList = new List<{masterName}>();
        var {masterProperty} = new {masterName}{{}};
        var doneIndex = 0;
        for(var valueIndex = 0; valueIndex < {valueList.Length}; valueIndex++)
        {{
            var isDone = false;
            if(valueIndex == 0 || doneIndex >= {length})
            {{
                Debug.Log(""new Instance"");
                Debug.Log(valueIndex);
                doneIndex = 0;
                {masterProperty} = new {masterName}{{}};
            }}
            for(var parameterIndex = 0; parameterIndex < {length}; parameterIndex++)
            {{
                if(isDone)
                {{
                    continue;
                }}
{parameterCode}
            }}
            if(doneIndex == {length} - 1)
            {{
                dataList.Add({masterProperty});
            }}
        }}
        _{masterProperty} = dataList;";
                var masterCode =
                    $@"using UnityEngine;
using System.Collections.Generic;
using System;
using MasterLoader;

[CreateAssetMenu]
public class {masterName}{Master} : ScriptableObject
{{
    public List<{masterName}> {masterProperty} => _{masterProperty};
    [SerializeField]
    private List<{masterName}> _{masterProperty} = new List<{masterName}>();


    public void SetData(string[] data)
    {{
        {setDataCode}
    }}

    private int GetPrime(int value, int length)
    {{
        var _value = value;
        while (_value >= length)
        {{
            _value -= length;
        }}
        return _value;
    }}

    private void OutputParseErrorLog(string s, string type)
    {{
        Debug.LogError(($""MasterLoaderInfo: could not cast {{s}} to {{type}}.""));
    }}
}}";

                var rowCsPath = $"{csPath}{masterName}{cs}";
                var masterCsPath = $"{csPath}{masterName}{Master}{cs}";

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
    }
}