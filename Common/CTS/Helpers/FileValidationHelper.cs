using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace NV.CT.CTS.Helpers;

public static class FileValidationHelper
{
    private static List<string> IgnoreExtList = new List<string>();

    public static void SetMD5CheckIgnoreFileExtension(List<string> exts)
    {
        IgnoreExtList.Clear();
        IgnoreExtList.AddRange(exts);
    }

    public static bool ValidateMD5CheckListAndSaveResult(string dirPath, string checkFile,string resultFile)
    {
        var result = ValidateMD5CheckList(dirPath, checkFile);
        if(result is null)
        {
            return false;
        }

        SaveValidationResult(resultFile,result);
        return true;
    }

    public static ValidationResult ValidateMD5CheckList(string dirPath, string checkFile)
    {
        if (!Directory.Exists(dirPath))
        {
            return null;
        }
        var fileList = ListFilesInDiretory(dirPath);

        return ValidateMD5CheckList(fileList, checkFile, dirPath);
    }

    public static ValidationResult ValidateMD5CheckList(List<string> fileList, string checkFile, string rootPath = "" )
    {
        var validationResult = new ValidationResult();
        var checkList = LoadMD5CheckList(checkFile);
        foreach(string filePath in fileList)
        {
            var relativePath = GetRelativePath(filePath, rootPath);
            var matchItem = checkList.MD5CheckItems.SingleOrDefault(x=>x.RelativePath == relativePath);
            if(matchItem == null)
            {
                validationResult.AdditionalItems.Add(filePath);
            }
            else
            {
                var newMD5CheckItem = MD5Check(filePath, rootPath);
                if(matchItem.MD5Hash ==  newMD5CheckItem.MD5Hash)
                {
                    validationResult.MatchItems.Add(filePath);
                }
                else
                {
                    validationResult.ModifiedItems.Add(filePath);
                }

                checkList.MD5CheckItems.Remove(matchItem);
            }
        }

        validationResult.MissingItems.AddRange(checkList.MD5CheckItems.Select(x=>x.RelativePath).ToList());
        validationResult.ValidationStatus = validationResult.MissingItems.Count == 0
                                            && validationResult.AdditionalItems.Count == 0
                                            && validationResult.ModifiedItems.Count == 0;
        return validationResult;
    }

    public static bool GenerateMD5CheckResult(string dirPath, string resultFilePath)
    {
        if(!Directory.Exists(dirPath))
        {
            return false;
        }
        var fileList = ListFilesInDiretory(dirPath);

        return GenerateMD5CheckResult(fileList, resultFilePath, dirPath);
    }

    public static bool GenerateMD5CheckResult(List<string> fileList,string resultFilePath, string rootPath)
    {
        MD5CheckList resultList = new MD5CheckList();
        foreach (var path in fileList)
        {
            resultList.MD5CheckItems.Add(MD5Check(path, rootPath));
        }

        SaveMD5CheckListResult(resultFilePath, resultList);
        return true;
    }

    private static MD5CheckItem MD5Check(string path,string rootPath)
    {
        if(!File.Exists(path)) {                
            return null;
        }

        using (FileStream fs = File.OpenRead(path))
        {
            using (var crypto = MD5.Create())
            {                    
                var md5Hash = crypto.ComputeHash(fs);
                var file = new FileInfo(path);
                return new MD5CheckItem
                {
                    Name = file.Name,
                    RelativePath = GetRelativePath(file.FullName,rootPath),
                    MD5Hash = BitConverter.ToString(md5Hash)
                };
            }
        }
    }

    private static string GetRelativePath(string fullPath,string rootPath)
    {
        return fullPath.Replace(rootPath, "");
    }
        

    private static List<string> ListFilesInDiretory(string path,int iterLevel = -1)
    {
        List<string> result = new List<string>();
        ListDirectory(path, iterLevel, result);
        return result;
    }

    private static void ListDirectory(string path,int iterLevel,List<string> pathList,int currentLevel = 0)
    {
        if(iterLevel > -1 && iterLevel < currentLevel)
        {
            return;
        }

        DirectoryInfo folder = new DirectoryInfo(path);

        foreach(FileInfo nextFile in folder.GetFiles())
        {
            if(IgnoreExtList.Contains(nextFile.Extension)) 
                continue;
            pathList.Add(nextFile.FullName);
        }

        foreach(DirectoryInfo nextFolder in folder.GetDirectories())
        {
            ListDirectory(nextFolder.FullName, iterLevel, pathList,currentLevel + 1);
        }
    }

    private static void SaveMD5CheckListResult(string path, MD5CheckList resultList)
    {
        using var fileStream = new FileStream(path, FileMode.Create);
        using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
        var serializer = new XmlSerializer(typeof(MD5CheckList));
        serializer.Serialize(streamWriter, resultList);
        streamWriter.Flush();
        streamWriter.Close();
        fileStream.Close();
    }

    private static void SaveValidationResult(string path,ValidationResult result)
    {
        using var fileStream = new FileStream(path, FileMode.Create);
        using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
        var serializer = new XmlSerializer(typeof(ValidationResult));
        serializer.Serialize(streamWriter, result);
        streamWriter.Flush();
        streamWriter.Close();
        fileStream.Close();

    }

    public static MD5CheckList LoadMD5CheckList(string path)
    {
        using var fileStream = new FileStream(path, FileMode.Open);
        using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
        var serializer = new XmlSerializer(typeof(MD5CheckList));
        var result = serializer.Deserialize(streamReader);
        return result as MD5CheckList;
    }
}


[Serializable]
[XmlRoot("MD5CheckList")]
public class MD5CheckList
{
    [XmlArrayItem("MD5CheckItem")]
    public List<MD5CheckItem> MD5CheckItems { get; set; }

    public MD5CheckList() {
        MD5CheckItems = new List<MD5CheckItem>();
    }
}

[Serializable]
public class MD5CheckItem
{
    public string Name { get; set; }
    public string RelativePath { get; set; }

    public string MD5Hash { get; set; }
}

[Serializable]
[XmlRoot("MD5ValidationResult")]
public class ValidationResult
{

    [XmlElement("ValidationStatus")]
    public bool ValidationStatus { get; set; }

    [XmlArray("MatchItems")]
    public List<string> MatchItems { get; set; }

    [XmlArray("ModifiedItems")]
    public List<string> ModifiedItems { get; set; }

    [XmlArray("MissingItems")]
    public List<string> MissingItems { get; set; }

    [XmlArray("AdditionalItems")]
    public List<string> AdditionalItems { get; set; }

    public ValidationResult()
    {
        MatchItems = new List<string>();
        ModifiedItems = new List<string>();
        MissingItems = new List<string>();
        AdditionalItems = new List<string>();
    }
}