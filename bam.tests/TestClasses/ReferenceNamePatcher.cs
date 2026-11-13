using System.Text;

namespace Bam.Tests.TestClasses
{
    /// <summary>
    /// Rewrites an assembly reference's simple name in a compiled image, to produce names no compiler emits, such
    /// as a path (bamtk#111). Compile against a dependency named <see cref="PlaceholderFor"/>, then
    /// <see cref="Rename"/> that name to the one wanted: the two have the same UTF-8 length, so the string heap
    /// keeps its layout.
    /// </summary>
    public static class ReferenceNamePatcher
    {
        /// <summary>Gets an ordinary assembly name, starting with <paramref name="seed"/>, as long as <paramref name="replacement"/> in UTF-8.</summary>
        /// <param name="seed">Letters and digits to start the name with, unique to the test.</param>
        /// <param name="replacement">The name the placeholder will be renamed to.</param>
        /// <returns>The placeholder name.</returns>
        public static string PlaceholderFor(string seed, string replacement)
        {
            int length = Encoding.UTF8.GetByteCount(replacement);
            return ("Q" + seed + new string('q', length)).Substring(0, length);
        }

        /// <summary>
        /// Replaces <paramref name="placeholder"/> in <paramref name="image"/> with <paramref name="replacement"/>.
        /// The placeholder must appear exactly once, as a null-terminated string (the reference's name).
        /// </summary>
        /// <param name="image">The compiled assembly; changed in place.</param>
        /// <param name="placeholder">The name the image was compiled against.</param>
        /// <param name="replacement">The name to give the reference, of the same UTF-8 length.</param>
        /// <returns><paramref name="image"/>, for chaining.</returns>
        public static byte[] Rename(byte[] image, string placeholder, string replacement)
        {
            byte[] needle = Encoding.UTF8.GetBytes(placeholder + "\0");
            byte[] replacementBytes = Encoding.UTF8.GetBytes(replacement);
            if (replacementBytes.Length != needle.Length - 1)
            {
                throw new ArgumentException("The replacement must be as long as the placeholder in UTF-8.", nameof(replacement));
            }

            int at = IndexOf(image, needle, 0);
            if (at < 0 || IndexOf(image, needle, at + 1) >= 0)
            {
                throw new InvalidOperationException($"Expected '{placeholder}' exactly once in the image.");
            }

            Array.Copy(replacementBytes, 0, image, at, replacementBytes.Length);
            return image;
        }

        private static int IndexOf(byte[] haystack, byte[] needle, int start)
        {
            for (int i = start; i <= haystack.Length - needle.Length; i++)
            {
                int j = 0;
                while (j < needle.Length && haystack[i + j] == needle[j])
                {
                    j++;
                }

                if (j == needle.Length)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
