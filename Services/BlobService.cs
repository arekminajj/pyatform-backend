using Azure.Storage.Blobs;
using pyatform.Services;

public class BlobService : IBlobService
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public BlobService(IConfiguration config)
    {
        _connectionString = config["AzureBlob:ConnectionString"]!;

        _containerName = config["AzureBlob:ContainerName"]!;
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var blobClient = new BlobContainerClient(_connectionString, _containerName);
        await blobClient.CreateIfNotExistsAsync();

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var blob = blobClient.GetBlobClient(fileName);

        using (var stream = file.OpenReadStream())
        {
            await blob.UploadAsync(stream, overwrite: true);
        }

        return blob.Uri.ToString();
    }
}
