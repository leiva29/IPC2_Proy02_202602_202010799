using IPC2_Proy02_202602_202010799.Models;

namespace IPC2_Proy02_202602_202010799.Estructuras;

public class ArbolLibros
{
    private NodoLibro? raiz;

    public bool Agregar(Libro libro)
    {
        bool agregado = false;
        raiz = Insertar(raiz, libro, ref agregado);
        return agregado;
    }

    public Libro? Buscar(int isbn)
    {
        NodoLibro? actual = raiz;

        while (actual != null)
        {
            if (isbn == actual.Libro.ISBN)
                return actual.Libro;

            actual = isbn < actual.Libro.ISBN
                ? actual.Izquierdo
                : actual.Derecho;
        }

        return null;
    }

    private NodoLibro Insertar(NodoLibro? nodo, Libro libro, ref bool agregado)
    {
        if (nodo == null)
        {
            agregado = true;
            return new NodoLibro(libro);
        }

        if (libro.ISBN < nodo.Libro.ISBN)
            nodo.Izquierdo = Insertar(nodo.Izquierdo, libro, ref agregado);
        else if (libro.ISBN > nodo.Libro.ISBN)
            nodo.Derecho = Insertar(nodo.Derecho, libro, ref agregado);
        else
            return nodo; // El ISBN ya existe.

        ActualizarAltura(nodo);
        int balance = Balance(nodo);

        if (balance > 1 && libro.ISBN < nodo.Izquierdo!.Libro.ISBN)
            return RotarDerecha(nodo);

        if (balance < -1 && libro.ISBN > nodo.Derecho!.Libro.ISBN)
            return RotarIzquierda(nodo);

        if (balance > 1 && libro.ISBN > nodo.Izquierdo!.Libro.ISBN)
        {
            nodo.Izquierdo = RotarIzquierda(nodo.Izquierdo);
            return RotarDerecha(nodo);
        }

        if (balance < -1 && libro.ISBN < nodo.Derecho!.Libro.ISBN)
        {
            nodo.Derecho = RotarDerecha(nodo.Derecho);
            return RotarIzquierda(nodo);
        }

        return nodo;
    }

    private static int Altura(NodoLibro? nodo) => nodo?.Altura ?? 0;

    private static int Balance(NodoLibro nodo) =>
        Altura(nodo.Izquierdo) - Altura(nodo.Derecho);

    private static void ActualizarAltura(NodoLibro nodo) =>
        nodo.Altura = 1 + Math.Max(Altura(nodo.Izquierdo), Altura(nodo.Derecho));

    private static NodoLibro RotarDerecha(NodoLibro nodo)
    {
        NodoLibro nuevaRaiz = nodo.Izquierdo!;
        nodo.Izquierdo = nuevaRaiz.Derecho;
        nuevaRaiz.Derecho = nodo;

        ActualizarAltura(nodo);
        ActualizarAltura(nuevaRaiz);
        return nuevaRaiz;
    }

    private static NodoLibro RotarIzquierda(NodoLibro nodo)
    {
        NodoLibro nuevaRaiz = nodo.Derecho!;
        nodo.Derecho = nuevaRaiz.Izquierdo;
        nuevaRaiz.Izquierdo = nodo;

        ActualizarAltura(nodo);
        ActualizarAltura(nuevaRaiz);
        return nuevaRaiz;
    }
    public Libro? ObtenerMenor()
    {
        NodoLibro? actual = raiz;

        if (actual == null)
            return null;

        while (actual.Izquierdo != null)
            actual = actual.Izquierdo;

        return actual.Libro;
    }

    public Libro? ObtenerMayor()
    {
        NodoLibro? actual = raiz;

        if (actual == null)
            return null;

        while (actual.Derecho != null)
            actual = actual.Derecho;

        return actual.Libro;
    }

    public void RecorrerEnOrden(Action<Libro> mostrar)
    {
        RecorrerEnOrden(raiz, mostrar);
    }

    private void RecorrerEnOrden(NodoLibro? nodo, Action<Libro> mostrar)
    {
        if (nodo == null)
            return;

        RecorrerEnOrden(nodo.Izquierdo, mostrar);
        mostrar(nodo.Libro);
        RecorrerEnOrden(nodo.Derecho, mostrar);
    }
    public bool Eliminar(int isbn)
    {
        bool eliminado = false;
        raiz = EliminarNodo(raiz, isbn, ref eliminado);
        return eliminado;
    }

    private NodoLibro? EliminarNodo(
        NodoLibro? nodo, int isbn, ref bool eliminado)
    {
        if (nodo == null)
            return null;

        if (isbn < nodo.Libro.ISBN)
        {
            nodo.Izquierdo = EliminarNodo(nodo.Izquierdo, isbn, ref eliminado);
        }
        else if (isbn > nodo.Libro.ISBN)
        {
            nodo.Derecho = EliminarNodo(nodo.Derecho, isbn, ref eliminado);
        }
        else
        {
            eliminado = true;

            if (nodo.Izquierdo == null)
                return nodo.Derecho;

            if (nodo.Derecho == null)
                return nodo.Izquierdo;

            // Tiene dos hijos: lo reemplazamos con el menor del lado derecho.
            NodoLibro sucesor = nodo.Derecho;
            while (sucesor.Izquierdo != null)
                sucesor = sucesor.Izquierdo;

            nodo.Libro = sucesor.Libro;
            bool sucesorEliminado = false;
            nodo.Derecho = EliminarNodo(
                nodo.Derecho, sucesor.Libro.ISBN, ref sucesorEliminado);
        }

        ActualizarAltura(nodo);
        int balance = Balance(nodo);

        if (balance > 1)
        {
            if (Balance(nodo.Izquierdo!) < 0)
                nodo.Izquierdo = RotarIzquierda(nodo.Izquierdo!);

            return RotarDerecha(nodo);
        }

        if (balance < -1)
        {
            if (Balance(nodo.Derecho!) > 0)
                nodo.Derecho = RotarDerecha(nodo.Derecho!);

            return RotarIzquierda(nodo);
        }

        return nodo;
    }

}