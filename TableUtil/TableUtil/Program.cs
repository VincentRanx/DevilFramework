using Microsoft.Office.Interop.Excel;
using org.vr.rts.component;
using org.vr.rts.unity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TableUtil.TableModel;

namespace TableUtil
{
    class Program
    {
        static Application sApp;
        static Workbook sBook;

        static void Main(string[] args)
        {
            Console.ReadKey();
        }

        static bool OpenExcel(string file)
        {
            if (!File.Exists(file))
                return false;
            object miss = Missing.Value;
            if (sApp == null)
            {
                sApp = new Application();
                sApp.Visible = false;
                sApp.UserControl = false;
            }
            if (sBook != null)
                sBook.Close();
            sBook = sApp.Application.Workbooks.Open(file, miss, true, miss, miss);
            return sBook != null;
        }

        static void CloseExcel()
        {
            if (sBook != null)
            {
                sBook.Close();
                sBook = null;
            }
            if (sApp != null)
            {
                sApp.Quit();
                sApp = null;
            }
        }

        public static ClassModel GetModel(Worksheet sheet, int nameRow = 1, int typeRow = 2, int startCol = 1)
        {
            if (sheet == null)
                return null;
            ClassModel model = new ClassModel();

            return null;
        }
        
    }
}
