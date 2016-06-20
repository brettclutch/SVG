﻿using System.IO;

namespace Svg.Interfaces
{
    public interface IFileSystem
    {
        bool FileExists(string path);

        bool FolderExists(string path);

        Stream OpenRead(string path);

        Stream OpenWrite(string path);

        string GetFullPath(string path);

        string GetDefaultStoragePath();
        string GetDownloadFolder();

        string PathCombine(params string[] segments);
        void DeleteFile(string storagePath);
    }
}