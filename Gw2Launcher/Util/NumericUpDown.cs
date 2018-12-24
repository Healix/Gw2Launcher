namespace Gw2Launcher.Util
{
    static class NumericUpDown
    {
        public static void SetValue(System.Windows.Forms.NumericUpDown n, int value)
        {
            if (value < n.Minimum)
                value = (int)n.Minimum;
            else if (value > n.Maximum)
                value = (int)n.Maximum;

            n.Value = value;
        }
    }
}
