using AutoMapper;
using Medo;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Repository.Base;
using Repository.Constant;
using Repository.CustomFunctions.SupabaseFileUploader;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Car;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.DTO.ResponseDTO.Car;
using Repository.DTO.ResponseDTO.Feedbacks;
using Repository.Extension.SupabaseFileUploader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class CarService : ICarService
    {
        private readonly IMapper _mapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly UploadFile _upload;

        int expirationTimeinSeconds = 1800;
        bool isPublic = true;

        public CarService(IMapper mapper, UnitOfWork unitOfWork, UploadFile upload)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _upload = upload;
        }

        public async Task<List<CarView>> GetAllCarsAsync()
        {
            await _upload.EnsureInitializedAsync();

            var cars = await _unitOfWork._carRepo.GetAllCars();
            var carViews = new List<CarView>();
            foreach(var car in cars)
            {
                var urls = await Task.WhenAll(car.Images.Select(
                    img => _upload.GetPublicUrlAsync(img.Bucket, img.FilePath)
                    ));
                var carView = _mapper.Map<CarView>(car);
                carView.ImageUrls.AddRange(urls);
                carViews.Add(carView);
            }
            return carViews;
        }

        public async Task<List<CarView>> GetActiveCarsAsync()
        {
            await _upload.EnsureInitializedAsync();

            var cars = await _unitOfWork._carRepo.GetAllActiveCars();
            var carViews = new List<CarView>();
            foreach (var car in cars)
            {
                var urls = await Task.WhenAll(car.Images.Select(
                    img => _upload.GetPublicUrlAsync(img.Bucket, img.FilePath)
                    ));
                var carView = _mapper.Map<CarView>(car);
                carView.ImageUrls.AddRange(urls);
                carViews.Add(carView);
            }
            return carViews;
        }

        public async Task<CarView> GetCarByIdAsync(Guid carId)
        {
            await _upload.EnsureInitializedAsync();
            var car = await _unitOfWork._carRepo.GetByIdWithIncludeAsync(carId, "Id", 
                x => x.Owner,
                x => x.PreferredLot,
                x => x.RentalRate,
                x => x.Images.Where(img => img.Status.Equals(ConstantEnum.Statuses.ACTIVE)));
            /*var images = car.Images
                .GroupBy(img => img.FilePath.Split('_')[0]) //image1, image2
                .Select(g => g.OrderByDescending(i => i.CreateDate).First())
                .OrderBy(i => i.FilePath)
                .ToList();*/
            var urls = await Task.WhenAll(car.Images.Select(
                    img => _upload.GetPublicUrlAsync(img.Bucket, img.FilePath)
                    ));
            var carView = _mapper.Map<CarView>(car);
            carView.ImageUrls.AddRange(urls);
            return carView;
        }

        public async Task<List<CarImage>> OverwritePrevCarImagesAsync(Guid carId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _upload.EnsureInitializedAsync();
                var car = await _unitOfWork._carRepo.GetByIdWithIncludeAsync(carId, "Id",
                    x => x.Images);

                foreach(var i in car.Images)
                {
                    i.Status = ConstantEnum.Statuses.INACTIVE;
                }
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return car.Images.ToList();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<(string status, CarView car)> RegisterCarAsync(CarInfoForm form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if (!form.UserId.HasValue && string.IsNullOrEmpty(form.Username))
                {
                    return ("Owner name or id is required", null);
                }
                else if (!form.UserId.HasValue)
                {
                    var owner = await _unitOfWork._userRepo.GetUserByUsernameAsync(form.Username);
                    if (owner == null) throw new InvalidDataException("No user found!");
                    form.UserId = owner.Id;
                }
                if (!form.PrefLotId.HasValue && string.IsNullOrEmpty(form.PrefLotName))
                {
                    return ("Owner name or id is required", null);
                }
                else if (!form.PrefLotId.HasValue)
                {
                    var lot = await _unitOfWork._lotRepo.GetLotByNameAsync(form.PrefLotName);
                    if (lot == null) throw new InvalidDataException("No lot found!");
                    form.PrefLotId = lot.Id;
                }

                var newCar = _mapper.Map<Car>(form);

                newCar.Status = ConstantEnum.Statuses.PENDING;
                newCar.Id = Uuid7.NewGuid();
                newCar.Rating = 0.0;
                var result = await _unitOfWork._carRepo.RegisterCarAsync(newCar);

                /* //Sequential uploads
                List<string> urls = new List<string>();
                int count = 1;
                foreach (var file in form.Medias)
                {
                    var url = await UploadCarImagesAsync(file, newCar.Id, count);
                    if (url.url.IsNullOrEmpty() || url.obj == null) throw new Exception("File upload failure!");
                    urls.Add(url.url);
                    count++;
                }
                */

                //Parallel uploads
                var uploadTasks = new List<Task<(string url, CarImage obj)>>();
                int count = 1;

                await _upload.EnsureInitializedAsync();

                foreach (var file in form.Medias)
                {
                    uploadTasks.Add(UploadCarImagesAsync(file, newCar.Id, count));
                    count++;
                }

                var uploadResults = await Task.WhenAll(uploadTasks);

                var urls = uploadResults.Select(r =>
                {
                    if (r.url.IsNullOrEmpty() || r.obj == null) throw new Exception("File upload failure!");
                    return r.url;
                }).ToList();

                foreach (var u in uploadResults)
                {
                    await _unitOfWork._carImageRepo.AddCarImageAsync(u.obj);
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var info = await _unitOfWork._carRepo.GetByIdWithIncludeAsync(result.car.Id, "Id", x => x.Owner, x => x.PreferredLot);

                if (result.status.Equals(ConstantEnum.RepoStatus.FAILURE))
                {
                    return (result.status, null);
                }
                else
                {
                    var mapped = _mapper.Map<CarView>(info);
                    mapped.ImageUrls.AddRange(urls);
                    return (result.status, mapped);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<CarView> UpdateCarImageAsync(List<IFormFile> images, Guid carId)
        {
            try
            {
                //var car = await _unitOfWork._carRepo.GetByIdWithIncludeAsync(carId, "Id", x => x.Owner, x => x.PreferredLot, x => x.Images);
                var car = await _unitOfWork._carRepo.GetByIdAsync(carId);

                if (car == null) throw new KeyNotFoundException("Car not found!");

                //var existImage = await _unitOfWork._carImageRepo.GetAllAsync();
                await OverwritePrevCarImagesAsync(car.Id);

                await _unitOfWork.BeginTransactionAsync();

                var uploadTasks = new List<Task<(string url, CarImage obj)>>();
                //int count = existImage.Count();
                int count = 1;

                await _upload.EnsureInitializedAsync();

                foreach (var file in images)
                {
                    uploadTasks.Add(UploadCarImagesAsync(file, carId, count));
                    count++;
                }

                var uploadResults = await Task.WhenAll(uploadTasks);

                var urls = uploadResults.Select(r =>
                {
                    if (r.url.IsNullOrEmpty() || r.obj == null) throw new Exception("File upload failure!");
                    return r.url;
                }).ToList();

                foreach (var u in uploadResults)
                {
                    await _unitOfWork._carImageRepo.AddCarImageAsync(u.obj);
                }
                var result = await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var info = await _unitOfWork._carRepo.GetByIdWithIncludeAsync(carId, "Id", x => x.Owner, x => x.PreferredLot);

                if (info == null) return null;

                var mapped = _mapper.Map<CarView>(info);
                mapped.ImageUrls.AddRange(urls);
                return mapped;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }  
        }

        public async Task<(string url, CarImage obj)> UploadCarImagesAsync(IFormFile file, Guid carId, int count)
        {
            try
            {
                string bucket = ConstantEnum.SupabaseBucket.CarImages;
                string uploadDate = DateTime.UtcNow.AddHours(7).ToString("ddMMyyyy");

                string originalExt = Path.GetExtension(file.FileName).ToLowerInvariant();
                string fileName = $"image{count}_{uploadDate}{originalExt}"; //abc-cde-def_01011990.png
                string imagePath = $"{carId}/{fileName}"; //userid/carid_date.ext

                var url = await _upload.UploadImageAsync(file, fileName, imagePath, bucket, expirationTimeinSeconds, isPublic);

                if (url.IsNullOrEmpty()) throw new Exception("File upload failure!");

                var carImage = new CarImage
                {
                    FilePath = imagePath,
                    FileName = fileName,
                    Bucket = bucket,
                    CreateDate = DateTime.UtcNow,
                    MimeType = MimeTypeHelper.GetMimeType(originalExt),
                    FileSize = file.Length,
                    Status = ConstantEnum.Statuses.ACTIVE,
                    CarId = carId
                };

                return (url, carImage);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<List<CarDetailsManufacturer>> GetManufacturerLookup()
        {
            return await _unitOfWork._lookupRepo.GetCarDetailsManufacturer();
        }

        public async Task<List<CarDetailsModel>> GetModelLookupOfManufacturer(int manufacturerId)
        {
            return await _unitOfWork._lookupRepo.GetCarDetailsModelByManufacturer(manufacturerId);
        }

        public async Task<CarView> ChangeCarStatusAsync(CarStatusChange form)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                Car carExist = new Car();

                if (!form.LicensePlate.IsNullOrEmpty()) carExist = await _unitOfWork._carRepo.GetCarByLicensePlate(form.LicensePlate);
                else if (!form.carId.HasValue) carExist = await _unitOfWork._carRepo.GetByIdAsync(form.carId.Value);
                else throw new InvalidOperationException("Must fill in one field!");

                if (carExist == null) throw new KeyNotFoundException("No car with the given number or Id found!");

                if (form.isActive)
                {
                    //if(carExist.Status.Equals(ConstantEnum.Statuses.RESERVED))
                    carExist.Status = ConstantEnum.Statuses.ACTIVE;
                }
                else
                {
                    carExist.Status = ConstantEnum.Statuses.INACTIVE;
                }

                await _unitOfWork._carRepo.UpdateCarAsync(carExist);
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<CarView>(carExist);

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }

        public Task<List<CarView>> SearchCarAsync(SearchCarForm searchParam)
        {
            throw new NotImplementedException();
        }

        public async Task<CarView> UpdateCarAsync(Guid carId, UpdateCarForm form)
        {
            try
            {
                var carExist = await _unitOfWork._carRepo.GetByIdAsync(carId);
                if (carExist == null) throw new KeyNotFoundException("No car with this Id found");

                var updateObject = _mapper.Map(form, carExist);

                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork._carRepo.UpdateCarAsync(carExist);
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<CarView>(carExist);
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
