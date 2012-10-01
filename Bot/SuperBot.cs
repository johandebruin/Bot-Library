using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace Bot
{
    public class SuperBot
    {
        //Atributos
        public OleDbDataReader lector;
        public OleDbConnection conexion;
        string strconexion = @"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\Peliculas.mdf;Integrated Security=True;User Instance=True;Provider=SQLOLEDB;";

        public void Obtener(string consulta)
        {
            conexion = new OleDbConnection(strconexion);
            OleDbCommand orden = new OleDbCommand(consulta, conexion);
            try
            {
                conexion.Open();
                lector = orden.ExecuteReader();
            }
            catch(Exception e)
            {
                MessageBox.Show("Problema conectando con la BD: \n\r" + e.ToString());
            }
        }

        public void Cerrar()
        {
            if (lector != null) { lector.Close(); }
        }
    }
}
