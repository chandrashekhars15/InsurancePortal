using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using InsuranceClientPortal.Models;
using System.IO;
using InsuranceClientPortal.Helpers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace InsuranceClientPortal.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration configuration;

        public HomeController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create (CustomerViewModel customer)
        {
            if (ModelState.IsValid)
            {
                var customerId = Guid.NewGuid().ToString();
                var tempFile = Path.GetTempFileName();
                using(var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                {
                    await customer.Image.CopyToAsync(fs);
                }

                var fileName = Path.GetFileName(customer.Image.FileName);
                var tempPath = Path.GetDirectoryName(tempFile);
                var imagePath = Path.Combine(tempPath, string.Concat(customerId, "_", fileName));
                System.IO.File.Move(tempFile, imagePath);

                StorageHelper storageHelper = new StorageHelper();
                storageHelper.StorageConnectionString = configuration.GetConnectionString("StorageConnection");
                var imageUri = await storageHelper.UploadFileAsync(imagePath, "images");

                Customer customerEntity = new Customer(customerId, customer.InsuranceType);
                customerEntity.Amount = customer.Amount;
                customerEntity.Email = customer.Email;
                customerEntity.Name = customer.Name;
                customerEntity.Premium = customer.Premium;
                customerEntity.AppDate = customer.AppDate;
                customerEntity.EndDate = customer.EndDate;
                customerEntity.ImageUrl = imageUri;
                Customer newCustomer = await storageHelper.SaveInsuranceDetailAsync(customerEntity, "customers");

                string messageText = JsonConvert.SerializeObject(newCustomer);
                await storageHelper.SendMessageAsync(messageText, "insurance-queue");
            }
            else
            {
                //error
            }

            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
