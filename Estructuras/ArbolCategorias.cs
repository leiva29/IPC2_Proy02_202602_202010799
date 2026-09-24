namespace IPC2_Proy02_202602_202010799.Estructuras;

public class ArbolCategorias
{
    private NodoCategoria? primeraRaiz;

    public NodoCategoria? PrimeraRaiz => primeraRaiz;

    public NodoCategoria? Buscar(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return null;

        return BuscarDesde(primeraRaiz, nombre.Trim());
    }

    public bool Agregar(string nombre, string? nombrePadre = null)
    {
        nombre = nombre?.Trim() ?? "";
        nombrePadre = nombrePadre?.Trim();

        if (nombre.Length == 0 || Buscar(nombre) != null)
            return false;

        NodoCategoria? padre = null;

        if (!string.IsNullOrEmpty(nombrePadre))
        {
            padre = Buscar(nombrePadre);
            if (padre == null)
                return false;
        }

        var nueva = new NodoCategoria(nombre)
        {
            Padre = padre
        };

        if (padre == null)
            primeraRaiz = InsertarOrdenado(primeraRaiz, nueva);
        else
            padre.PrimerHijo = InsertarOrdenado(padre.PrimerHijo, nueva);

        return true;
    }

    private static NodoCategoria? BuscarDesde(
        NodoCategoria? actual, string nombre)
    {
        while (actual != null)
        {
            if (string.Equals(
                actual.Nombre, nombre,
                StringComparison.CurrentCultureIgnoreCase))
                return actual;

            NodoCategoria? encontrado =
                BuscarDesde(actual.PrimerHijo, nombre);

            if (encontrado != null)
                return encontrado;

            actual = actual.SiguienteHermano;
        }

        return null;
    }

    private static NodoCategoria InsertarOrdenado(
        NodoCategoria? primero, NodoCategoria nueva)
    {
        if (primero == null ||
            string.Compare(
                nueva.Nombre, primero.Nombre,
                StringComparison.CurrentCultureIgnoreCase) < 0)
        {
            nueva.SiguienteHermano = primero;
            return nueva;
        }

        NodoCategoria actual = primero;

        while (actual.SiguienteHermano != null &&
               string.Compare(
                   actual.SiguienteHermano.Nombre, nueva.Nombre,
                   StringComparison.CurrentCultureIgnoreCase) < 0)
        {
            actual = actual.SiguienteHermano;
        }

        nueva.SiguienteHermano = actual.SiguienteHermano;
        actual.SiguienteHermano = nueva;
        return primero;
    }
}