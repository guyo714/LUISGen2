using System;
namespace LUISGen2
{
    public class Indent
    {
        private int spaceCount;
        static private int defaultTabCount;

        public static string operator +(string prefix, Indent indent)
        {
            string space = indent.whitespace;
            return $"{prefix}{space}";
        }

        public static string operator +(Indent indent, string suffix)
        {
            string space = indent.whitespace;
            return $"{space}{suffix}";
        }

        public static Indent operator+(Indent indent, int increment)
        {
            indent.spaceCount += increment;
            return indent;
        }

        public static Indent operator -(Indent indent, int increment)
        {
            indent.spaceCount -= increment;
            return indent;
        }

        public static Indent operator ++(Indent indent)
        {
            indent.spaceCount += Indent.defaultTabCount;
            return indent;
        }

        public static Indent operator --(Indent indent)
        {
            indent.spaceCount -= Indent.defaultTabCount;
            if (indent.spaceCount < 0)
            {
                indent.spaceCount = 0;
            }
            return indent;
        }

        public string whitespace => new string(' ', spaceCount);

        public void reset()
        {
            spaceCount = 0;
        }

        public void test()
        {
            Indent indent = Indent.Instance;

            indent++;
            string x = "1" + indent;
            string y = indent + "1";
            string space0 = "0" + indent + "0";
            indent++;
            string space8 = "8" + indent + "8";
            indent--;
            string space4 = "4" + indent + "4";
            string spaceX = "4" + ++indent + "4";
            string spaceX1 = "4" + indent++ + "4";

        }

        public Indent()
        {
            spaceCount = 0;
            defaultTabCount = 4;
        }

        public static Indent Instance { get; } = new Indent();

    }
}
