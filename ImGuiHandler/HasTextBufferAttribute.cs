using System;

namespace ImGuiHandler
{
    /// <summary>
    /// Designates that the targeted property has a text buffer backing it.  Used for string properties as ImGui.Net
    /// requires a byte array to be passed to ImGui for text inputs
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HasTextBufferAttribute : Attribute
    {
        /// <summary>
        /// Maximum number of bytes the string allows
        /// </summary>
        public int MaxLength { get; set; }

        public HasTextBufferAttribute(int maxLength)
        {
            MaxLength = maxLength;
        }
    }
}