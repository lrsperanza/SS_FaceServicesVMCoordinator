// See https://aka.ms/new-console-template for more information
using Azure.Core;
using Azure.Storage.Queues;
using EllipticCurve.Utils;
using SharedServerSide.Microservices;
using SharedServerSide.Storage;
using SharedServerSide.Storage.Blobs.BlobOperations;
using SharedServerSide.Storage.Queues;
using SharedServerSide.Storage.Tables.TableEntities;
using SharedServerSide.Tables;
using SkiaSharp;
using System.Diagnostics;
using System.Reflection;
using System.Text;

Console.WriteLine("Starting");

string creationDateString = "";
try
{
	var creationDate = System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
	creationDateString = creationDate.ToShortDateString() + " " + creationDate.ToShortTimeString();
}
catch
{

}


while (true)
{
	try
	{
		Console.WriteLine("Running check loop. Built in: " + creationDateString);
		bool success = await Run();
		if (!success) 
			await Task.Delay(1000);

	} catch (Exception ex)
	{
		Console.Write(ex.ToString());
		Console.Write(ex.StackTrace);
		await Task.Delay(1000);
	}
}

//async Task<bool> Run()
//{
//    Stopwatch sw = new Stopwatch();
//    sw.Start();

//    var message = await QueueOperations.PhotosToBeRecQueue.ReceiveMessageAsync();
//    if (message == null)
//        return false;
//    if (message.Value == null)
//        return false;
//    if (message.Value.DequeueCount > 5)
//    {
//        await QueueOperations.PhotosToBeRecQueue.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
//        return false;
//    }

//    Console.WriteLine($"Running rec for: {message.Value.Body.ToString()}");
//    var parts = message.Value.Body.ToString().Split('?');
//    if (parts.Length < 2)
//        return false;
//    var pk = parts[0];
//    var rk = parts[1];
//    var success = await RunPTE(pk, rk);
//    if(success)
//        await QueueOperations.PhotosToBeRecQueue.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);

//    return success;
//}


async Task<bool> Run()
{
    Stopwatch sw = new Stopwatch();
    sw.Start();

    var message = await QueueOperations.PhotosToBeRecQueue.ReceiveMessageAsync();
    if (message == null)
        return false;
    if (message.Value == null)
        return false;
    if (message.Value.DequeueCount > 5)
    {
        await QueueOperations.PhotosToBeRecQueue.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
        return false;
    }

    Console.WriteLine($"Running rec for: {message.Value.Body.ToString()}");
    var parts = message.Value.Body.ToString().Split('?');
    if (parts.Length < 2)
        return false;
    var pk = parts[0];
    var rk = parts[1];
    var success = await RunPTE(pk, rk);
    if (success)
        await QueueOperations.PhotosToBeRecQueue.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);

    return success;
}

async Task<bool> RunPTE(string pk, string rk)
    {
        var pte = await TableOperations.GetPhotoTableEntity(pk, rk);
        if (pte == null)
            return false;
        var ptte = await TableOperations.GetProfessionalTaskTableEntity(pte.HiringCompany, pte.ClassCode);


        var reducedContainerDestination = BlobOperations.GetPhotoContainerClient(ptte, pte, BlobOperations.PhotoStorageQualities.REDUCED);

        var blobClient = reducedContainerDestination.GetBlobClient(pte.BlobReduced);
        byte[] bytes;


        using (var ms = new MemoryStream())
        {
            await blobClient.DownloadToAsync(ms);
            bytes = ms.ToArray();
        }

        await StorageOperations.RunFaceServicesAndSaveResultsToBlobStorageAndPhotoTableEntity(bytes, ptte.EnableFaceRelevanceDetection == true, pte, reducedContainerDestination, true);

        await TableOperations.PhotosTable.UpsertEntityAsync(pte);

        return true;
    }



