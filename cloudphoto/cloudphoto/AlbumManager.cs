using System.Net;
using System.Text;
using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;

namespace cloudphoto;

public class AlbumManager
{
    private readonly IAmazonS3 _s3Client;
    private readonly string? _bucketName;

    public AlbumManager(Config config)
    {
        var amazonS3Config = new AmazonS3Config
        {
            ServiceURL = config.Endpoint,
            AuthenticationRegion = config.Region
        };

        _s3Client = new AmazonS3Client(
            config.AccessKey,
            config.SecretKey,
            amazonS3Config
        );
        _bucketName = config.Bucket;
    }

    public async Task<int> Upload(UploadOptions options)
    {
        var album = options.Album;
        var path = options.Path ?? Directory.GetCurrentDirectory();

        FileAttributes attr = File.GetAttributes(path);  
  
        if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
        {
            Console.WriteLine($"Could not found directory with path {path}");
            return 1;
        }
        
        Console.WriteLine("Photo upload has started");
        
        var filesToUpload = Directory.GetFiles(path)
            .Where(fileName => fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg"));

        foreach (var filePath in filesToUpload)
        {
            var fileName = Path.GetFileName(filePath);
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = $"album/{album}/{fileName}",
                FilePath = filePath,
            };

            try
            {
                var response = await _s3Client.PutObjectAsync(request);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not upload file {fileName}");
            }
        }

        Console.WriteLine("Photo upload completed");

        return 0;
    }

    public async Task<int> Download(DownloadOptions options)
    {
        var album = options.Album;
        var path = options.Path ?? Directory.GetCurrentDirectory();
        
        FileAttributes attr = File.GetAttributes(path);
  
        if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
        {
            Console.WriteLine($"Could not found directory with path {path}");
            return 1;
        }
        
        Console.WriteLine($"Photo download has started");
        
        path = !path.EndsWith("/") ? path.SkipLast(1).ToString() : path;

        var listObjectsRequest = new ListObjectsRequest()
        {
            BucketName = _bucketName,
            Prefix = $"album/{album}/",
        };

        var objectsResponse = await _s3Client.ListObjectsAsync(listObjectsRequest);

        if (!objectsResponse.S3Objects.Any())
        {
            Console.WriteLine($"Album {album} does not exist");
            return 1;
        }
        
        foreach (var photo in objectsResponse.S3Objects)
        {
            var request = new GetObjectRequest()
            {
                BucketName = _bucketName,
                Key = photo.Key,
            };

            var response = await _s3Client.GetObjectAsync(request);
            var fileName = photo.Key.Split("/").Last();

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Could not download file {fileName}");
                return 1;
            }

            try
            {
                await response.WriteResponseStreamToFileAsync($"{path}/{fileName}", true, CancellationToken.None);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Path {path} access denied");
                return 1;
            }
        }

        Console.WriteLine($"Photo download completed");

        return 0;
    }

    public async Task<int> Delete(DeleteOptions options)
    {
        var album = options.Album;
        var photoName = options.Photo;

        var listObjectsRequest = new ListObjectsRequest()
        {
            BucketName = _bucketName,
            Prefix = $"album/",
        };

        var objectsResponse = await _s3Client.ListObjectsAsync(listObjectsRequest);

        if (!objectsResponse.S3Objects.Any())
        {
            Console.WriteLine($"Album {album} does not exist");
            return 1;
        }
        
        
        Console.WriteLine($"Photo deletion has started");

        if (!string.IsNullOrEmpty(photoName))
        {
            var deletePhotoRequest = new DeleteObjectRequest()
            {
                BucketName = _bucketName,
                Key = $"album/{album}/{photoName}"
            };

            try
            {
                var response = await _s3Client.DeleteObjectAsync(deletePhotoRequest);
                if (response.HttpStatusCode == HttpStatusCode.NoContent)
                {
                    Console.WriteLine($"Photo {photoName} does not exist");
                    return 1;
                }
            }
            catch
            {
                Console.WriteLine($"Could not delete file {photoName}");
                return 1;
            }

        }
        else
        {
            
            try
            {
                foreach (var photo in objectsResponse.S3Objects)
                {
                    var request = new DeleteObjectRequest()
                    {
                        BucketName = _bucketName,
                        Key = photo.Key
                    };

                    await _s3Client.DeleteObjectAsync(request);
                }
            }
            catch
            {
                Console.WriteLine($"Could not delete album {album}");
                return 1;
            }
        }

        Console.WriteLine($"Photo deletion completed");

        return 0;
    }

    public async Task<int> List(ListOptions options)
    {
        var album = options.Album;

        if (string.IsNullOrEmpty(album))
        {
            var listAlbumsRequest = new ListObjectsRequest()
            {
                BucketName = _bucketName,
                Prefix = $"album/",
            };

            var albumsResponse = await _s3Client.ListObjectsAsync(listAlbumsRequest);

            if (!albumsResponse.S3Objects.Any())
            {
                Console.WriteLine($"Albums does not exist");
                return 1;
            }

            var albumsList = albumsResponse.S3Objects
                .GroupBy(ob => ob.Key.Split("/")[1]);

            foreach (var name in albumsList)
            {
                Console.WriteLine($"/{name.Key}");
            }

            return 0;
        }

        var listObjectsRequest = new ListObjectsRequest()
        {
            BucketName = _bucketName,
            Prefix = $"album/{album}/",
        };

        var objectsResponse = await _s3Client.ListObjectsAsync(listObjectsRequest);

        if (!objectsResponse.S3Objects.Any())
        {
            Console.WriteLine($"Album {album} does not exist");
            return 1;
        }

        var photosList = objectsResponse.S3Objects
            .Select(ob => ob.Key.Split("/").Last());
        foreach (var name in photosList)
        {
            Console.WriteLine($"{name}");
        }

        return 0;
    }

    public async Task<int> MakeSite()
    {
        var listAlbumsRequest = new ListObjectsRequest()
        {
            BucketName = _bucketName,
            Prefix = $"album/",
        };

        var albumsListResponse = await _s3Client.ListObjectsAsync(listAlbumsRequest);

        if (!albumsListResponse.S3Objects.Any())
        {
            Console.WriteLine($"There are no albums to create a website");
            return 1;
        }

        var albumsList = albumsListResponse.S3Objects
            .GroupBy(ob => ob.Key.Split("/")[1])
            .Select((ob, index) => new Album($"album{index}.html", ob.Key))
            .ToList();


        var indexPage = Templates.GetIndexPage(albumsList);
        var indexPageBytes = Encoding.UTF8.GetBytes(indexPage);
        var indexPageStream = new MemoryStream(indexPageBytes);

        var createIndexPageRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = "index.html",
            InputStream = indexPageStream,
            CannedACL = S3CannedACL.PublicRead,
        };

        try
        {
            var response = await _s3Client.PutObjectAsync(createIndexPageRequest);
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not create website");
            return 1;
        }
        
        var errorPage = Templates.GetErrorPage();
        var errorPageBytes = Encoding.UTF8.GetBytes(errorPage);
        var errorPageStream = new MemoryStream(errorPageBytes);

        var createErrorPageRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = "error.html",
            InputStream = errorPageStream,
            CannedACL = S3CannedACL.PublicRead,
        };

        try
        {
            var response = await _s3Client.PutObjectAsync(createErrorPageRequest);
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not create website");
            return 1;
        }

        foreach (var album in albumsList)
        {
            var albumRequest = new ListObjectsRequest()
            {
                BucketName = _bucketName,
                Prefix = $"album/{album.Name}/",
            };

            var albumResponse = await _s3Client.ListObjectsAsync(albumRequest);

            if (!albumResponse.S3Objects.Any())
            {
                Console.WriteLine($"Album {album.Name} does not exist");
                return 1;
            }

            var photoList = albumResponse.S3Objects.Select(
                photo => new Photo(photo.Key.Split("/").Last(),
                    _s3Client.GeneratePreSignedURL(_bucketName, photo.Key, DateTime.Now.AddDays(5), null))
            ).ToList();
            
            var albumPage = Templates.GetAlbumPage(photoList);
            var albumPageBytes = Encoding.UTF8.GetBytes(albumPage);
            var albumPageStream = new MemoryStream(albumPageBytes);

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = album.Href,
                InputStream = albumPageStream,
                CannedACL = S3CannedACL.PublicRead,
            };

            try
            {
                var response = await _s3Client.PutObjectAsync(request);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new Exception();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not create website");
                return 1;
            }
        }

        var websiteCreateRequest = new PutBucketWebsiteRequest()
        {
            BucketName = _bucketName,
            WebsiteConfiguration = new WebsiteConfiguration()
            {
                IndexDocumentSuffix = "index.html",
                ErrorDocument = "error.html",
            },
        };
        var websiteCreateResponse = await _s3Client.PutBucketWebsiteAsync(websiteCreateRequest);
        if (websiteCreateResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            Console.WriteLine($"Could not create website");
            return 1;
        }
        Console.WriteLine($"http://{_bucketName}.website.yandexcloud.net/");
        
        return 0;
    }
}