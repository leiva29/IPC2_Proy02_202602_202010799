using IPC2_Proy02_202602_202010799.Estructuras;
using IPC2_Proy02_202602_202010799.Models;
using IPC2_Proy02_202602_202010799.Servicios;
using System.Xml;
using System.Net;
using System.Text;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var arbol = new ArbolLibros();
var categorias = new ArbolCategorias();

app.MapGet("/", () => Results.Content("""
    <h1>Catálogo de libros</h1>
    <h2>Registrar libro</h2>
    <form method="post" action="/libros">
        <input name="isbn" type="number" min="1" placeholder="ISBN" required>
        <input name="titulo" placeholder="Título" required>
        <input name="autor" placeholder="Autor" required>
        <input name="categoria" placeholder="Categoría" required>
        <button type="submit">Registrar</button>
    </form>
    <h2>Buscar por ISBN</h2>
    <h2>Extremos del catálogo</h2>
    <p><a href="/libros">Ver todos los libros ordenados por ISBN</a></p>
    <a href="/libros/menor">Ver libro con menor ISBN</a><br>
    <a href="/libros/mayor">Ver libro con mayor ISBN</a>
    <form method="get" action="/buscar">
        <input name="isbn" type="number" min="1" placeholder="ISBN" required>
        <button type="submit">Buscar</button>
    </form>
    <h2>Eliminar libro</h2>
    <form method="post" action="/libros/eliminar">
        <input name="isbn" type="number" min="1" placeholder="ISBN" required>
        <button type="submit">Eliminar</button>
    </form>

    <h2>Agregar categoría</h2>
    <form method="post" action="/categorias">
        <input name="nombre" placeholder="Nombre de la categoría" required>
        <input name="padre" placeholder="Categoría padre (opcional)">
        <button type="submit">Agregar categoría</button>
    </form>
    <p><a href="/categorias">Ver estructura de categorías</a></p>

    <h2>Libros de una categoría</h2>
    <form method="get" action="/categorias/libros">
        <input name="nombre" placeholder="Nombre de la categoría" required>
        <button type="submit">Ver libros</button>
    </form>

    <h2>Cargar archivo XML</h2>
    <form method="post" action="/cargar-xml" enctype="multipart/form-data">
        <input type="file" name="archivo" accept=".xml" required>
        <button type="submit">Cargar XML</button>
    </form>

    <h2>Gráfica de categorías</h2>
    <p><a href="/categorias/grafica">Ver todas las categorías</a></p>
    <form method="get" action="/categorias/grafica">
        <input name="nombre" placeholder="Subcategoría desde la cual iniciar">
        <button type="submit">Ver subcategoría</button>
    </form>

    <h2>Gráfica de libros por categoría</h2>
    <form method="get" action="/categorias/libros/grafica">
        <input name="nombre" placeholder="Nombre de la categoría" required>
        <button type="submit">Ver gráfica de libros</button>
    </form>

    <h2>Inicializar catálogo</h2>
    <form method="post" action="/inicializar"
        onsubmit="return confirm('¿Borrar todos los libros y categorías cargados?')">
        <button type="submit">Vaciar catálogo</button>
    </form>

    """, "text/html; charset=utf-8"));

app.MapPost("/libros", async (HttpRequest request) =>
{
    var datos = await request.ReadFormAsync();

    if (!int.TryParse(datos["isbn"], out int isbn) || isbn <= 0 ||
        string.IsNullOrWhiteSpace(datos["titulo"]) ||
        string.IsNullOrWhiteSpace(datos["autor"]) ||
        string.IsNullOrWhiteSpace(datos["categoria"]))
    {
        return Results.BadRequest("Completa todos los datos con un ISBN válido.");
    }

    string nombreCategoria = datos["categoria"].ToString().Trim();

    if (categorias.Buscar(nombreCategoria) == null)
    {
        return Results.BadRequest(
            "Primero debes crear la categoría del libro.");
    }

    var libro = new Libro
    {
        ISBN = isbn,
        Titulo = datos["titulo"].ToString().Trim(),
        Autor = datos["autor"].ToString().Trim(),
        Categoria = nombreCategoria
    };

    if (!arbol.Agregar(libro))
        return Results.Conflict("Ya existe un libro con ese ISBN.");

    return Results.Content(
        "<p>Libro registrado correctamente.</p><a href='/'>Volver</a>",
        "text/html; charset=utf-8");
});

app.MapGet("/buscar", (int isbn) =>
{
    Libro? libro = arbol.Buscar(isbn);

    if (libro == null)
        return Results.Content(
            "<p>No se encontró ese ISBN.</p><a href='/'>Volver</a>",
            "text/html; charset=utf-8");

    return Results.Content($"""
        <h1>{WebUtility.HtmlEncode(libro.Titulo)}</h1>
        <p>ISBN: {libro.ISBN}</p>
        <p>Autor: {WebUtility.HtmlEncode(libro.Autor)}</p>
        <p>Categoría: {WebUtility.HtmlEncode(libro.Categoria)}</p>
        <a href="/">Volver</a>
        """, "text/html; charset=utf-8");
});


app.MapGet("/libros/menor", () =>
{
    Libro? libro = arbol.ObtenerMenor();

    if (libro == null)
        return Results.Content(
            "<p>El catálogo está vacío.</p><a href='/'>Volver</a>",
            "text/html; charset=utf-8");

    return Results.Content($"""
        <h1>Libro con menor ISBN</h1>
        <p>ISBN: {libro.ISBN}</p>
        <p>Título: {WebUtility.HtmlEncode(libro.Titulo)}</p>
        <p>Autor: {WebUtility.HtmlEncode(libro.Autor)}</p>
        <a href="/">Volver</a>
        """, "text/html; charset=utf-8");
});

app.MapGet("/libros/mayor", () =>
{
    Libro? libro = arbol.ObtenerMayor();

    if (libro == null)
        return Results.Content(
            "<p>El catálogo está vacío.</p><a href='/'>Volver</a>",
            "text/html; charset=utf-8");

    return Results.Content($"""
        <h1>Libro con mayor ISBN</h1>
        <p>ISBN: {libro.ISBN}</p>
        <p>Título: {WebUtility.HtmlEncode(libro.Titulo)}</p>
        <p>Autor: {WebUtility.HtmlEncode(libro.Autor)}</p>
        <a href="/">Volver</a>
        """, "text/html; charset=utf-8");
});

app.MapGet("/libros", () =>
{
    var html = new StringBuilder(
        "<h1>Libros ordenados por ISBN</h1><ol>");

    arbol.RecorrerEnOrden(libro =>
    {
        html.Append("<li>ISBN: ")
            .Append(libro.ISBN)
            .Append(" — ")
            .Append(WebUtility.HtmlEncode(libro.Titulo))
            .Append(" — ")
            .Append(WebUtility.HtmlEncode(libro.Autor))
            .Append("</li>");
    });

    html.Append("</ol><a href='/'>Volver</a>");

    return Results.Content(html.ToString(), "text/html; charset=utf-8");
});

app.MapPost("/libros/eliminar", async (HttpRequest request) =>
{
    var datos = await request.ReadFormAsync();

    if (!int.TryParse(datos["isbn"], out int isbn) || isbn <= 0)
        return Results.BadRequest("Ingresa un ISBN válido.");

    if (!arbol.Eliminar(isbn))
        return Results.Content(
            "<p>No existe un libro con ese ISBN.</p><a href='/'>Volver</a>",
            "text/html; charset=utf-8");

    return Results.Content(
        "<p>Libro eliminado correctamente.</p><a href='/'>Volver</a>",
        "text/html; charset=utf-8");
});

app.MapPost("/categorias", async (HttpRequest request) =>
{
    var datos = await request.ReadFormAsync();
    string nombre = datos["nombre"].ToString();
    string padre = datos["padre"].ToString();

    if (!categorias.Agregar(nombre, padre))
        return Results.Content(
            "<p>No se pudo agregar: el nombre ya existe, está vacío o el padre no existe.</p><a href='/'>Volver</a>",
            "text/html; charset=utf-8");

    return Results.Content(
        "<p>Categoría agregada.</p><a href='/'>Volver</a>",
        "text/html; charset=utf-8");
});

app.MapGet("/categorias", () =>
{
    var html = new StringBuilder("<h1>Categorías</h1>");

    if (categorias.PrimeraRaiz == null)
        html.Append("<p>Aún no hay categorías.</p>");
    else
        MostrarCategorias(html, categorias.PrimeraRaiz);

    html.Append("<a href='/'>Volver</a>");
    return Results.Content(html.ToString(), "text/html; charset=utf-8");
});

static void MostrarCategorias(StringBuilder html, NodoCategoria? actual)
{
    html.Append("<ul>");

    while (actual != null)
    {
        html.Append("<li>")
            .Append(WebUtility.HtmlEncode(actual.Nombre));

        if (actual.PrimerHijo != null)
            MostrarCategorias(html, actual.PrimerHijo);

        html.Append("</li>");
        actual = actual.SiguienteHermano;
    }

    html.Append("</ul>");
}

app.MapGet("/categorias/libros", (string nombre) =>
{
    NodoCategoria? categoria = categorias.Buscar(nombre);

    if (categoria == null)
        return Results.Content(
            "<p>La categoría no existe.</p><a href='/'>Volver</a>",
            "text/html; charset=utf-8");

    var html = new StringBuilder();

    html.Append("<h1>Libros de ")
        .Append(WebUtility.HtmlEncode(categoria.Nombre))
        .Append("</h1><ol>");

    int cantidad = 0;

    arbol.RecorrerEnOrden(libro =>
    {
        if (!string.Equals(
            libro.Categoria,
            categoria.Nombre,
            StringComparison.OrdinalIgnoreCase))
            return;

        html.Append("<li>ISBN: ")
            .Append(libro.ISBN)
            .Append(" — ")
            .Append(WebUtility.HtmlEncode(libro.Titulo))
            .Append(" — ")
            .Append(WebUtility.HtmlEncode(libro.Autor))
            .Append("</li>");

        cantidad++;
    });

    html.Append("</ol>");

    if (cantidad == 0)
        html.Append("<p>Esta categoría aún no tiene libros.</p>");

    html.Append("<a href='/'>Volver</a>");

    return Results.Content(html.ToString(), "text/html; charset=utf-8");
});

app.MapPost("/cargar-xml", async (HttpRequest request) =>
{
    var formulario = await request.ReadFormAsync();
    var archivo = formulario.Files.GetFile("archivo");

    if (archivo == null || archivo.Length == 0)
        return Results.BadRequest("Selecciona un archivo XML.");

    try
    {
        using Stream contenido = archivo.OpenReadStream();
        var cargador = new CargadorXml();
        string resultado = cargador.Cargar(contenido, categorias, arbol);

        return Results.Content(
            $"<p>{WebUtility.HtmlEncode(resultado)}</p><a href='/'>Volver</a>",
            "text/html; charset=utf-8");
    }
    catch (XmlException)
    {
        return Results.BadRequest("El archivo no contiene XML válido.");
    }
    catch (FormatException error)
    {
        return Results.BadRequest(error.Message);
    }
});

app.MapGet("/categorias/grafica", async (string? nombre) =>
{
    NodoCategoria? inicio;
    bool incluirHermanos;

    if (string.IsNullOrWhiteSpace(nombre))
    {
        inicio = categorias.PrimeraRaiz;
        incluirHermanos = true;
    }
    else
    {
        inicio = categorias.Buscar(nombre);
        incluirHermanos = false;

        if (inicio == null)
            return Results.NotFound("La categoría no existe.");
    }

    if (inicio == null)
        return Results.BadRequest("Aún no hay categorías para graficar.");

    string dot = new GeneradorCategoriasDot()
        .Generar(inicio, incluirHermanos);

    var opciones = new ProcessStartInfo("dot", "-Tsvg")
    {
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    try
    {
        using Process proceso = Process.Start(opciones)!;

        await proceso.StandardInput.WriteAsync(dot);
        proceso.StandardInput.Close();

        Task<string> salida = proceso.StandardOutput.ReadToEndAsync();
        Task<string> errores = proceso.StandardError.ReadToEndAsync();

        await proceso.WaitForExitAsync();

        if (proceso.ExitCode != 0)
            return Results.Problem(await errores);

        return Results.Content(await salida, "image/svg+xml");
    }
    catch (System.ComponentModel.Win32Exception)
    {
        return Results.Problem(
            "No se encontró Graphviz. Comprueba 'dot -V' en la terminal de VS Code.");
    }
});

app.MapGet("/categorias/libros/grafica", async (string nombre) =>
{
    NodoCategoria? categoria = categorias.Buscar(nombre);

    if (categoria == null)
        return Results.NotFound("La categoría no existe.");

    string dot = new GeneradorLibrosDot()
        .Generar(arbol, categoria.Nombre);

    var opciones = new ProcessStartInfo("dot", "-Tsvg")
    {
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    try
    {
        using Process proceso = Process.Start(opciones)!;

        await proceso.StandardInput.WriteAsync(dot);
        proceso.StandardInput.Close();

        Task<string> salida = proceso.StandardOutput.ReadToEndAsync();
        Task<string> errores = proceso.StandardError.ReadToEndAsync();

        await proceso.WaitForExitAsync();

        if (proceso.ExitCode != 0)
            return Results.Problem(await errores);

        return Results.Content(await salida, "image/svg+xml");
    }
    catch (System.ComponentModel.Win32Exception)
    {
        return Results.Problem(
            "No se encontró Graphviz. Comprueba 'dot -V' en la terminal.");
    }
});

app.MapPost("/inicializar", () =>
{
    arbol = new ArbolLibros();
    categorias = new ArbolCategorias();

    return Results.Content(
        "<p>El catálogo está vacío.</p><a href='/'>Volver</a>",
        "text/html; charset=utf-8");
});

app.Run();