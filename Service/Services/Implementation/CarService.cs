using AutoMapper;
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
        bool isPublic = false;

        public CarService(IMapper mapper, UnitOfWork unitOfWork, UploadFile upload)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _upload = upload;
        }

        public async Task<List<CarView>> GetAllCarsAsync()
        {
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

        public async Task<CarView> GetCarByIdAsync(Guid carId)
        {
            var car = await _unitOfWork._carRepo.GetByIdWithIncludeAsync(carId, "Id", x => x.Owner, x => x.PreferredLot, x => x.Images);
            var urls = await Task.WhenAll(car.Images.Select(
                    img => _upload.GetPublicUrlAsync(img.Bucket, img.FilePath)
                    ));
            var carView = _mapper.Map<CarView>(car);
            carView.ImageUrls.AddRange(urls);
            return carView;
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
                newCar.Id = Guid.NewGuid();
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

        public async Task<(string url, CarImage obj)> UploadCarImagesAsync(IFormFile file, Guid carId, int count)
        {
            try
            {
                string bucket = ConstantEnum.SupabaseBucket.CarImages;
                string uploadDate = DateTime.UtcNow.ToString("ddMMyyyy");

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
    }
}
