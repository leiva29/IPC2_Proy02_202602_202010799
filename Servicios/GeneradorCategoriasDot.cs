using System.Text;
using IPC2_Proy02_202602_202010799.Estructuras;

namespace IPC2_Proy02_202602_202010799.Servicios;

public class GeneradorCategoriasDot
{
    private int siguienteId;

    public string Generar(NodoCategoria? inicio, bool incluirHermanos = true)
    {
        siguienteId = 0;

        var dot = new StringBuilder();
        dot.AppendLine("digraph Categorias {");
        dot.AppendLine("  rankdir=TB;");
        dot.AppendLine("  node [shape=box, style=rounded];");

        DibujarNivel(dot, inicio, null, incluirHermanos);

        dot.AppendLine("}");
        return dot.ToString();
    }

    private void DibujarNivel(
        StringBuilder dot,
        NodoCategoria? actual,
        int? idPadre,
        bool incluirHermanos)
    {
        while (actual != null)
        {
            int idActual = siguienteId++;

            dot.Append("  n").Append(idActual)
                .Append(" [label=\"")
                .Append(Escapar(actual.Nombre))
                .AppendLine("\"];");

            if (idPadre != null)
            {
                dot.Append("  n").Append(idPadre.Value)
                    .Append(" -> n").Append(idActual)
                    .AppendLine(";");
            }

            // Los hijos de esta categoría sí deben mostrarse todos.
            DibujarNivel(dot, actual.PrimerHijo, idActual, true);

            // Si empezamos desde una subcategoría específica,
            // no mostramos sus hermanos.
            if (!incluirHermanos)
                break;

            actual = actual.SiguienteHermano;
        }
    }

    private static string Escapar(string texto) =>
        texto.Replace("\\", "\\\\")
             .Replace("\"", "\\\"")
             .Replace("\r", " ")
             .Replace("\n", " ");
}