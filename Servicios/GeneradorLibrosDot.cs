using System.Text;
using IPC2_Proy02_202602_202010799.Estructuras;

namespace IPC2_Proy02_202602_202010799.Servicios;

public class GeneradorLibrosDot
{
    public string Generar(ArbolLibros libros, string categoria)
    {
        var dot = new StringBuilder();

        dot.AppendLine("digraph Libros {");
        dot.AppendLine("  rankdir=LR;");
        dot.AppendLine("  node [shape=box, style=rounded];");

        int cantidad = 0;
        int? anterior = null;

        libros.RecorrerEnOrden(libro =>
        {
            if (!string.Equals(
                libro.Categoria,
                categoria,
                StringComparison.OrdinalIgnoreCase))
                return;

            int actual = cantidad++;

            dot.Append("  n").Append(actual)
                .Append(" [label=\"ISBN: ")
                .Append(libro.ISBN)
                .Append("\\n")
                .Append(Escapar(libro.Titulo))
                .AppendLine("\"];");

            if (anterior != null)
            {
                dot.Append("  n").Append(anterior.Value)
                    .Append(" -> n").Append(actual)
                    .AppendLine(";");
            }

            anterior = actual;
        });

        if (cantidad == 0)
            dot.AppendLine("  vacio [label=\"Esta categoría no tiene libros\"];");

        dot.AppendLine("}");
        return dot.ToString();
    }

    private static string Escapar(string texto) =>
        texto.Replace("\\", "\\\\")
             .Replace("\"", "\\\"")
             .Replace("\r", " ")
             .Replace("\n", " ");
}