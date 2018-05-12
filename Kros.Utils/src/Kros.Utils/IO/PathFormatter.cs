﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kros.IO
{
    /// <summary>
    /// Helper for formatting file paths. It takes care of maximum path length and of creating the path, which
    /// does not yet exists on disk.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The class is useful for example in exports, when there are multiple files generated by export.
    /// The user of the application sets just output folder and file names are generated automatically based on
    /// exported data. The class ensures, that the generated paths are valid (do not contain invalid characters)
    /// and that they are not too long. It also checks the existence of files. If there already exists a file,
    /// counter is added to generated path.
    /// </para>
    /// <para>It is even possible to add own string as an additional information to generated paths.</para>
    /// <para>Static property <see cref="Default"/> is created for simple direct use of <c>PathFormatter</c>.</para>
    /// </remarks>
    public class PathFormatter
    {
        /// <summary>
        /// Default instance for simpler direct use.
        /// </summary>
        public static PathFormatter Default { get; } = new PathFormatter();

        #region Fields

        private string _infoOpeningString = "(";
        private string _infoClosingString = ")";
        private string _counterOpeningString = "(";
        private string _counterClosingString = ")";

        #endregion

        #region Path formatting

        /// <summary>
        /// Formats date for use in file/folder names. Default format is <c>yyyy_MM_dd</c>.
        /// </summary>
        /// <param name="value">Date to format.</param>
        /// <remarks>Minimum (<see cref="DateTime.MinValue"/>) and maximum (<see cref="DateTime.MaxValue"/>) values
        /// are not formatted, but empty string is returned.</remarks>
        protected virtual string FormatDateForPath(DateTime value)
        {
            if ((value == DateTime.MinValue) || (value == DateTime.MaxValue))
            {
                return string.Empty;
            }

            return value.ToString("yyyy_MM_dd");
        }

        /// <summary>
        /// Formats season <paramref name="from"/> - <paramref name="to"/> for use in file/folder names.
        /// </summary>
        /// <param name="from">Start of the season.</param>
        /// <param name="to">End of the season.</param>
        /// <remarks>
        /// If any value is minimum or maximum date (<see cref="DateTime.MinValue"/>, <see cref="DateTime.MaxValue"/>),
        /// the value is not used. For the formatting itself is used the method <see cref="FormatDateForPath(DateTime)"/>.
        /// </remarks>
        public virtual string FormatSeasonForPath(DateTime from, DateTime to)
        {
            var sb = new StringBuilder(30);

            if ((from != DateTime.MinValue) && (from != DateTime.MaxValue))
            {
                sb.Append(FormatDateForPath(from));
            }
            if ((to != DateTime.MinValue) && (to != DateTime.MaxValue))
            {
                if (sb.Length > 0)
                {
                    sb.Append(" - ");
                }
                sb.Append(FormatDateForPath(to));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats folder <paramref name="folder"/> and file name <paramref name="fileName"/> to the resulting path.
        /// It does not check if the file already exist.
        /// </summary>
        /// <param name="folder">Folder path.</param>
        /// <param name="fileName">File name.</param>
        /// <remarks>If resulting path is too long, the file name is shortened to make the path correct.</remarks>
        public string FormatPath(string folder, string fileName)
        {
            return FormatPathCore(folder, fileName, null, false);
        }

        /// <summary>
        /// Formats folder <paramref name="folder"/> and file name <paramref name="fileName"/> to the resulting path and
        /// ensures, that the file does not exist. The counter is added to file name if the path exists.
        /// </summary>
        /// <param name="folder">Folder path.</param>
        /// <param name="fileName">File name.</param>
        /// <remarks>If file already exists at generated path, such counter is added to the file name (<c>(1)</c>, <c>(2)</c>...),
        /// to make the path to non-existing file. If resulting path is too long, file name is shortened to make it valid.
        /// Shortened is original <paramref name="fileName"/> - if counter was added, it is preserved.</remarks>
        public string FormatNewPath(string folder, string fileName)
        {
            return FormatPathCore(folder, fileName, null, true);
        }

        /// <summary>
        /// Formats folder <paramref name="folder"/> and file name <paramref name="fileName"/> to the resulting path.
        /// Additional info <paramref name="info"/> is added to the file name. It does not check if the file already exist.
        /// </summary>
        /// <param name="folder">Folder path.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="info">Additional info added to the file name.</param>
        /// <remarks>
        /// <para>
        /// Additional info <paramref name="info"/> is added to the file name enclosed between <see cref="InfoOpeningString"/>
        /// and <see cref="InfoClosingString"/>. So if <paramref name="fileName"/> is <c>exported.xml</c> and
        /// <paramref name="info"/> is <c>Lorem ipsum</c>, resulting file name is <c>exported (Lorem ipsum).xml</c>.
        /// </para>
        /// <para>
        /// If resulting path is too long, the file name is shortened to make the path correct. Shortened is the original
        /// <paramref name="fileName"/>, additional info <paramref name="info"/> is preserved.
        /// </para>
        /// </remarks>
        public string FormatPath(string folder, string fileName, string info)
        {
            return FormatPathCore(folder, fileName, info, false);
        }

        /// <summary>
        /// Formats folder <paramref name="folder"/> and file name <paramref name="fileName"/> to the resulting path.
        /// Additional info <paramref name="info"/> is added to the file name and it is ensured, that the file does not exist.
        /// The counter is added to file name if the path exists.
        /// </summary>
        /// <param name="folder">Folder path.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="info">Additional info added to the file name.</param>
        /// <remarks>
        /// <para>
        /// Additional info <paramref name="info"/> is added to the file name enclosed between <see cref="InfoOpeningString"/>
        /// and <see cref="InfoClosingString"/>. So if <paramref name="fileName"/> is <c>exported.xml</c> and
        /// <paramref name="info"/> is <c>Lorem ipsum</c>, resulting file name is <c>exported (Lorem ipsum).xml</c>.
        /// If file already exists at generated path, such counter is added to the file name (<c>(1)</c>, <c>(2)</c>...),
        /// to make the path to non-existing <see langword="false"/>, so final file name may be
        /// <c>exported (Lorem ipsum) (1).xml</c>.
        /// </para>
        /// <para>
        /// If resulting path is too long, the file name is shortened to make the path correct. Shortened is the original
        /// <paramref name="fileName"/>, additional info <paramref name="info"/> and possible counter are preserved.
        /// </para>
        /// </remarks>
        public string FormatNewPath(string folder, string fileName, string info)
        {
            return FormatPathCore(folder, fileName, info, true);
        }

        /// <summary>
        /// Creates a list of paths to files. The paths are created with base folder <paramref name="baseFolder"/>,
        /// base file name <paramref name="baseFileName"/> and additional information for individual files
        /// <paramref name="fileInfos"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in additional info dictionary <paramref name="fileInfos"/>. The same keys
        /// are in the returned dictionary with generated file paths.</typeparam>
        /// <param name="baseFolder">Base folder for generated file paths.</param>
        /// <param name="baseFileName">Base file name. All paths are created with this file name with additional info added.
        /// </param>
        /// <param name="fileInfos">Additional informations added to individual file names.</param>
        /// <returns>Dictionary with the same keys as int <paramref name="fileInfos"/>, where for each key a file path
        /// is generated.</returns>
        /// <remarks>
        /// <para>For every key and value in <paramref name="fileInfos"/> si generated a file path in resultng dictionary.
        /// File paths are created using <see cref="FormatPath(string, string, string)"/>, meaning that file name is
        /// created as <paramref name="baseFileName"/> with additional information added from <paramref name="fileInfos"/>
        /// and joined with <paramref name="baseFolder"/>.</para>
        /// <para>If needed, resulting paths are shortened to be valid. All of the resulting paths are shortened the same way
        /// and in such a way that the longest of them must be valid. Base file name <paramref name="baseFileName"/> is
        /// shortened, additional informations are preserved.</para>
        /// <para>Resulting paths are checked if they already exists, and if so, a counter is added to the filename.
        /// So returned generated paths do not exist on file system (at least at the time of their generation).</para>
        /// </remarks>
        /// <example>
        /// <para>If <paramref name="baseFolder"/> is <c>C:\lorem\ipsum</c>, <paramref name="baseFileName"/>
        /// is <c>filename.xml</c> and values in <paramref name="fileInfos"/> are:</para>
        /// <list type="table">
        /// <listheader><term>key</term><description>additional file name info</description></listheader>
        /// <item><term>1</term><description>some info 1</description></item>
        /// <item><term>2</term><description>some info 2</description></item>
        /// <item><term>3</term><description>some info 3</description></item>
        /// </list>
        /// <para>Created list of paths is:</para>
        /// <list type="table">
        /// <listheader><term>key</term><description>path</description></listheader>
        /// <item><term>1</term><description>C:\lorem\ipsum\filename (some info 1).xml</description></item>
        /// <item><term>2</term><description>C:\lorem\ipsum\filename (some info 2).xml</description></item>
        /// <item><term>3</term><description>C:\lorem\ipsum\filename (some info 3).xml</description></item>
        /// </list>
        /// </example>
        public Dictionary<TKey, string> FormatPaths<TKey>(
            string baseFolder,
            string baseFileName,
            Dictionary<TKey, string> fileInfos)
        {
            return FormatPathsCore(baseFolder, baseFileName, false, null, fileInfos);
        }

        /// <summary>
        /// Creates a list of paths to files. The paths are created with base folder <paramref name="baseFolder"/>,
        /// to which a subfolder is added with the same name as value of <paramref name="baseFileName"/> without extension.
        /// File names are created as value of <paramref name="baseFileName"/> and additional information from
        /// <paramref name="fileInfos"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in additional info dictionary <paramref name="fileInfos"/>. The same keys
        /// are in the returned dictionary with generated file paths.</typeparam>
        /// <param name="baseFolder">Base folder for generated file paths. Another subfolder is added with the same name
        /// as the file name <paramref name="baseFileName"/> without theextension.</param>
        /// <param name="baseFileName">Base file name. All paths are created with this file name with additional info added.
        /// At the same time this value (without the extension) is used as a subfolder name in <paramref name="baseFolder"/>.
        /// </param>
        /// <param name="fileInfos">Additional informations added to individual file names.</param>
        /// <returns>Dictionary with the same keys as int <paramref name="fileInfos"/>, where for each key a file path
        /// is generated.</returns>
        /// <remarks>
        /// <para>For every key and value in <paramref name="fileInfos"/> si generated a file path in resultng dictionary.
        /// At first, base path <paramref name="baseFolder"/> is extended with subfolder with name as file name in
        /// <paramref name="baseFileName"/> (without extension). File paths are then created using
        /// <see cref="FormatPath(string, string, string)"/>, meaning that file name is created as
        /// <paramref name="baseFileName"/> with additional information added from <paramref name="fileInfos"/>.
        /// </para>
        /// <para>If needed, resulting paths are shortened to be valid. All of the resulting paths are shortened the same way
        /// and in such a way that the longest of them must be valid. The value of base file name <paramref name="baseFileName"/>
        /// is shortened before it is added as a subfolder to <paramref name="baseFolder"/>. So when shortening occurs,
        /// the subfolder and file names are shortened. Additional informations are preserved.</para>
        /// <para>Resulting paths are checked if they already exists, and if so, a counter is added to the filename.
        /// So returned generated paths do not exist on file system (at least at the time of their generation).</para>
        /// </remarks>
        /// <example>
        /// <para>If <paramref name="baseFolder"/> is <c>C:\lorem\ipsum</c>, <paramref name="baseFileName"/>
        /// is <c>filename.xml</c> and values in <paramref name="fileInfos"/> are:</para>
        /// <list type="table">
        /// <listheader><term>key</term><description>additional file name info</description></listheader>
        /// <item><term>1</term><description>some info 1</description></item>
        /// <item><term>2</term><description>some info 2</description></item>
        /// <item><term>3</term><description>some info 3</description></item>
        /// </list>
        /// <para>Created list of paths is:</para>
        /// <list type="table">
        /// <listheader><term>key</term><description>path</description></listheader>
        /// <item><term>1</term><description>C:\lorem\ipsum\filename\filename (some info 1).xml</description></item>
        /// <item><term>2</term><description>C:\lorem\ipsum\filename\filename (some info 2).xml</description></item>
        /// <item><term>3</term><description>C:\lorem\ipsum\filename\filename (some info 3).xml</description></item>
        /// </list>
        /// </example>
        public Dictionary<TKey, string> FormatPathsInSubfolder<TKey>(
            string baseFolder,
            string baseFileName,
            Dictionary<TKey, string> fileInfos)
        {
            return FormatPathsCore(baseFolder, baseFileName, true, null, fileInfos);
        }

        /// <summary>
        /// Creates a list of paths to files. The paths are created with base folder <paramref name="baseFolder"/>,
        /// to which a subfolder is added. Subfolder name is the same name as value of <paramref name="baseFileName"/>
        /// without extension and <paramref name="subfolderInfo"/> is added to it. File names are created as value of
        /// <paramref name="baseFileName"/> and additional information from <paramref name="fileInfos"/>.
        /// </summary>
        /// <typeparam name="TKey">Type of the key in additional info dictionary <paramref name="fileInfos"/>. The same keys
        /// are in the returned dictionary with generated file paths.</typeparam>
        /// <param name="baseFolder">Base folder for generated file paths. Another subfolder is added with the same name
        /// as the file name <paramref name="baseFileName"/> without theextension.</param>
        /// <param name="baseFileName">Base file name. All paths are created with this file name with additional info added.
        /// At the same time this value (without the extension) is used as a subfolder name in <paramref name="baseFolder"/>.
        /// </param>
        /// <param name="subfolderInfo">Additional info added to subfolder name.</param>
        /// <param name="fileInfos">Additional informations added to individual file names.</param>
        /// <returns>Dictionary with the same keys as in <paramref name="fileInfos"/>, where for each key a file path
        /// is generated.</returns>
        /// <remarks>
        /// <para>For every key and value in <paramref name="fileInfos"/> si generated a file path in resultng dictionary.
        /// At first, base path <paramref name="baseFolder"/> is extended with subfolder with name as file name in
        /// <paramref name="baseFileName"/> (without extension) and <paramref name="subfolderInfo"/>.
        /// File paths are then created using <see cref="FormatPath(string, string, string)"/>, meaning that file name is
        /// created as <paramref name="baseFileName"/> with additional information added from <paramref name="fileInfos"/>.
        /// </para>
        /// <para>If needed, resulting paths are shortened to be valid. All of the resulting paths are shortened the same way
        /// and in such a way that the longest of them must be valid. The value of base file name <paramref name="baseFileName"/>
        /// is shortened before it is added as a subfolder to <paramref name="baseFolder"/>. So when shortening occurs,
        /// the subfolder and file names are shortened. Additional informations <paramref name="fileInfos"/> and
        /// <paramref name="subfolderInfo"/> are preserved.</para>
        /// <para>Resulting paths are checked if they already exists, and if so, a counter is added to the filename.
        /// So returned generated paths do not exist on file system (at least at the time of their generation).</para>
        /// </remarks>
        /// <example>
        /// <para>If <paramref name="baseFolder"/> is <c>C:\lorem\ipsum</c>, <paramref name="baseFileName"/>
        /// is <c>filename.xml</c>, <paramref name="subfolderInfo"/> is <c>subfolder info</c> and values in
        /// <paramref name="fileInfos"/> are:</para>
        /// <list type="table">
        /// <listheader><term>key</term><description>additional file name info</description></listheader>
        /// <item><term>1</term><description>some info 1</description></item>
        /// <item><term>2</term><description>some info 2</description></item>
        /// <item><term>3</term><description>some info 3</description></item>
        /// </list>
        /// <para>Created list of paths is:</para>
        /// <list type="table">
        /// <listheader><term>key</term><description>path</description></listheader>
        /// <item>
        /// <term>1</term>
        /// <description>C:\lorem\ipsum\filename (subfolder info)\filename (some info 1).xml</description>
        /// </item>
        /// <item>
        /// <term>2</term>
        /// <description>C:\lorem\ipsum\filename (subfolder info)\filename (some info 2).xml</description>
        /// </item>
        /// <item>
        /// <term>3</term>
        /// <description>C:\lorem\ipsum\filename (subfolder info)\filename (some info 3).xml</description>
        /// </item>
        /// </list>
        /// </example>
        public Dictionary<TKey, string> FormatPathsInSubfolder<TKey>(
            string baseFolder,
            string baseFileName,
            string subfolderInfo,
            Dictionary<TKey, string> fileInfos)
        {
            return FormatPathsCore(baseFolder, baseFileName, true, subfolderInfo, fileInfos);
        }

        #endregion

        #region Settings

        /// <summary>
        /// Opening string for additional information, which is inserted into file/folder name.
        /// Additional information is enclosed between <see cref="InfoOpeningString"/> and <see cref="InfoClosingString"/>.
        /// Default value is left parenthesis <c>(</c>.
        /// </summary>
        /// <remarks>
        /// When set, invalid path characters in the value are replaced using
        /// <see cref="PathHelper.ReplaceInvalidPathChars(string)"/>.
        /// </remarks>
        public string InfoOpeningString
        {
            get {
                return _infoOpeningString;
            }
            set {
                _infoOpeningString = value == null ? string.Empty : PathHelper.ReplaceInvalidPathChars(value);
            }
        }

        /// <summary>
        /// Closing string for additional information, which is inserted into file/folder name.
        /// Additional information is enclosed between <see cref="InfoOpeningString"/> and <see cref="InfoClosingString"/>.
        /// Default value is right parenthesis <c>)</c>.
        /// </summary>
        /// <remarks>
        /// When set, invalid path characters in the value are replaced using
        /// <see cref="PathHelper.ReplaceInvalidPathChars(string)"/>.
        /// </remarks>
        public string InfoClosingString
        {
            get {
                return _infoClosingString;
            }
            set {
                _infoClosingString = value == null ? string.Empty : PathHelper.ReplaceInvalidPathChars(value);
            }
        }

        /// <summary>
        /// Opening string for counter, which is inserted into file/folder name.
        /// The counter is enclosed between <see cref="CounterOpeningString"/> and <see cref="CounterClosingString"/>.
        /// Default value is left parenthesis <c>(</c>.
        /// </summary>
        /// <remarks>
        /// When set, invalid path characters in the value are replaced using
        /// <see cref="PathHelper.ReplaceInvalidPathChars(string)"/>.
        /// </remarks>
        public string CounterOpeningString
        {
            get {
                return _counterOpeningString;
            }
            set {
                _counterOpeningString = value == null ? string.Empty : PathHelper.ReplaceInvalidPathChars(value);
            }
        }

        /// <summary>
        /// Closing string for counter, which is inserted into file/folder name.
        /// The counter is enclosed between <see cref="CounterOpeningString"/> and <see cref="CounterClosingString"/>.
        /// Default value is right parenthesis <c>)</c>.
        /// </summary>
        /// <remarks>
        /// When set, invalid path characters in the value are replaced using
        /// <see cref="PathHelper.ReplaceInvalidPathChars(string)"/>.
        /// </remarks>
        public string CounterClosingString
        {
            get {
                return _counterClosingString;
            }
            set {
                _counterClosingString = value == null ? string.Empty : PathHelper.ReplaceInvalidPathChars(value);
            }
        }

        #endregion

        #region Helpers

        // 260 characters is maximum path length on disk, taken from .NET framework.
        // 12 characters is a reserve for numbers in case the file already exists.
        private int _maxPathLength = 260 - 12;

        /// <summary>
        /// Maximum path length. Intended for internal use.
        /// </summary>
        protected internal int MaxPathLength
        {
            get {
                return _maxPathLength;
            }
            set {
                _maxPathLength = value;
            }
        }

        /// <summary>
        /// Checks if file <paramref name="filePath"/> exists. Method is intended for internal use.
        /// </summary>
        /// <param name="filePath">Path to file.</param>
        /// <returns><see langword="true"/> if file exists, <see langword="false"/> otherwise.</returns>
        public virtual bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Checks if folder <paramref name="folderPath"/> exists. Method is intended for internal use.
        /// </summary>
        /// <param name="folderPath">Path to folder.</param>
        /// <returns><see langword="true"/> if folder exists, <see langword="false"/> otherwise.</returns>
        public virtual bool FolderExists(string folderPath)
        {
            return Directory.Exists(folderPath);
        }

        private string FormatPathCore(string folder, string fileName, string info, bool fileMustNotExist)
        {
            var filePath = FormatPathCore(folder, fileName, info, 0);

            if (fileMustNotExist && FileExists(filePath))
            {
                filePath = FormatNewPathCore(folder, fileName, info);
            }
            return filePath;
        }

        private string FormatPathCore(string folder, string fileName, string info, int counter)
        {
            var ext = Path.GetExtension(fileName);
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var filePath = BuildFilePath(folder, baseName, ext, info, counter);

            if (filePath.Length > MaxPathLength)
            {
                var overflow = filePath.Length - MaxPathLength;
                baseName = baseName.Substring(0, baseName.Length - overflow);
                filePath = BuildFilePath(folder, baseName, ext, info, counter);
            }

            return filePath;
        }

        private string FormatNewPathCore(string folder, string fileName, string info)
        {
            string filePath;
            int counter = 0;
            do
            {
                counter += 1;
                filePath = FormatPathCore(folder, fileName, info, counter);
            } while (FileExists(filePath));

            return filePath;
        }

        private Dictionary<TKey, string> FormatPathsCore<TKey>(
            string baseFolder,
            string baseFileName,
            bool useSubfolder,
            string subfolderInfo,
            Dictionary<TKey, string> fileInfos)
        {
            string ext = Path.GetExtension(baseFileName);
            string baseName = Path.GetFileNameWithoutExtension(baseFileName);
            int overflow = 0;

            string outputFolder = baseFolder;
            if (useSubfolder)
            {
                outputFolder = BuildSubfolderPath(baseFolder, baseName, subfolderInfo);
            }
            foreach (var item in fileInfos)
            {
                string filePath = BuildFilePath(outputFolder, baseName, ext, item.Value, 0);
                overflow = Math.Max(overflow, filePath.Length - MaxPathLength);
            }

            if (overflow > 0)
            {
                if (useSubfolder)
                {
                    // Half of the overflow is cut from folder and half from filename.
                    overflow = (int)Math.Ceiling(overflow / 2.0);
                }
                baseName = baseName.Substring(0, baseName.Length - overflow);
                if (useSubfolder)
                {
                    outputFolder = BuildSubfolderPath(baseFolder, baseName, subfolderInfo);
                }
            }
            if (useSubfolder)
            {
                outputFolder = GetNonExistingOutputFolder(outputFolder);
            }

            var result = new Dictionary<TKey, string>(fileInfos.Count);
            var counters = new Dictionary<string, int>(fileInfos.Count);
            foreach (var item in fileInfos)
            {
                string filePath = BuildFilePath(outputFolder, baseName, ext, item.Value, 0);
                if (counters.ContainsKey(filePath))
                {
                    counters[filePath] += 1;
                    filePath = BuildFilePath(outputFolder, baseName, ext, item.Value, counters[filePath]);
                }
                else
                {
                    counters.Add(filePath, 0);
                }
                result[item.Key] = filePath;
            }

            return result;
        }

        private string BuildFilePath(string folder, string fileName, string extension, string info, int counter)
        {
            var sbFileName = new StringBuilder(fileName);

            if (!string.IsNullOrEmpty(info))
            {
                sbFileName.Append(" ");
                sbFileName.Append(GetInfoString(info));
            }
            if (counter > 0)
            {
                sbFileName.Append(" ");
                sbFileName.Append(GetCounterString(counter));
            }
            sbFileName.Append(extension);
            return PathHelper.BuildPath(folder, sbFileName.ToString());
        }

        private string BuildSubfolderPath(string baseFolder, string subfolder, string info)
        {
            if (!string.IsNullOrEmpty(info))
            {
                subfolder = string.Format("{0} {1}", subfolder, PathHelper.ReplaceInvalidPathChars(info));
            }
            return PathHelper.BuildPath(baseFolder, subfolder);
        }

        private string GetNonExistingOutputFolder(string folder)
        {
            string newFolder = folder;
            int counter = 0;

            while (FolderExists(newFolder))
            {
                counter += 1;
                newFolder = $"{folder} {GetCounterString(counter)}";
            }

            return newFolder;
        }

        private string GetCounterString(int counter)
        {
            return string.Concat(CounterOpeningString, counter, CounterClosingString);
        }

        private string GetInfoString(string info)
        {
            return string.Concat(InfoOpeningString, PathHelper.ReplaceInvalidPathChars(info), InfoClosingString);
        }

        #endregion
    }
}
