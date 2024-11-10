using ATL;
using System.IO;
using System.Text.RegularExpressions;

namespace mp32desc
{
    internal static class Func
    {
        // == FILE PATH ==
        private static string RemoveLastItemFromPathAndUseForwardSlashAsSeparator(string input)
            => string.Join("/", input.Split(Path.DirectorySeparatorChar)[..^1]);

        // == USER TEMPLATE ==
        private static readonly Regex userFieldInCurlyBracesRegex = new(@"{([^{}]*)}");

        private static string UserTemplate2StringFormatTemplate(string userTemplate)
            => userFieldInCurlyBracesRegex.Replace(userTemplate, m => UserTemplateField2StringFormatField(m.Groups[1].Value));

        private static string UserTemplateField2StringFormatField(string userField) => userField switch {
            // Numbers are in order of arguments in `TrackAndStringFormatTemplate2String`'s String.Format
            // userTemplate "{title}" ⇒ userFieldInCurlyBracesRegex match.Groups[1].Value "title" ⇒ this "{0}" ⇒ `TrackAndStringFormatTemplate2String` String.Format $"{Track.Title}"
            "title" => "{0}",
            "artist" => "{1}",
            "album" => "{2}",
            "year" => "{3}",
            "trackNumber" => "{4}",
            "discNumber" => "{5}",
            "genre" => "{6}",
            "comment" => "{7}",
            "duration" => "{8}",
            "bitrate" => "{9}",
            _ => $"{{{{{userField}}}}}", // Put in second pair of braces so it does not crash the String.Format
        };

        private static string TrackAndStringFormatTemplate2String(Track t, string stringFormatTemplate)
            => string.Format(stringFormatTemplate, t.Title, t.Artist, t.Album, t.Year, t.TrackNumber, t.DiscNumber, t.Genre, t.Comment, t.Duration, t.Bitrate);

        // == IMPORTING FILES FROM DIRECTORY ==
        private static readonly string[] ALLOWED_FILE_TYPES = [".mp3", ".m4a", ".flac", ".wav"];
        private static readonly int N_ALLOWED_FILE_TYPES = ALLOWED_FILE_TYPES.Length;
        public record Folder2AudioFilesCollectionCrate(string Text, List<Track> Collection);

        /// <param name="selectedFolderName"></param>
        /// <param name="progress">Used to report progress back to the UI</param>
        /// <returns>Text with info about imported files and Collection of tracks from directory `selectedFolderName` and its subdirectories</returns>
        public static Folder2AudioFilesCollectionCrate Folder2AudioFilesCollection(string selectedFolderName, IProgress<Tuple<int, int>> progress)
        {
            IEnumerable<string> enumeratedFiles = Directory.EnumerateFiles(selectedFolderName, "*", SearchOption.AllDirectories);

            // If user is not authorized to view said (sub)directory, inform them in the UI and abort
            try {
                _ = enumeratedFiles.Count();
            }
            catch (UnauthorizedAccessException e) {
                return new Folder2AudioFilesCollectionCrate(e.Message, []);
            }

            if (enumeratedFiles is not null && enumeratedFiles.Any()) {
                // For reporting progress in UI:
                int nProcessedFiles = 0;
                int nTotalFiles = enumeratedFiles.Count(); // (also used in resultText)
                // For resultText:
                int[] nOfFileTypes = new int[N_ALLOWED_FILE_TYPES];
                int nAudioFiles = 0;
                // Fill the collection with files that have allowed extension (called "supported aduio files" in UI)
                List<Track> resultCollection = [];
                foreach (string filePath in enumeratedFiles) {
                    for (int fileTypeIndex = 0; fileTypeIndex < N_ALLOWED_FILE_TYPES; fileTypeIndex++) {
                        if (filePath.EndsWith(ALLOWED_FILE_TYPES[fileTypeIndex])) {
                            resultCollection.Add(new(filePath));
                            nOfFileTypes[fileTypeIndex]++;
                            nAudioFiles++;
                            break;
                        }
                    }
                    progress.Report(new(++nProcessedFiles, nTotalFiles));
                }
                // Fill the string with info about imported files (total files found, how many foreach supported extension, how many un/supported)
                string resultText = $"Found {nTotalFiles} files:";
                for (int fileTypeIndex = 0; fileTypeIndex < N_ALLOWED_FILE_TYPES; fileTypeIndex++) {
                    resultText += $"\n{nOfFileTypes[fileTypeIndex]} × {ALLOWED_FILE_TYPES[fileTypeIndex]}";
                }
                resultText += $"\n\n{nAudioFiles} × supported audio files\n{nTotalFiles - nAudioFiles} × other";

                return new Folder2AudioFilesCollectionCrate(resultText, resultCollection);
            }
            else return new Folder2AudioFilesCollectionCrate("Nothing found.", []); // If the directory is empty, inform user in the UI and abort
        }

        // == GENERATING DESCRIPTION ==
        /// <param name="audioFiles"></param>
        /// <param name="userTemplate"></param>
        /// <param name="isSubdirectoriesPrintEnabled">"Include names of subdirectories" option (inclSubdirs for short)</param>
        /// <param name="isTimestampPrefixEnabled">"Prefix with YouTube timestamps" option (inclTimestamps for short)</param>
        /// <returns>String with description for tracks in `audioFiles` according to `userTemplate`</returns>
        public static string TrackCollectionAndStringFormatTemplate2String(List<Track> audioFiles, string userTemplate, bool isSubdirectoriesPrintEnabled, bool isTimestampPrefixEnabled)
        {
            string result = ""; // This will be returned
            string stringFormatTemplate = UserTemplate2StringFormatTemplate(userTemplate); // Convert userTemplate once to a more suitable format that can be used in String.format
            string tempDirectoryPath, currentDirectoryPath = ""; // Used for inclSubdirs option
            TimestampCounter timestampCounter = new(); // Used for inclTimestamps option

            foreach (var audioFile in audioFiles) {
                // inclSubdirs: Check path of every audio file – if it differs from the last one, print new subdirectory name above
                if (isSubdirectoriesPrintEnabled) {
                    tempDirectoryPath = RemoveLastItemFromPathAndUseForwardSlashAsSeparator(audioFile.Path);
                    if (tempDirectoryPath != currentDirectoryPath) {
                        currentDirectoryPath = tempDirectoryPath;
                        result += $"\n{currentDirectoryPath}\n\n";
                    }
                }
                // inclTimestamps: Put timestamp on beginning of every line with audio file info
                if (isTimestampPrefixEnabled) {
                    result += timestampCounter + " ";
                    timestampCounter.Increment(audioFile.DurationMs);
                }
                // Line with audio file info
                result += TrackAndStringFormatTemplate2String(audioFile, stringFormatTemplate) + "\n";
            }
            // Throw away first '\n' generated by inclSubdirs logic if necessary
            return (isSubdirectoriesPrintEnabled) ? result[1..] : result;
        }
    }
}
