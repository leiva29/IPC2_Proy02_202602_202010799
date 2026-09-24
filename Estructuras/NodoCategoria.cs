namespace IPC2_Proy02_202602_202010799.Estructuras;

public class NodoCategoria
{
    public string Nombre { get; set; }
    public NodoCategoria? Padre { get; set; }
    public NodoCategoria? PrimerHijo { get; set; }
    public NodoCategoria? SiguienteHermano { get; set; }

    public NodoCategoria(string nombre)
    {
        Nombre = nombre;
    }
}