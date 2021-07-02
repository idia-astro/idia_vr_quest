namespace Util
{
    public class FormattingFunctions
    {
        public static string GetPrettyFileSizeString(long fileSizeInBytes)
        {
            if (fileSizeInBytes >= 1e12)
            {
                return $"{(fileSizeInBytes / 1e12):F2} TB";
            }
            else if (fileSizeInBytes >= 1e9)
            {
                return $"{(fileSizeInBytes / 1e9):F2} GB";
            }
            else if (fileSizeInBytes >= 1e6)
            {
                return $"{(fileSizeInBytes / 1e6):F1} MB";
            }
            else if (fileSizeInBytes >= 1e3)
            {
                return $"{(fileSizeInBytes / 1e3):F1} kB";
            }
            else
            {
                return $"{fileSizeInBytes} B";
            }
        }
    }
}