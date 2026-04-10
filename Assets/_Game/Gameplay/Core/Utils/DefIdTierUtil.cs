using System;

namespace SeasonalBastion
{
    public static class DefIdTierUtil
    {
        public static string BaseId(string defId)
        {
            if (string.IsNullOrEmpty(defId)) return defId;

            int idx = defId.LastIndexOf("_t", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return defId;

            int suffixStart = idx + 2;
            if (suffixStart >= defId.Length) return defId;

            for (int i = suffixStart; i < defId.Length; i++)
            {
                char c = defId[i];
                if (c < '0' || c > '9')
                    return defId;
            }

            return defId.Substring(0, idx);
        }

        public static bool IsBase(string defId, string baseId)
        {
            if (string.IsNullOrEmpty(defId) || string.IsNullOrEmpty(baseId)) return false;
            return string.Equals(BaseId(defId), baseId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
