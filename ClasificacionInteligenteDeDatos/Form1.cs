using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClasificacionInteligenteDeDatos
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            cant_columnas = 0;
            cant_instancias = 0;
        }
        List<string> encabezado;
        List<List<string>> elementos;
        private Dictionary<string, List<string>> instancias;
        private int cant_instancias;
        private int cant_columnas;
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog archivo = new OpenFileDialog
            {
                Filter = "Archivo separado por comas (*.csv)|*.csv",
                Title = "Indica el archivo que deseas abrir"
            };
            if (archivo.ShowDialog().Equals(DialogResult.OK))
            {
                encabezado = new List<string>();
                elementos = new List<List<string>>();
                instancias = new Dictionary<string, List<string>>();
                cargarCsv(archivo.FileName);
                llenarDataGrid();
            }
        }
        private void cargarCsv(string direccionArchivo)
        {
            MessageBox.Show("Se cargara el archivo con la direccion " + direccionArchivo);
            StreamReader leerCsv = new StreamReader(direccionArchivo);
            bool coma = false;
            string comilla = '"'.ToString();
            string aux = "";
            int j = 0;
            if (!leerCsv.EndOfStream)
            {
                foreach (string columna in leerCsv.ReadLine().Split(','))
                {
                    encabezado.Add(columna);
                }

                cant_columnas = encabezado.Count();
                foreach (string columna in encabezado)
                {
                    elementos.Add(new List<string>());
                }

                while (!leerCsv.EndOfStream)
                {
                    string[] instancia = leerCsv.ReadLine().Split(',');
                    for (int i = 0; i < instancia.Length; i++)
                    {
                        if (instancia[i].Contains("No diving") || instancia[i].Contains("Toilet facilities"))
                        {
                            instancia[i] = "";
                        }
                        if (j == 13) j = 0;
                        string aux2 = instancia[i];
                        if (aux2 == "")
                        {
                            elementos.ElementAt(j).Add(aux2);
                            j++;
                        }
                        else
                        {
                            if (aux2[0].ToString() == comilla)
                            {
                                aux += aux2 + ",";
                                coma = true;
                            }
                            else if (coma == false)
                            {
                                elementos.ElementAt(j).Add(aux2);
                                j++;
                            }
                            else if (coma == true)
                            {
                                aux += aux2 + ",";
                                if (aux2[aux2.Length - 1].ToString() == comilla)
                                {
                                    elementos.ElementAt(j).Add(aux);
                                    coma = false;
                                    j++;
                                    aux = "";
                                    if (j == 13) j = 0;
                                }
                            }
                        }
                    }

                }
                cant_instancias = elementos[elementos.Count - 1].Count;
                int x = 0;
                foreach (string columna in encabezado)
                {
                    instancias.Add(columna, elementos[x]);
                    x++;
                }
            }
            else
            {
                MessageBox.Show("Es el final del archivo");
            }
            leerCsv.Close();
        }

        private void llenarDataGrid()
        {
            foreach (string a in encabezado)
            {
                DataGridViewTextBoxColumn columna = new DataGridViewTextBoxColumn { HeaderText = a, Name = a, SortMode = DataGridViewColumnSortMode.NotSortable };
                dataGridView1.Columns.Add(columna);
            }

            int indice = 0;
            int renglon = 0;

            dataGridView1.Rows.Add(cant_instancias);
            DateTime conversion = default(DateTime);
            string fechaConvertida = "";

            while (indice != cant_instancias)
            {
                //renglon = dataGridView1.Rows.Add();
                foreach (string columna in encabezado)
                {
                    //cambia el formato de la fecha en "inspection_date"
                    if (columna == "inspection_date")
                    {
                        string fecha = instancias[columna][indice].Replace('/', '-');
                         conversion = Convert.ToDateTime(fecha, CultureInfo.InvariantCulture.DateTimeFormat);
                         fechaConvertida = conversion.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        dataGridView1.Rows[renglon].Cells[columna].Value = fechaConvertida;
                    }
                    //cambia a cero si "inspection_score" esta en blanco
                    else if(columna=="inspection_score")
                    {
                        if (String.IsNullOrEmpty(instancias[columna][indice]))
                        {
                            dataGridView1.Rows[renglon].Cells[columna].Value = 0;
                        }
                        else
                        {
                            dataGridView1.Rows[renglon].Cells[columna].Value = instancias[columna][indice];
                        }
                    }
                    else
                    {
                        dataGridView1.Rows[renglon].Cells[columna].Value = instancias[columna][indice];
                    }
              
                }
                indice++;
                renglon++;
            }
            
            //Se elimina la fila solo si "business_phone_number","violation_description" y "violation_code" estan vacios
            indice = 0;
            renglon = 0;
            int numFilasVacias = 0;
            while(indice!=cant_instancias)
            {
                if (String.IsNullOrEmpty(dataGridView1.Rows[renglon].Cells["business_phone_number"].Value as String)&&
                    String.IsNullOrEmpty(dataGridView1.Rows[renglon].Cells["violation_description"].Value as String)&&
                    String.IsNullOrEmpty(dataGridView1.Rows[renglon].Cells["violation_code"].Value as String))
                {
                    dataGridView1.Rows.RemoveAt(renglon);
                    numFilasVacias++;
                }
                else
                {
                    renglon++;
                }
                indice++;
            }
            //nuevo valor de cant_instancias
            cant_instancias = cant_instancias - numFilasVacias;
        }
        private void guardarCsv(string direccionArchivo)
        {
            //StreamWriter escribir = new StreamWriter(direccionArchivo);
            System.IO.File.WriteAllText(direccionArchivo, string.Empty);
            StreamWriter escribir = File.AppendText(direccionArchivo);

            int contador = 1;
                foreach (string columna in encabezado)
                {
                    escribir.Write(columna);

                    if (encabezado.Count() > contador)
                    {
                        escribir.Write(',');
                    }
                    else
                    {
                        escribir.WriteLine("");
                    }
                    contador += 1;
                }

            for (int fila = 0; fila < cant_instancias; fila += 1)
            {
                contador = 1;
                foreach (string columna in encabezado)
                {
                    escribir.Write(instancias[columna].ElementAt(fila));

                    if (encabezado.Count() > contador)
                    {
                        escribir.Write(',');
                    }
                    else
                    {
                        escribir.WriteLine("");
                    }
                    contador += 1;
                }
            }

            escribir.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveArchivos = new SaveFileDialog();
            saveArchivos.Title = "Guardar properties";
            saveArchivos.Filter = "Archivo Csv(*.csv)|*.csv";
            saveArchivos.ShowDialog();
            if (saveArchivos.FileName != "")
            {
                guardarCsv(saveArchivos.FileName);
            }
            else
                MessageBox.Show("No se selecciono alguna de las direcciones");
        }
    }
}
