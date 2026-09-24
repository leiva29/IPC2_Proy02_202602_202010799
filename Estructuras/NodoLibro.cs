using IPC2_Proy02_202602_202010799.Models;

namespace IPC2_Proy02_202602_202010799.Estructuras;

public class NodoLibro
{
    public Libro Libro { get; set; }
    public NodoLibro? Izquierdo { get; set; }
    public NodoLibro? Derecho { get; set; }
    public int Altura { get; set; }

    public NodoLibro(Libro libro)
    {
        Libro = libro;
        Altura = 1;
    }
}