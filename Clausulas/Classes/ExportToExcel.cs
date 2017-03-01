using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

using Excel = Microsoft.Office.Interop.Excel;

namespace Clausulas
{
    /// <summary>
    /// Clase para exportar a excel una lista con datos
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExportToExcel<T>
    {

        #region Declaración de Variables

        public List<T> dataToPrint;
        // Excel object references.
        private Excel.Application excelApp = null;
        private Excel.Workbooks books = null;
        private Excel.Workbook book = null;
        private Excel.Sheets sheets = null;
        private Excel.Worksheet sheet = null;
        private Excel.Range range = null;
        private Excel.Font font = null;
        // Optional argument variable
        private object optionalValue = Missing.Value;

        #endregion

        #region Métodos

        /// <summary>
        /// Generar excel
        /// </summary>
        public void GenerateReport()
        {
            if (dataToPrint == null || dataToPrint.Count == 0)
                return;

            Mouse.SetCursor(Cursors.Wait);
            try
            {
                CreateExcelRef();
                FillSheet();
                OpenReport();
            }
            catch (Exception)
            {
                MessageBox.Show("Error mientras se generaba el fichero excel");
            }
            finally
            {
                ReleaseObject(sheet);
                ReleaseObject(sheets);
                ReleaseObject(book);
                ReleaseObject(books);
                ReleaseObject(excelApp);
            }
            Mouse.SetCursor(Cursors.Arrow);
        }

        /// <summary>
        /// Crea las instancias para la aplicación Excel
        /// </summary>
        private void CreateExcelRef()
        {
            excelApp = new Excel.Application();
            books = excelApp.Workbooks;
            book = books.Add(optionalValue);
            sheets = book.Worksheets;
            sheet = sheets.get_Item(1);
            sheet.Name = "Cuotas";
        }

        /// <summary>
        /// Llena la Hoja Excel
        /// </summary>
        private void FillSheet()
        {
            object[] header = CreateHeader();
            WriteData(header);
        }

        /// <summary>
        /// Crea las cabeceras de las columnas desde las propiedades
        /// </summary>
        /// <returns>Devuelve un objeto con las cabeceras</returns>
        private object[] CreateHeader()
        {
            PropertyInfo[] headerInfo = typeof(T).GetProperties();

            // Create an array for the headers and add it to the
            // worksheet starting at cell A1.
            List<object> objHeaders = new List<object>();
            for (int n = 0; n < headerInfo.Length; n++)
            {
                objHeaders.Add(headerInfo[n].Name);
            }

            var headerToAdd = objHeaders.ToArray();
            AddExcelRows("A1", 1, headerToAdd.Length, headerToAdd);
            SetHeaderStyle();

            // Aplicar el formato a las celdas
            for (int i = 0; i < headerInfo.Length; i++)
            {
                char c = (char)(i + 65);
                range = sheet.get_Range(c + ":" + c, optionalValue);
                if (headerInfo[i].PropertyType == typeof(int))
                {
                    range.NumberFormat = "@";
                    continue;
                }
                if (headerInfo[i].PropertyType == typeof(decimal))
                {
                    range.NumberFormat = "0.00";
                    continue;
                }
                if (headerInfo[i].PropertyType == typeof(float))
                {
                    range.NumberFormat = "0.000";
                    continue;
                }
                if (headerInfo[i].PropertyType == typeof(DateTime))
                {
                    range.NumberFormat = "dd/mm/yyyy";
                    continue;
                }
                range.NumberFormat = "General";
            }
            return headerToAdd;
        }

        /// <summary>
        /// Añade las filas a la hoja excel
        /// </summary>
        /// <param name="startRange"></param>
        /// <param name="rowCount"></param>
        /// <param name="colCount"></param>
        /// <param name="values"></param>
        private void AddExcelRows(string startRange, int rowCount, int colCount, object values)
        {
            range = sheet.get_Range(startRange, optionalValue);
            range = range.get_Resize(rowCount, colCount);
            range.set_Value(optionalValue, values);
        }

        /// <summary>
        /// Establece el estilo de fuente de la cabecera en negrita
        /// </summary>
        private void SetHeaderStyle()
        {
            font = range.Font;
            font.Bold = true;
        }

        /// <summary>
        /// Escribir los datos en la hoja Excel
        /// </summary>
        /// <param name="header"></param>
        private void WriteData(object[] header)
        {
            object[,] objData = new object[dataToPrint.Count, header.Length];

            for (int j = 0; j < dataToPrint.Count; j++)
            {
                var item = dataToPrint[j];
                for (int i = 0; i < header.Length; i++)
                {
                    var y = typeof(T).InvokeMember(header[i].ToString(), BindingFlags.GetProperty, null, item, null);
                    //objData[j, i] = (y == null) ? "" : y.ToString();
                    objData[j, i] = y;
                }
            }
            AddExcelRows("A2", dataToPrint.Count, header.Length, objData);
            AutoFitColumns("A1", dataToPrint.Count + 1, header.Length);
        }

        /// <summary>
        /// Ajustar las columnas de la hoja
        /// </summary>
        /// <param name="startRange"></param>
        /// <param name="rowCount"></param>
        /// <param name="colCount"></param>
        private void AutoFitColumns(string startRange, int rowCount, int colCount)
        {
            range = sheet.get_Range(startRange, optionalValue);
            range = range.get_Resize(rowCount, colCount);
            range.Columns.AutoFit();
        }

        /// <summary>
        /// Hace que Microsoft Excel sea visible
        /// </summary>
        private void OpenReport()
        {
            excelApp.Visible = true;
        }

        /// <summary>
        /// Elimina los recursos de memoria
        /// </summary>
        /// <param name="obj"></param>
        private void ReleaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show(ex.Message.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        #endregion

    }
}
