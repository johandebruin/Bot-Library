using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using HeatonResearch.Spider.HTML;
using System.Data.SqlClient;

namespace Bot
{
    public class MiembrosEstaticos
    {
        /// <summary>
        /// Este metodo descarga la URI especificada y transforma
        /// la respuesta en una cadena (el codigo HTML normalmente)
        /// </summary>
        /// <param name="url">La URL a descargar</param>
        /// <returns>El contenido en tipo string de la URL</returns>
        public static string DescargarCadena(string url,CookieContainer galletas)
        {
            HttpWebRequest http = (HttpWebRequest)HttpWebRequest.Create(url);
            //Establecemos el tiempo de espera en 1 segundo
            http.Headers.Add("Accept-Charset", "utf-8");
            http.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; es-ES; rv:1.9.0.3) Gecko/2008092417 Firefox/3.0.3";
            if (galletas != null)
                http.CookieContainer = galletas;
            //Recogemos y leemos la respuesta
            HttpWebResponse response = (HttpWebResponse)http.GetResponse();
            StreamReader stream = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
            String result = stream.ReadToEnd();
            //Cerramos y enviamos
            response.Close();
            stream.Close();
            return result;
        }

        /// <summary>
        /// Este metodo se dedica a descargar la subcadena especificada
        /// del codigo HTML de una pagina. Ademas puedes especificar el
        /// la itineracion o veces que se encuentra con la misma situacion
        /// hasta que encuentre la cadena deseada
        /// </summary>
        /// <param name="str">El codigo HTML a evaluar</param>
        /// <param name="token1">El texto que aparece antes de la cadena deseada</param>
        /// <param name="token2">El texto que aparece despues de la cadena deseada</param>
        /// <param name="count">Cuantas veces ignorar token1 hasta dar con el acertado
        /// (minimo 1)</param>
        /// <returns>El contenido de entre los 2 puntos, o null si no lo encuentra</returns>
        public static String Extraer(String str, String token1, String token2, int count)
        {
            //Ponemos todo en minuscula para que no le afecten las mayusculas.
            str = str.ToLower();
            token1 = token1.ToLower();
            token2 = token2.ToLower();

            int location1, location2;

            location1 = location2 = 0;
            do
            {
                location1 = str.IndexOf(token1, location1 + 1);

                if (location1 == -1)
                    return null;

                count--;
            } while (count > 0);

            location2 = str.IndexOf(token2, location1 + 1);
            if (location2 == -1)
                return null;

            location1 += token1.Length;
            return str.Substring(location1, location2 - location1);
        }

        /// <summary>
        /// Este metodo descarga la Uri especificada dentro del fichero 
        /// indicado, conveniente a la hora de descargar cosas que no sea
        /// texto (imagenes, videos, ejecutables...)
        /// </summary>
        /// <param name="url">La Uri definitiva (el final .jpg o .flv)</param>
        /// <param name="filename">Ruta y nombre del fichero (C:/imagen.jpg)</param>
        public static void DescargarBinario(Uri url, String filename)
        {
            //Buffer actuara como intermediario para insertar informacion
            //en el fichero os. Limitamos el buffer en 4096 bytes
            byte[] buffer = new byte[4096];
            FileStream os = new FileStream(filename, FileMode.Create);

            HttpWebRequest http = (HttpWebRequest)HttpWebRequest.Create(url);
            http.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; es-ES; rv:1.9.0.3) Gecko/2008092417 Firefox/3.0.3";
            http.Timeout = 200000;
            HttpWebResponse response = (HttpWebResponse)http.GetResponse();
            //Transformamos la respuesta en un stream
            Stream stream = response.GetResponseStream();
            int count = 0;
            do
            {
                //Count muestra cuanta informacion es aguantada por buffer
                //Mientras este no sea = a 0 seguira leyendo del stream
                count = stream.Read(buffer, 0, buffer.Length);
                if (count > 0)
                    //Escribe el buffer(seccion) leido en el fichero
                    os.Write(buffer, 0, count);
            } while (count > 0);

            response.Close();
            stream.Close();
            os.Close();
        }

        /// <summary>
        /// Este metodo escribira en la consola los encabezados recibidos
        /// de la respuesta que manda cierta URL.
        /// </summary>
        /// <param name="u">La URL en cadena que se quiera comprobar</param>
        public static void EscanearHeaders(String u)
        {
            Uri url = new Uri(u);
            WebRequest http = HttpWebRequest.Create(url);
            WebResponse response = http.GetResponse();

            int count = 0;
            String key, value;

            for (count = 0; count < response.Headers.Keys.Count; count++)
            {
                key = response.Headers.Keys[count];
                value = response.Headers[key];

                if (value != null)
                {
                    if (key == null)
                        Console.WriteLine(value);
                    else
                        Console.WriteLine(key + ": " + value);
                }
            }
        }

        /// <summary>
        /// Metodo muy parecido a Extraer, solo que en este caso avanza el
        /// HTML parseado hasta el tag espeficificado numero count
        /// </summary>
        /// <param name="parse">El HTML parseado</param>
        /// <param name="tag">Por ejemplo "title" o "select"</param>
        /// <param name="count">1 para el primero encontrado, 2 para el segundo...</param>
        /// <returns>True si lo encontro, false si no</returns>
        public static bool Avanzar(ParseHTML parse, String tag, int count)
        {
            int ch;
            while ((ch = parse.Read()) != -1)
            {
                if (ch == 0)
                {
                    if (String.Compare(parse.Tag.Name, tag, true) == 0)
                    {
                        count--;
                        if (count <= 0)
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Versión mejorada para avanzar a un tag que cumpla cierto atributo
        /// </summary>
        /// <param name="analizador">El parseHTML que queramos avanzar en</param>
        /// <param name="etiqueta">La etiqueta a la que queramos llegar</param>
        /// <param name="nombreAtributo">El nombre del atributo como "src"</param>
        /// <param name="atributo">el propio atributo como "/imagenes/...</param>
        /// <returns></returns>
        public static bool AvanzarA(ParseHTML analizador, String etiqueta,String nombreAtributo, String atributo)
        {
            int ch;
            while ((ch = analizador.Read()) != -1)
            {
                if (ch == 0)
                {
                    if (analizador.Tag.Name== etiqueta && analizador.Tag[nombreAtributo] == atributo)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// UploadFTP(@"C:\Archivo.txt", "ftp://ftp.vtortola.net/Upload", "MiUsuario", "MiPassword");
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="RemotePath"></param>
        /// <param name="Login"></param>
        /// <param name="Password"></param>
        public static void UploadFTP(string FilePath, string RemotePath, string Login, string Password)
        {
            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                string url = Path.Combine(RemotePath, Path.GetFileName(FilePath));
                // Creo el objeto ftp
                FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(url);
                // Fijo las credenciales, usuario y contraseña
                ftp.Credentials = new NetworkCredential(Login, Password);
                // Le digo que no mantenga la conexión activa al terminar.
                ftp.KeepAlive = false;
                // Indicamos que la operación es subir un archivo...
                ftp.Method = WebRequestMethods.Ftp.UploadFile;
                // … en modo binario … (podria ser como ASCII)
                ftp.UseBinary = true;
                // Indicamos la longitud total de lo que vamos a enviar.
                ftp.ContentLength = fs.Length;
                // Desactivo cualquier posible proxy http.
                // Ojo pues de saltar este paso podría usar 
                // un proxy configurado en iexplorer
                ftp.Proxy = null;
                // Pongo el stream al inicio
                fs.Position = 0;
                // Configuro el buffer a 2 KBytes
                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;
                // obtener el stream del socket sobre el que se va a escribir.
                using (Stream strm = ftp.GetRequestStream())
                {
                    // Leer del buffer 2kb cada vez
                    contentLen = fs.Read(buff, 0, buffLength);
                    // mientras haya datos en el buffer ….
                    while (contentLen != 0)
                    {
                        // escribir en el stream de conexión
                        //el contenido del stream del fichero
                        strm.Write(buff, 0, contentLen);
                        contentLen = fs.Read(buff, 0, buffLength);
                    }
                }
            }
        }

        public static ParseHTML DescargarStream(string URL)
        {
            HttpWebRequest peticion = (HttpWebRequest)HttpWebRequest.Create(URL);
            peticion.Timeout = 10000;
            peticion.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; es-ES; rv:1.9.0.3) Gecko/2008092417 Firefox/3.0.3";
            HttpWebResponse respuesta = (HttpWebResponse)peticion.GetResponse();
            Stream istream = respuesta.GetResponseStream();
            return new ParseHTML(istream);
        }

        public static string tituloAmigable(string titulo)
        {
            string temp = titulo;
            if (titulo.IndexOf("(") > -1)
                temp = titulo.Substring(0,titulo.IndexOf("("));
            if (temp.IndexOf(":") > -1)
                temp = temp.Replace(":", "");
            temp = temp.Replace("*", "");
            if (temp.IndexOf("[") > -1)
                temp = temp.Substring(0, temp.IndexOf("["));
            temp = temp.Replace("á", "a").Replace("é", "e").Replace("í","i").Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n").Replace("É","E").Replace("Á","A").Replace("Í","Í").Replace("Ó","O").Replace("Ú","Ú");
            temp = temp.Trim();
            return temp;
        }
    }
}