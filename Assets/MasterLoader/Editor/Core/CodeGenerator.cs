using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using MasterLoaderConfig;
using System.Text.RegularExpressions;

namespace MasterLoader.Core
{
    public class CodeGenerator
    {
        static CodeGenerator() { }

        private static string _WARNING_MESSAGE = $"/*{GetLine()}" +
$"* ---------------------------------------------{GetLine()}" +
$"*this Code is Auto Generated.{GetLine()}" +
$"* All Changes will be nothing when regenerated.{GetLine()}" +
$"* ---------------------------------------------{GetLine()}" +
$"* これは自動生成コードです。{GetLine()}" +
$"* このコードに行った変更は自動生成時に破棄されます。{GetLine()}" +
$"* ---------------------------------------------{GetLine()}" +
$"*/{GetLine()}{GetLine()}";

        private const string _DEFAULT_MASTER_PATH = "Assets/MasterLoader/Master";

        private const string _DEFAULT_GENERATED_ROOT = "Assets/PlugIns/MasterLoader/Generated";

        private static string GetGeneratedRootPath(IConfig config)
        {
            var path = config.WindowConfig.GeneratedRootPath;
            return string.IsNullOrEmpty(path) ? _DEFAULT_GENERATED_ROOT : path;
        }

        public static string GetGeneratedScriptsPath(IConfig config)
            => $"{GetGeneratedRootPath(config)}/Scripts/";

        private static string GetFacadePath(IConfig config)
            => $"{GetGeneratedRootPath(config)}/Scripts/";

        private static string GetPrefabPath(IConfig config)
            => $"{GetGeneratedRootPath(config)}/Prefab";

        private const string _NAMESPACE = "MasterLoader";
        public const string CS = ".cs";
        private const string _TAB = "\t";
        private const string _SPACE = "    ";
        private const string _CRLF = "\r\n";
        private const string _CR = "\r";
        private const string _LF = "\n";

        private static string GetLine()
        {
            var windowConfig = MasterLoader.ConfigData.WindowConfig;
            if (windowConfig.LineType == LineType.LF)
            {
                return _LF;
            }
            else if (windowConfig.LineType == LineType.CR)
            {
                return _CR;
            }
            return _CRLF;
        }

        private static string GetIndent()
        {
            var windowConfig = MasterLoader.ConfigData.WindowConfig;
            if (windowConfig.IndentType == IndentType.TAB)
            {
                return _TAB;
            }
            return _SPACE;
        }

        private static int GetPrime(int value, int length)
        {
            var _value = value;
            while (_value >= length)
            {
                _value -= length;
            }
            return _value;
        }

        public static bool Generate(IConfig config, MasterDataRaw result)
        {
            var masterName = GetPascalCase(result.Name);
            var typeList = result.Type;
            var commentList = result.Comment;
            var parameterList = result.Parameter;
            var valueList = result.ValueList;

            var csPath = GetGeneratedScriptsPath(config);
            CreateDirectoryIfNeed(csPath, csPath);
            CreateDirectoryIfNeed
            (
                AssetDatabase.GetAssetPath(config.WindowConfig.MasterPathFolder),
                _DEFAULT_MASTER_PATH
            );

            var body = string.Empty;
            for (var i = 0; i < typeList.Length; i++)
            {
                body += GetParameterArgumentCode
                (
                    commentList[i],
                    typeList[i],
                    parameterList[i]
                );
            }

            var rawCode = string.Empty;
            rawCode += _WARNING_MESSAGE;

            var parameterCode = string.Empty;

            var nameSpace = GetNameSpaceCode(config.LoadingConfig.NameSpace);

            #region コード作成
            try
            {
                var switchCode = GenerateSwitchCode(parameterList, typeList, out var enumIndexList);

                if (string.IsNullOrEmpty(switchCode))
                {
                    throw new Exception($"MasterLoader Info: MasterLoader supports only 'int', 'float', 'double', 'bool', 'string', 'enum' type.\n check your master sheet's type or value row.");
                }

                parameterCode += GenerateParameterCode(typeList, switchCode);

                rawCode +=
                AddNameSpaceCode
                (
                    nameSpace,
                    GetClassCode(masterName, body)
                );
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            #endregion

            var masterProperty = $"{masterName}List";

            #region ファイル作成
            try
            {
                var setDataCode = GenerateMasterFunctionCode(masterName, masterProperty, valueList, parameterList.Length, parameterCode);
                var masterCode = GenerateMasterCode(masterName, StringStore.MASTER, masterProperty, nameSpace, setDataCode);

                var rawCsPath = $"{csPath}{masterName}{CS}";
                var masterCsPath = $"{csPath}{masterName}{StringStore.MASTER}{CS}";

                using (var sw = new StreamWriter(rawCsPath, false, Encoding.UTF8))
                {
                    sw.Write(rawCode);
                }
                using (var sw = new StreamWriter(masterCsPath, false, Encoding.UTF8))
                {
                    sw.Write(masterCode);
                }

                if (config.LoadingConfig.EnumValueList.Count > 0)
                {
                    var code = string.Empty;
                    if (config.LoadingConfig.EnumValueList.Count > 1)
                    {
                        foreach (var ev in config.LoadingConfig.EnumValueList)
                        {
                            var enumCode = AddNameSpaceCode
                            (
                                nameSpace,
                                GenerateEnumCode(ev),
                                false
                            );
                            var enumCsPath = $"{csPath}{GetPascalCase(ev.Name)}{CS}";
                            using (var sw = new StreamWriter(enumCsPath, false, Encoding.UTF8))
                            {
                                sw.Write(enumCode);
                            }
                        }
                    }
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
            #endregion
        }

        private static string GetCommentCode(string comment)
        {
            var body = string.Empty;
            if (!string.IsNullOrEmpty(comment))
            {
                var comments = comment.Split(GetLine());
                for (var row = 0; row < comments.Length; row++)
                {
                    body += $"{GetBaseIndent(2)}/// {comments[row]}";
                }
                return
                    $"{GetBaseIndent(2)}/// <summary>" +
                    $"{body}" +
                    $"{GetBaseIndent(2)}/// </summary>{GetLine()}";
            }
            return body;
        }

        private static string GetParameterArgumentCode(string comment, string type, string parameter)
        {
            return
                GetLine() +
                $"{GetCommentCode(comment)}" +
                $"{GetIndent()}{GetIndent()}public {GetParameterString(type, parameter)};"; ;
        }

        private static string GetNameSpaceCode(string nameSpace)
        {
            return string.IsNullOrEmpty(nameSpace) ?
                $"namespace {_NAMESPACE}" :
                $"namespace {nameSpace}";
        }

        private static string GetClassCode(string masterName, string body)
        {
            return
                $"{GetBaseIndent(1)}[Serializable]" +
                $"{GetBaseIndent(1)}public class {GetPascalCase(masterName)}" +
                $"{GetBaseIndent(1)}{{" +
                $"{GetBaseIndent(1)}{GetIndent()}{body}" +
                $"{GetBaseIndent(1)}}}";
        }

        private static void CreateDirectoryIfNeed(string targetPath, string defaultPath)
        {
            if (!Directory.Exists(targetPath))
            {
                if (string.IsNullOrEmpty(targetPath))
                {
                    targetPath = _DEFAULT_MASTER_PATH;
                }
                Directory.CreateDirectory(targetPath);
            }
        }

        private static string GetUsingSystemCode()
        {
            return
            $"using System;{GetLine()}";
        }

        private static string AddNameSpaceCode(string nameSpace, string body, bool needUsingSystem = true)
        {
            var code = string.Empty;
            if (needUsingSystem)
            {
                code = GetUsingSystemCode();
            }
            return
            code +
            $"{nameSpace}{GetLine()}" +
            $"{{" +
            $"{body}{GetLine()}"+
            $"}}";
        }

        public static bool GenerateInjector(IConfig config)
        {
            try
            {
                var injectorCode = _WARNING_MESSAGE;
                injectorCode += GenerateFacadeCode
                (
                    config.WindowConfig,
                    config.LoadingConfig,
                    config.MasterBody
                );

                var facadePath = GetFacadePath(config);
                CreateDirectoryIfNeed(facadePath, facadePath);

                var facadeCsPath = $"{facadePath}{StringStore.MASTER_FACADE_NAME}{CS}";
                using (var sw = new StreamWriter(facadeCsPath, false, Encoding.UTF8))
                {
                    sw.Write(injectorCode);
                }
                AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
            return true;
        }

        private static string GetParameterString(string type, string parameter)
        {
            if (type.Equals("enum"))
            {
                return $"{GetPascalCase(parameter)} {GetPascalCase(parameter)}";
            }
            else
            {
                return $"{type} {parameter}";
            }
        }

        private static string GenerateSwitchCode(string[] parameterList, string[] typeList, out List<int> enumIndexList)
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
                        $"{GetBaseIndent(6)}{GetIndent()}var value = data[valueIndex];" +
                        $"{GetBaseIndent(6)}{GetIndent()}{GetInputCode(parameter)}" +
                        $"{GetBaseIndent(6)}}}";
                        break;
                    case "int":
                    case "float":
                    case "double":
                    case "bool":
                        code += GetSwitchCode(type, parameter, parameterIndex);
                        break;
                    case "enum":
                        code += $"{GetBaseIndent(6)}case {parameterIndex}:" +
                        $"{GetBaseIndent(6)}{{" +
                        $"{GetBaseIndent(6)}{GetIndent()}if (!Enum.TryParse<{GetPascalCase(parameter)}>(data[valueIndex], out var value))" +
                        $"{GetBaseIndent(6)}{GetIndent()}{{" +
                        $"{GetBaseIndent(6)}{GetIndent()}{GetIndent()}OutputParseErrorLog(data[valueIndex], \"{type}\");" +
                        $"{GetBaseIndent(6)}{GetIndent()}{GetIndent()}break;" +
                        $"{GetBaseIndent(6)}{GetIndent()}}}" +
                        $"{GetBaseIndent(6)}{GetIndent()}{GetInputCode(parameter)}" +
                        $"{GetBaseIndent(6)}}}";
                        enumIndexList.Add(parameterIndex);
                        break;
                    default:
                        // type 列に enum 型名が直接指定されている場合（例: TreasureType）
                        code += $"{GetBaseIndent(6)}case {parameterIndex}:" +
                        $"{GetBaseIndent(6)}{{" +
                        $"{GetBaseIndent(6)}{GetIndent()}if (!Enum.TryParse<{GetPascalCase(type)}>(data[valueIndex], out var value))" +
                        $"{GetBaseIndent(6)}{GetIndent()}{{" +
                        $"{GetBaseIndent(6)}{GetIndent()}{GetIndent()}OutputParseErrorLog(data[valueIndex], \"{type}\");" +
                        $"{GetBaseIndent(6)}{GetIndent()}{GetIndent()}break;" +
                        $"{GetBaseIndent(6)}{GetIndent()}}}" +
                        $"{GetBaseIndent(6)}{GetIndent()}{GetInputCode(parameter)}" +
                        $"{GetBaseIndent(6)}}}";
                        enumIndexList.Add(parameterIndex);
                        break;
                }
            }
            return code;
        }

        private static string GenerateParameterCode(string[] typeList, string switchCode)
        {
            return
            $"{GetBaseIndent(5)}switch (GetPrime(valueIndex, {typeList.Length}))" +
            $"{GetBaseIndent(5)}{{" +
                                    switchCode +
            $"{GetBaseIndent(5)}}}";
        }

        private static string GenerateEnumCode(EnumValue enumValue)
        {
            var valuesString = string.Empty;
            for (var vIndex = 0; vIndex < enumValue.Values.Length; vIndex++)
            {
                valuesString +=
                $"{GetBaseIndent(2)}{GetPascalCase(enumValue.Values[vIndex])},";
            }
            var code =
            _WARNING_MESSAGE +
            $"{GetBaseIndent(1)}public enum {GetPascalCase(enumValue.Name)}" +
            $"{GetBaseIndent(1)}{{{GetPascalCase(valuesString)}" +
            $"{GetBaseIndent(1)}}}";
            return code;
        }

        private static string GenerateMasterFunctionCode(string masterName, string masterProperty, string[] valueList, int length, string parameterCode)
        {
            return
            $"{GetBaseIndent(3)}var dataList = new List<{GetPascalCase(masterName)}>();" +
            $"{GetBaseIndent(3)}var obj = new {GetPascalCase(masterName)}{{}};" +
            $"{GetBaseIndent(3)}var doneIndex = 0;" +
            $"{GetBaseIndent(3)}for (var valueIndex = 0; valueIndex < {valueList.Length}; valueIndex++)" +
            $"{GetBaseIndent(3)}{{" +
            $"{GetBaseIndent(3)}{GetIndent()}var isDone = false;" +
            $"{GetBaseIndent(3)}{GetIndent()}if (valueIndex == 0 || doneIndex >= {length})" +
            $"{GetBaseIndent(3)}{GetIndent()}{{" +
            $"{GetBaseIndent(3)}{GetIndent()}{GetIndent()}doneIndex = 0;" +
            $"{GetBaseIndent(3)}{GetIndent()}{GetIndent()}obj = new {GetPascalCase(masterName)}{{}};" +
            $"{GetBaseIndent(3)}{GetIndent()}}}" +
            $"{GetBaseIndent(3)}{GetIndent()}for (var parameterIndex = 0; parameterIndex < {length}; parameterIndex++)" +
            $"{GetBaseIndent(3)}{GetIndent()}{{" +
            $"{GetBaseIndent(3)}{GetIndent()}{GetIndent()}if (isDone)" +
            $"{GetBaseIndent(3)}{GetIndent()}{GetIndent()}{{" +
            $"{GetBaseIndent(3)}{GetIndent()}{GetIndent()}{GetIndent()}continue;" +
            $"{GetBaseIndent(3)}{GetIndent()}{GetIndent()}}}" +
            $"{GetBaseIndent(3)}{GetIndent()}{GetIndent()}{parameterCode}" +
            $"{GetBaseIndent(3)}{GetIndent()}}}" +
            $"{GetBaseIndent(3)}{GetIndent()}if (doneIndex == {length})" +
            $"{GetBaseIndent(3)}{GetIndent()}{{" +
            $"{GetBaseIndent(3)}{GetIndent()}{GetIndent()}dataList.Add(obj);" +
            $"{GetBaseIndent(3)}{GetIndent()}}}" +
            $"{GetBaseIndent(3)}}}" +
            $"{GetBaseIndent(3)}{GetLowerPascalCase(masterProperty)} = dataList;";
        }

        private static string GenerateMasterCode(string masterName, string Master, string masterProperty, string nameSpace, string setDataCode)
        {
            return
            _WARNING_MESSAGE +
            $"using UnityEngine;{GetLine()}" +
            $"using System.Collections.Generic;{GetLine()}" +
            $"using System;{GetLine()}{GetLine()}" +
            $"{nameSpace}{GetLine()}" +
            $"{{" +
            $"{GetBaseIndent(1)}public class {GetPascalCase(masterName)}{Master} : ScriptableObject" +
            $"{GetBaseIndent(1)}{{" +
            $"{GetBaseIndent(1)}{GetIndent()}public List<{GetPascalCase(masterName)}> {GetPascalCase(masterProperty)} {{ get {{ return {GetLowerPascalCase(masterProperty)}; }} }}" +
            $"{GetBaseIndent(1)}{GetIndent()}[SerializeField]" +
            $"{GetBaseIndent(1)}{GetIndent()}private List<{GetPascalCase(masterName)}> {GetLowerPascalCase(masterProperty)} = new List<{GetPascalCase(masterName)}>();{GetLine()}" +

            $"{GetBaseIndent(1)}{GetIndent()}public void SetData(string[] data)" +
            $"{GetBaseIndent(1)}{GetIndent()}{{" +
            $"{setDataCode}" +
            $"{GetBaseIndent(1)}{GetIndent()}}}{GetLine()}" +

            $"{GetBaseIndent(1)}{GetIndent()}private int GetPrime(int value, int length)" +
            $"{GetBaseIndent(1)}{GetIndent()}{{" +
            $"{GetBaseIndent(1)}{GetIndent()}{GetIndent()}var _value = value;" +
            $"{GetBaseIndent(1)}{GetIndent()}{GetIndent()}while (_value >= length)" +
            $"{GetBaseIndent(1)}{GetIndent()}{GetIndent()}{{" +
            $"{GetBaseIndent(1)}{GetIndent()}{GetIndent()}{GetIndent()}_value -= length;" +
            $"{GetBaseIndent(1)}{GetIndent()}{GetIndent()}}}" +
            $"{GetBaseIndent(1)}{GetIndent()}{GetIndent()}return _value;" +
            $"{GetBaseIndent(1)}{GetIndent()}}}{GetLine()}" +

            $"{GetBaseIndent(1)}{GetIndent()}private void OutputParseErrorLog(string s, string type)" +
            $"{GetBaseIndent(1)}{GetIndent()}{{" +
            $"{GetBaseIndent(1)}{GetIndent()}{GetIndent()}if (string.IsNullOrEmpty(s))" +
            $"{GetBaseIndent(1)}{GetIndent()}{GetIndent()}{{" +
            $"{GetBaseIndent(1)}{GetIndent()}{GetIndent()}{GetIndent()}return;" +
            $"{GetBaseIndent(1)}{GetIndent()}{GetIndent()}}}" +
            $"{GetBaseIndent(1)}{GetIndent()}{GetIndent()}Debug.LogError(($\"MasterLoaderInfo: could not cast {{s}} to {{type}}.\"));" +
            $"{GetBaseIndent(1)}{GetIndent()}}}" +
            $"{GetBaseIndent(1)}}}" +
            $"{GetLine()}}}";
        }

        private static string GenerateFacadeCode
        (
            IWindowConfig windowConfig,
            ILoadingConfig loadingConfig,
            IMasterBody masterBody
        )
        {
            var namespaceCode = string.Empty;
            var masterListCode = string.Empty;
            var facadeCode = string.Empty;
            var nameSpace = loadingConfig.NameSpace;

            for (var i = 0; i < masterBody.Masters.Length; i++)
            {
                var masterName = masterBody.Masters[i];
                masterListCode +=
                $"{GetBaseIndent(2)}public {GetPascalCase(masterName)}{StringStore.MASTER} {GetPascalCase(masterName)} {{ get {{ return {GetLowerPascalCase(masterName)}; }} }}" +
                $"{GetBaseIndent(2)}[SerializeField]" +
                $"{GetBaseIndent(2)}private {GetPascalCase(masterName)}{StringStore.MASTER} {GetLowerPascalCase(masterName)};";

                var path = $"\"{AssetDatabase.GetAssetPath(windowConfig.MasterPathFolder)}/{GetPascalCase(masterName)}.asset\"";
                facadeCode +=
                $"{GetBaseIndent(2)}{GetIndent()}{GetLowerPascalCase(masterName)} = AssetDatabase.LoadMainAssetAtPath({path}) as {GetPascalCase(masterName)}{StringStore.MASTER};";
            }

            facadeCode +=
                $"{GetBaseIndent(2)}{GetIndent()}Debug.Log($\"MasterLoader Info: Master data has injected completely.\");";

            if (!string.IsNullOrEmpty(nameSpace))
            {
                namespaceCode =
                $"using {nameSpace};{GetLine()}";
            }

            var editorCode =
            $"#if UNITY_EDITOR" +
            $"{GetBaseIndent(2)}public void SetMaster()" +
            $"{GetBaseIndent(2)}{{" +
            facadeCode +
            $"{GetBaseIndent(2)}}}{GetLine()}" +
            $"#endif";
            
            return
            $"using UnityEngine;{GetLine()}" +
            $"#if UNITY_EDITOR{GetLine()}" +
            $"using UnityEditor;{GetLine()}" +
            $"#endif{GetLine()}" +
            $"{namespaceCode}{GetLine()}" +
            $"namespace MasterLoader{GetLine()}" +
            $"{{" +
            $"{GetBaseIndent(1)}public class {StringStore.MASTER_FACADE_NAME} : MonoBehaviour" +
            $"{GetBaseIndent(1)}{{" +
            masterListCode +
            $"{GetLine()}" +
            editorCode +
            $"{GetBaseIndent(1)}}}" +
            $"{GetLine()}}}";
        }

        private static string GetBaseIndent(int num = 4)
        {
            var value = GetLine();
            for(var i = 0; i < num; i++)
            {
                value += GetIndent();
            }
            return value;
        }

        private static string GetInputCode(string parameter)
        {
            return
            $"{GetBaseIndent(7)}obj.{GetPascalCase(parameter)} = value;" +
            $"{GetBaseIndent(7)}isDone = true;" +
            $"{GetBaseIndent(7)}doneIndex++;" +
            $"{GetBaseIndent(7)}continue;";
        }

        private static string GetSwitchCode(string type, string parameter, int parameterIndex)
        {
            return
            $"{GetBaseIndent(6)}case {parameterIndex}:" +
            $"{GetBaseIndent(6)}{{" +
            $"{GetBaseIndent(6)}{GetIndent()}if (!{type}.TryParse(data[valueIndex], out var value))" +
            $"{GetBaseIndent(6)}{GetIndent()}{{" +
            $"{GetBaseIndent(6)}{GetIndent()}{GetIndent()}OutputParseErrorLog(data[valueIndex], \"{type}\");" +
            $"{GetBaseIndent(6)}{GetIndent()}{GetIndent()}break;" +
            $"{GetBaseIndent(6)}{GetIndent()}}}" +
            $"{GetBaseIndent(6)}{GetIndent()}{GetInputCode(parameter)}" +
            $"{GetBaseIndent(6)}}}";
        }

        public static string ToPascalCase(string body) => GetPascalCase(body);

        private static string GetPascalCase(string body)
        {
            return Regex.Replace
            (
                body,
                @"\b[a-z]",
                match => match.Value.ToUpper()
            );
        }

        private static string GetLowerPascalCase(string body)
        {
            body = GetPascalCase(body);
            return $"_{body.Replace(body[0], char.ToLower(body[0]))}";
        }
    }
}