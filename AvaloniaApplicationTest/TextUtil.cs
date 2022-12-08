using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaApplicationTest {
    public class TextUtil {
        public static string gb2312ToUtf8(string text) {
            //声明字符集   
            System.Text.Encoding utf8, gb2312;
            //utf8   
            utf8 = System.Text.Encoding.GetEncoding("utf-8");
            //gb2312   
            gb2312 = System.Text.Encoding.GetEncoding("gb2312");
            byte[] utf;
            utf = gb2312.GetBytes(text);
            utf = System.Text.Encoding.Convert(gb2312, utf8, utf);
            //返回转换后的字符   
            return utf8.GetString(utf);
        }
    }
}
