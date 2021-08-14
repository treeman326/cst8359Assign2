using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab4.Data;
using Lab4.Models;
using Lab4.Models.ViewModels;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Azure;
using System.IO;

namespace Lab4.Controllers
{
    public class AdvertisementController : Controller
    {
        private readonly SchoolCommunityContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string containerName = "communityads";


        public AdvertisementController(SchoolCommunityContext context, BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
            _context = context;
        }
        public async Task<IActionResult> Index(string? ID)
        {
            var viewModel = new AdViewModel();
            viewModel.Community = await _context.Communities.FindAsync(ID);
            viewModel.Advertisements = await _context.Advertisements.ToListAsync();
            viewModel.Advertisements = viewModel.Advertisements.Where(x => x.CommunityId == ID);
            return View(viewModel);
        }

        public IActionResult Create(string ID, string title)
        {
            var viewModel = new CreateAdViewModel();
            viewModel.CommunityId = ID;
            viewModel.CommunityTitle = title;
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm]FileUploadViewModel adImage)
        {
            BlobContainerClient containerClient;
            try
            {
                containerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName);
                containerClient.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);
            }
            catch (RequestFailedException)
            {
                containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            }

            try
            {
                var blockBlob = containerClient.GetBlobClient(adImage.File.FileName);
                if (await blockBlob.ExistsAsync())
                {
                    await blockBlob.DeleteAsync();
                }
                using (var memoryStream = new MemoryStream())
                {
                    await adImage.File.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    await blockBlob.UploadAsync(memoryStream);
                    memoryStream.Close();
                }

                var image = new Advertisement();
                image.Url = blockBlob.Uri.AbsoluteUri;
                image.FileName = adImage.File.FileName;
                image.CommunityId = adImage.CommunityId;
                _context.Advertisements.Add(image);
                _context.SaveChanges();
            }
            catch (RequestFailedException)
            {
                View("Error");
            }

            return RedirectToAction("Index", new { ID = adImage.CommunityId });
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.Advertisements
                .FirstOrDefaultAsync(m => m.Id == id);
            if (image == null)
            {
                return NotFound();
            }

            return View(image);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var image = await _context.Advertisements.FindAsync(id);


            BlobContainerClient containerClient;
            // Get the container and return a container client object
            try
            {
                containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            }
            catch (RequestFailedException)
            {
                return View("Error");
            }

            try
            {
                // Get the blob that holds the data
                var blockBlob = containerClient.GetBlobClient(image.FileName);
                if (await blockBlob.ExistsAsync())
                {
                    await blockBlob.DeleteAsync();
                }

                _context.Advertisements.Remove(image); 
                await _context.SaveChangesAsync();

            }
            catch (RequestFailedException)
            {
                return View("Error");
            }

            return RedirectToAction("Index", new { ID = image.CommunityId });
        }
    }
}
