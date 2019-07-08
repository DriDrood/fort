using System.Text.RegularExpressions;
using Fort.Utils.Logger;

namespace Fort.Utils
{
    public static class Colors
    {
        public static int[] HexColorToInt(string color)
        {
            color = color.ToLower();

            if (!Regex.Match(color, "^#[0-9a-f]{6}$").Success)
                throw new FortException(ELogLevel.UnknownException, "Barva je ve špatném formátu, použijte formát '#fff'");

            int[] result = new int[] { 0, 0, 0 };
            for (int i = 0; i < 6; i++)
            {
                var value = (int)color[i + 1] < 60
                    ? (int)color[i + 1] - 48
                    : (int)color[i + 1] - 87;

                if (i % 2 == 0)
                    value *= 16;

                result[i / 2] += value;
            }

            return result;
        }

        public static int[] Lighter(int[] color, double coef)
        {
            for (int i = 0; i < color.Length; i++)
            {
                double final = color[i] * coef;
                color[i] = final < 256
                    ? (int)final
                    : 255;
            }

            return color;
        }
    }
}