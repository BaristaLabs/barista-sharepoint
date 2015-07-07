namespace Barista.Extensions
{
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;

    public static class FileSystemExtensions
    {
        public static IEnumerable<DirectoryInfo> EnumerateDirectories(this DirectoryInfo target)
        {
            return EnumerateDirectories(target, "*");
        }

        public static IEnumerable<DirectoryInfo> EnumerateDirectories(this DirectoryInfo target, string searchPattern)
        {
            var searchPath = Path.Combine(target.FullName, searchPattern);
            NativeWin32.WIN32_FIND_DATA findData;
            using (var hFindFile = NativeWin32.FindFirstFile(searchPath, out findData))
            {
                if (hFindFile.IsInvalid)
                    yield break;

                do
                {
                    if ((findData.dwFileAttributes & FileAttributes.Directory) != 0 && findData.cFileName != "." && findData.cFileName != "..")
                    {
                        yield return new DirectoryInfo(Path.Combine(target.FullName, findData.cFileName));
                    }
                } while (NativeWin32.FindNextFile(hFindFile, out findData));
            }
        }

        public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo target)
        {
            return EnumerateFiles(target, "*");
        }

        public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo target, string searchPattern)
        {
            var searchPath = Path.Combine(target.FullName, searchPattern);
            NativeWin32.WIN32_FIND_DATA findData;
            using (var hFindFile = NativeWin32.FindFirstFile(searchPath, out findData))
            {
                if (hFindFile.IsInvalid)
                    yield break;

                do
                {
                    if ((findData.dwFileAttributes & FileAttributes.Directory) == 0 && findData.cFileName != "." && findData.cFileName != "..")
                    {
                        yield return new FileInfo(Path.Combine(target.FullName, findData.cFileName));
                    }
                } while (NativeWin32.FindNextFile(hFindFile, out findData));
            }
        }

        public static IEnumerable<FileInfo> EnumerateAllFiles(this DirectoryInfo target)
        {
            return EnumerateAllFiles(target, "*");
        }

        /// <summary>
        /// Recursively enumerate all files in all subdirectories in the target directory.
        /// </summary>
        /// <remarks>
        /// This implementation does not keep field handles option during recursive calls or yield operations.
        /// </remarks>
        /// <param name="target"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public static IEnumerable<FileInfo> EnumerateAllFiles(this DirectoryInfo target, string searchPattern)
        {
            var searchPath = Path.Combine(target.FullName, searchPattern);
            NativeWin32.WIN32_FIND_DATA findData;

            var folders = new List<string>();
            var files = new List<string>();

            using (var hFindFile = NativeWin32.FindFirstFile(searchPath, out findData))
            {
                do
                {
                    if (hFindFile.IsInvalid)
                        continue;

                    if (findData.cFileName == "." || findData.cFileName == "..")
                        continue;

                    if (findData.dwFileAttributes.HasFlag(FileAttributes.Directory))
                    {
                        folders.Add(Path.Combine(target.FullName, findData.cFileName));
                        
                    }
                    else if (findData.cFileName != "." && findData.cFileName != "..")
                    {
                        files.Add(Path.Combine(target.FullName, findData.cFileName));
                    }
                        
                } while (NativeWin32.FindNextFile(hFindFile, out findData));
            }

            foreach (var file in folders.SelectMany(folder => EnumerateAllFiles(new DirectoryInfo(folder), searchPattern)))
                yield return file;

            foreach (var file in files)
                yield return new FileInfo(file);
        }
    }
}
