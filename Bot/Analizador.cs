using System;
using System.Collections.Generic;
using System.Text;
using HeatonResearch.Spider.HTML;
using System.Net;
using System.IO;

namespace Bot
{
    public class Analizador
    {
        //Atributo abstracto
        public ParseHTML html;
        public HttpWebResponse respuesta;
        public Stream istream;

        public Analizador(string url)
        {
            HttpWebRequest peticion = (HttpWebRequest)HttpWebRequest.Create(url);
            peticion.Timeout = 200000;
            //System.Net.WebProxy x = new System.Net.WebProxy("192.168.1.34", 808);
            //peticion.AllowAutoRedirect = true;
            //peticion.Proxy = x;
            peticion.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; es-ES; rv:1.9.0.3) Gecko/2008092417 Firefox/3.0.3";
            respuesta = (HttpWebResponse)peticion.GetResponse();
            istream = respuesta.GetResponseStream();
            html = new ParseHTML(istream);
        }

        public void Cerrar()
        {
            respuesta.Close();
            istream.Close();
        }

        public string Leer(string tag, string atributo)
        {
            int ch;
            while ((ch = html.Read()) != -1)
            {
                if (ch == 0)
                    if (html.Tag.Name == tag)
                        return (html.Tag[atributo]);
            }
            return null;
        }
    }

    public class Imagenes : Analizador
    {
        // Invocamos al constructor original
        public Imagenes(string url) : base(url)
        {
        }

        /// <summary>
        /// Devuelve la proxima imagen que se encuentre
        /// </summary>
        /// <returns>Null si ya no quedan más imagenes.</returns>
        public string Leer()
        {
            return base.Leer("img", "src");
        }
    }

    public class Enlaces : Analizador
    {
        // Invocamos al constructor original
        public Enlaces(string url) : base(url)
        {
        }

        public string Leer()
        {
            return base.Leer("a", "href");
        }
    }
}
