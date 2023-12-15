using System.Text;
using cloudphoto;

namespace cloudphoto;

public static class Templates
{
    public static string GetIndexPage(List<Album> albumsList)
    {
        var sb = new StringBuilder();
        foreach (var album in albumsList)
        {
            sb.Append($"<li><a href={album.Href}>{album.Name}</a></li>");
        }

        var page = $"""
                    <!doctype html>
                    <html lang="ru">
                    <head>
                        <title>Фотоархив</title>
                        <meta charset="utf-8">
                    </head>
                    <body>
                    <h1>Фотоархив</h1>
                    <ul>
                            {sb.ToString()}
                    </ul>
                    </body>
                    </html>
                    """;

        return page;
    }

    public static string GetAlbumPage(List<Photo> photos)
    {
        var sb = new StringBuilder();
        foreach (var photo in photos)
        {
            sb.Append($"<img src={photo.Url} data-title={photo.Name}>");
        }
        
        var page = $$"""
                     <!doctype html>
                     <html>
                         <head>
                             <meta charset="utf-8">
                             <link rel="stylesheet" type="text/css" href="https://cdnjs.cloudflare.com/ajax/libs/galleria/1.6.1/themes/classic/galleria.classic.min.css" />
                             <style>
                                 .galleria{ width: 960px; height: 540px; background: #000 }
                             </style>
                             <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
                             <script src="https://cdnjs.cloudflare.com/ajax/libs/galleria/1.6.1/galleria.min.js"></script>
                             <script src="https://cdnjs.cloudflare.com/ajax/libs/galleria/1.6.1/themes/classic/galleria.classic.min.js"></script>
                         </head>
                         <body>
                             <div class="galleria">
                                 {{sb.ToString()}}
                             </div>
                             <p>Вернуться на <a href="index.html">главную страницу</a> фотоархива</p>
                             <script>
                                 (function() {
                                     Galleria.run('.galleria');
                                 }());
                             </script>
                         </body>
                     </html>
                     """;
        return page;
    }

    public static string GetErrorPage()
    {
        return """
               <!doctype html>
               <html>
                   <head>
                       <title>Фотоархив</title>
                       <meta charset="utf-8">
                   </head>
               <body>
                   <h1>Ошибка</h1>
                   <p>Ошибка при доступе к фотоархиву. Вернитесь на <a href="index.html">главную страницу</a> фотоархива.</p>
               </body>
               </html>
               """;
    }
}