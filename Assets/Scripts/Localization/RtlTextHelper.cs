using RTLTMPro;

// Provides a helper method to force correct RTL text formatting.
public static class RtlTextHelper
{
    // Fixes RTL text and optionally preserves tags and numbers.
    public static string FixForceRTL(string s, bool fixTags = true, bool preserveNumbers = true)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        bool farsi = false;
        var buffer = new FastStringBuilder(RTLSupport.DefaultBufferSize);

        // Apply RTL fixing using the RTLTMPro support utility.
        RTLSupport.FixRTL(s, buffer, farsi, fixTags, preserveNumbers);

        // Reverse the buffer to get the final correct visual order.
        buffer.Reverse();
        return buffer.ToString();
    }
}
