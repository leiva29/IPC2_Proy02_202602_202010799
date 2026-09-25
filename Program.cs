using IPC2_Proy02_202602_202010799.Estructuras;
using IPC2_Proy02_202602_202010799.Models;
using IPC2_Proy02_202602_202010799.Servicios;
using System.Xml;
using System.Net;
using System.Text;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseStaticFiles();

var arbol = new ArbolLibros();
var categorias = new ArbolCategorias();

app.MapGet("/", () => Pagina("""
    <link rel="stylesheet" href="/estilos.css">
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

    <p><a href="/ayuda">Ayuda y documentación</a></p>

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

    return Pagina(
        "<p>Libro registrado correctamente.</p><a href='/'>Volver</a>",
        "text/html; charset=utf-8");
});

app.MapGet("/buscar", (int isbn) =>
{
    Libro? libro = arbol.Buscar(isbn);

    if (libro == null)
        return Pagina(
            "<p>No se encontró ese ISBN.</p><a href='/'>Volver</a>",
            "text/html; charset=utf-8");

    return Pagina($"""
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
        return Pagina(
            "<p>El catálogo está vacío.</p><a href='/'>Volver</a>",
            "text/html; charset=utf-8");

    return Pagina($"""
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
        return Pagina(
            "<p>El catálogo está vacío.</p><a href='/'>Volver</a>",
            "text/html; charset=utf-8");

    return Pagina($"""
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
        "<link rel='stylesheet' href='/estilos.css'>" +
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

    return Pagina(html.ToString(), "text/html; charset=utf-8");
});

app.MapPost("/libros/eliminar", async (HttpRequest request) =>
{
    var datos = await request.ReadFormAsync();

    if (!int.TryParse(datos["isbn"], out int isbn) || isbn <= 0)
        return Results.BadRequest("Ingresa un ISBN válido.");

    if (!arbol.Eliminar(isbn))
        return Pagina(
            "<p>No existe un libro con ese ISBN.</p><a href='/'>Volver</a>",
            "text/html; charset=utf-8");

    return Pagina(
        "<p>Libro eliminado correctamente.</p><a href='/'>Volver</a>",
        "text/html; charset=utf-8");
});

app.MapPost("/categorias", async (HttpRequest request) =>
{
    var datos = await request.ReadFormAsync();
    string nombre = datos["nombre"].ToString();
    string padre = datos["padre"].ToString();

    if (!categorias.Agregar(nombre, padre))
        return Pagina(
            "<p>No se pudo agregar: el nombre ya existe, está vacío o el padre no existe.</p><a href='/'>Volver</a>",
            "text/html; charset=utf-8");

    return Pagina(
        "<p>Categoría agregada.</p><a href='/'>Volver</a>",
        "text/html; charset=utf-8");
});

app.MapGet("/categorias", () =>
{
    var html = new StringBuilder(
        "<link rel='stylesheet' href='/estilos.css'>" +
        "<h1>Categorías</h1>");

    if (categorias.PrimeraRaiz == null)
        html.Append("<p>Aún no hay categorías.</p>");
    else
        MostrarCategorias(html, categorias.PrimeraRaiz);

    html.Append("<a href='/'>Volver</a>");
    return Pagina(html.ToString(), "text/html; charset=utf-8");
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
        return Pagina(
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

    return Pagina(html.ToString(), "text/html; charset=utf-8");
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

        return Pagina(
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

        return Pagina(await salida, "image/svg+xml");
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

        return Pagina(await salida, "image/svg+xml");
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

    return Pagina(
        "<p>El catálogo está vacío.</p><a href='/'>Volver</a>",
        "text/html; charset=utf-8");
});

static IResult Pagina(
    string contenido,
    string tipo = "text/html; charset=utf-8")
{
    // Los diagramas de Graphviz deben conservar su formato SVG.
    if (tipo == "image/svg+xml")
        return Results.Content(contenido, tipo);

    string documento = $"""
        <!doctype html>
        <html lang="es">
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <link rel="stylesheet" href="/estilos.css">
            <title>Catálogo de libros</title>
        </head>
        <body>
            <main class="panel">
                {contenido}
            </main>
        </body>
        </html>
        """;

    return Results.Content(documento, "text/html; charset=utf-8");
}

app.MapGet("/ayuda", () =>
{
    return Results.Content("""
        <!doctype html>
        <html lang="es">
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <link rel="stylesheet" href="/estilos.css">
            <title>Ayuda - Catálogo de libros</title>
        </head>
        <body>
            <main class="panel">
                <h1>Ayuda</h1>
                <p><strong>Proyecto:</strong> Catálogo jerárquico de libros</p>
                <p><strong>Estudiante:</strong> Héctor Fernando Leiva Toc</p>
                <p><strong>Carné:</strong> 202010799</p>
                <p><strong>Curso:</strong> Introducción a la Programación y Computación 2</p>
                <p><strong>Sección:</strong> N</p>
                <p>
                    <a href="https://github.com/leiva29/IPC2_Proy02_202602_202010799/blob/main/Documentation/Ensayo_Proyecto2_202010799.docx"
                       target="_blank" rel="noopener noreferrer">
                        Ver documentación del proyecto
                    </a>
                </p>
                <p><a href="/">Volver al catálogo</a></p>
            </main>
        </body>
        </html>
        """, "text/html; charset=utf-8");
});

app.MapGet("/documentacion", () =>
{
    string ruta = Path.Combine(
        app.Environment.ContentRootPath,
        "Documentation",
        "Ensayo_Proyecto2_202010799.docx");

    if (!File.Exists(ruta))
        return Results.NotFound("No se encontró la documentación.");

    return Results.File(
        ruta,
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "Ensayo_Proyecto2_202010799.docx");
});

app.Run();