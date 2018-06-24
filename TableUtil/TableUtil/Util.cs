using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableUtil
{
    public static class Util
    {
        public static StringBuilder Tabs(this StringBuilder builder, int tab)
        {
            for (int i = 0; i < tab; i++)
            {
                builder.Append('\t');
            }
            return builder;
        }

        public static StringBuilder Space(this StringBuilder builder, int space)
        {
            for(int i = 0; i < space; i++)
            {
                builder.Append(' ');
            }
            return builder;
        }
    }
}
