//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/3/22 10:28:30     V1.0.0       胡 安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.CTS.Helpers;

public static class FileOperationHelper
{
    public static int CopyFolder(string sourceFolder, string destFolder,int fileCount)
    {
        if (!Directory.Exists(sourceFolder))
        {
            throw new DirectoryNotFoundException(sourceFolder);
        }

        //如果目标路径不存在,则创建目标路径
        if (!Directory.Exists(destFolder))
        {
            Directory.CreateDirectory(destFolder);
        }
        //得到原文件根目录下的所有文件
        string[] files = Directory.GetFiles(sourceFolder);
        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            string dest = Path.Combine(destFolder, name);
            fileCount=CopyFile(file, dest, fileCount);//复制文件
        }

        //得到原文件根目录下的所有文件夹
        string[] folders = Directory.GetDirectories(sourceFolder);
        foreach (string folder in folders)
        {
            string name = Path.GetFileName(folder);
            string dest = Path.Combine(destFolder, name);
            fileCount=CopyFolder(folder, dest,fileCount);//构建目标路径,递归复制文件
        }
        return fileCount;
    }

    public static int CopyFile(string sourceFile, string targetDirectory, int fileCount)
    {
        if (!File.Exists(sourceFile))
        {
            throw new FileNotFoundException(sourceFile);
        }

        File.Copy(sourceFile, targetDirectory, true);//复制文件
        fileCount++;
        return fileCount;
    }
}
