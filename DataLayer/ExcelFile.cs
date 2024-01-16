using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
//using Excel = Microsoft.Office.Interop.Excel;

namespace DataLayer
{
    public class ExcelFile : IDisposable
    {
        public bool LoadedExcel { get; private set; } = false;

        private string filepath;

        /*
        private readonly Excel.Application excel = null;
        private readonly Excel.Workbook excelWB;
        private readonly Excel.Worksheet excelWS;
        */

        /*
        */
        private readonly dynamic excel = null;
        private readonly dynamic excelWB = null;
        private readonly dynamic excelWS = null;

        public ExcelFile(string filepath, bool readFile, bool showFile)
        {
            Type typeExcel = Type.GetTypeFromProgID("Excel.Application");

            if (typeExcel is null)
            {
                LoadedExcel = false;
                return;
            }
            excel = Activator.CreateInstance(typeExcel);

            /*
            excel = new Excel.Application();
            */

            //excel.Visible = true;
            
            LoadedExcel = true;

            excel.Visible = showFile;

            this.filepath = filepath;

            if (readFile)
            {
                try { excelWB = excel.Workbooks.Open(filepath); }
                catch { }
            }

            if (excelWB is null) excelWB = excel.Workbooks.Add(Missing.Value);

            if (excelWB.Sheets.Count == 0) excelWS = excelWB.Sheets.Add(Missing.Value);
            else excelWS = excelWB.Sheets[1];
        }

        public static string IntToColumn(int columnNr)
        {
            string columnText = string.Empty;
            while (columnNr > 0)
            {
                int m = (columnNr - 1) % 26;
                columnText = Convert.ToChar('A' + m) + columnText;
                columnNr = (columnNr - m) / 26;
            }
            return columnText;
        }

        public bool WriteCell(string column, int row, string val)
        {
            if (!LoadedExcel) return false;
            excelWS.Cells[row, column] = val;
            return true;
        }

        public T ReadCell<T>(string column, int row)
        {
            if (!LoadedExcel) return default;

            try { return excelWS.Cells[row, column].Value2; }
            catch { return default; }
        }

        public bool Save()
        {
            if (!LoadedExcel) return false;
            string fileDir = filepath.Substring(0, filepath.LastIndexOf('\\'));
            Directory.CreateDirectory(fileDir);

            excelWB.SaveCopyAs(filepath);

            return true;
        }

        public bool Close()
        {
            if (!LoadedExcel || excel.Visible) return false;
            excelWB.Close(false);
            excel.Quit();
            LoadedExcel = false;
            return true;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
