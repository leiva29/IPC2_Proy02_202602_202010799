using System.Xml;
using IPC2_Proy02_202602_202010799.Estructuras;
using IPC2_Proy02_202602_202010799.Models;

namespace IPC2_Proy02_202602_202010799.Servicios;

public class CargadorXml
{
    public string Cargar(
        Stream archivo,
        ArbolCategorias categorias,
        ArbolLibros libros)
    {
        var configuracion = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        var documento = new XmlDocument
        {
            XmlResolver = null
        };

        using (XmlReader lector = XmlReader.Create(archivo, configuracion))
            documento.Load(lector);

        XmlElement? config = documento.DocumentElement;

        if (config == null || config.Name != "config")
            throw new FormatException("El XML debe comenzar con <config>.");

        int categoriasAgregadas = 0;
        int librosAgregados = 0;
        int librosOmitidos = 0;

        XmlNode? listaCategorias = config.SelectSingleNode("listaCategorias");

        if (listaCategorias != null)
        {
            // Repetimos para resolver padres que aparezcan después de sus hijos.
            bool huboCambios;

            do
            {
                huboCambios = false;

                foreach (XmlNode nodo in listaCategorias.ChildNodes)
                {
                    if (nodo.Name != "categoria")
                        continue;

                    string nombre = nodo.InnerText.Trim();
                    string? padre = nodo.Attributes?["padre"]?.Value.Trim();

                    if (nombre.Length == 0 ||
                        categorias.Buscar(nombre) != null)
                        continue;

                    if (categorias.Agregar(nombre, padre))
                    {
                        categoriasAgregadas++;
                        huboCambios = true;
                    }
                }
            }
            while (huboCambios);
        }

        XmlNode? listaLibros = config.SelectSingleNode("listaLibros");

        if (listaLibros != null)
        {
            foreach (XmlNode nodo in listaLibros.ChildNodes)
            {
                if (nodo.Name != "libro")
                    continue;

                string? textoIsbn = nodo.SelectSingleNode("ISBN")?.InnerText;
                string titulo = nodo.SelectSingleNode("titulo")?.InnerText.Trim() ?? "";
                string autor = nodo.SelectSingleNode("autor")?.InnerText.Trim() ?? "";
                string categoria = nodo.SelectSingleNode("categoria")?.InnerText.Trim() ?? "";

                if (!int.TryParse(textoIsbn, out int isbn) ||
                    isbn <= 0 ||
                    titulo.Length == 0 ||
                    autor.Length == 0 ||
                    categorias.Buscar(categoria) == null)
                {
                    librosOmitidos++;
                    continue;
                }

                var libro = new Libro
                {
                    ISBN = isbn,
                    Titulo = titulo,
                    Autor = autor,
                    Categoria = categoria
                };

                if (libros.Agregar(libro))
                    librosAgregados++;
                else
                    librosOmitidos++;
            }
        }

        return $"Categorías agregadas: {categoriasAgregadas}. " +
               $"Libros agregados: {librosAgregados}. " +
               $"Libros omitidos: {librosOmitidos}.";
    }
}