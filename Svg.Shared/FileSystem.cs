using System;
using System.IO;
using Svg.Interfaces;

namespace Svg
{
    public class FileSystem : IFileSystem
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public Stream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public Stream OpenWrite(string path)
        {
            return new FileStream(path, FileMode.Create, FileAccess.Write);
        }

        public string GetFullPath(string path)
        {
            return path;
        }

        public string GetDefaultStoragePath()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }

        public string GetDownloadFolder()
        {
            return
                Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)
                    .AbsolutePath;
        }

        public string PathCombine(params string[] segments)
        {
            return Path.Combine(segments);
        }

        public void DeleteFile(string storagePath)
        {
            File.Delete(storagePath);
        }
    }
}