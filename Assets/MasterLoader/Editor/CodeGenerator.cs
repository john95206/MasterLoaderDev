using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Object = UnityEngine.Object;

namespace MasterLoader
{
    public class CodeGenerator
    {
        static CodeGenerator() { }

        private const string cs = ".cs";

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
                    for(var row = 0; row < comments.Length; row++)
                    {
                        comment += "    /// " + comments[row] + "\n";
                    }
                    comment = $@"/// <summary>
{comment}    /// </summary>";
                }

                body += $@"
    {comment}
    public {typeList[i]} {parameterList[i]};";
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
                    for (var parameterIndex = 0; parameterIndex < parameterList.Length; parameterIndex++)
                    {
                        var parameter = parameterList[parameterIndex];
                        switch (typeList[parameterIndex])
                        {
                            case "string":
                                parameterCode += $@"                if(GetPrime(valueIndex, {typeList.Length}) == {parameterIndex})
                {{
                    {masterProperty}.{parameter} = data[valueIndex];
                    Debug.Log($""{masterProperty}.{parameter} = {{data[valueIndex]}}"");
                    Debug.Log($""doneIndex = {{doneIndex}}"");
                    isDone = true;
                    doneIndex++;
                    continue;
                }}
";
                                break;
                            case "int":
                                parameterCode += $@"                if(GetPrime(valueIndex, {typeList.Length}) == {parameterIndex})
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
                }}
";
                                break;
                            case "float":
                                parameterCode += $@"                if(GetPrime(valueIndex, {typeList.Length}) == {parameterIndex})
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
                }}
";
                                break;
                            case "double":
                                parameterCode += $@"                if(GetPrime(valueIndex, {typeList.Length}) == {parameterIndex})
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
                }}
";
                                break;
                            case "bool":
                                parameterCode += $@"                if(GetPrime(valueIndex, {typeList.Length}) == {parameterIndex})
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
                }}
";
                                break;
                            default:
                                Debug.LogError($"MasterLoader Info: unexpected parameter: {parameterList[parameterIndex]}. MasterLoader supports only 'int', 'float', 'double', 'bool', 'string' type.\n check your master sheet's type or value row.");
                                break;
                        }
                        if (parameterIndex < parameterList.Length - 1)
                        {
                            parameterCode += $@"                else
";
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    Debug.LogError($"MasterLoader Info: MasterLoader supports only 'int', 'float', 'double', 'bool', 'string' type.\n check your master sheet's type or value row.");
                    return false;
                }

                var setDataCode = $@"var dataList = new List<{masterName}>();
        var {masterProperty} = new {masterName}{{}};
        var doneIndex = 0;
        for(var valueIndex = 0; valueIndex < {valueList.Length}; valueIndex++)
        {{
            var isDone = false;
            if(valueIndex == 0 || doneIndex >= {parameterList.Length})
            {{
                Debug.Log(""new Instance"");
                Debug.Log(valueIndex);
                Debug.Log(valueIndex);
                doneIndex = 0;
                {masterProperty} = new {masterName}{{}};
            }}
            for(var parameterIndex = 0; parameterIndex < {parameterList.Length}; parameterIndex++)
            {{
                if(isDone)
                {{
                    continue;
                }}
{parameterCode}
            }}
            if(doneIndex == {parameterList.Length} - 1)
            {{
                dataList.Add({masterProperty});
            }}
        }}
        _{masterProperty} = dataList;";
                var masterCode =
                    $@"using UnityEngine;
using System.Collections.Generic;
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

                using (var sw = File.CreateText(rowCsPath))
                {
                    sw.Write(rowCode);
                }
                using (var sr = File.OpenText(rowCsPath))
                {
                }
                using (var sw = File.CreateText(masterCsPath))
                {
                    sw.Write(masterCode);
                }
                using (var sr = File.OpenText(masterCsPath))
                {
                }
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