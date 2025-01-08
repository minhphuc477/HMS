using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DataAL.Models;
using DataTransferO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DrawingImage = System.Drawing.Image; // Alias for System.Drawing.Image

namespace BusinessLL
{
    public class ImageService
    {
        private readonly HmsAContext _context;
        private readonly IMapper _mapper;

        public ImageService(HmsAContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Inserts an image into the database for a specific entity.
        /// </summary>
        public async Task<Guid> InsertImageAsync(Guid entityId, string entityType, byte[] imageData)
        {
            var image = new Image
            {
                ImageId = Guid.NewGuid(),
                EntityId = entityId,
                EntityType = entityType,
                ImageData = imageData,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return image.ImageId;
        }

        /// <summary>
        /// Retrieves the binary data of an image by its ID.
        /// </summary>
        public async Task<ImageDTO> GetImageByIdAsync(Guid imageId)
        {
            var image = await _context.Images.FindAsync(imageId);
            if (image == null)
            {
                throw new Exception("Image not found.");
            }

            return _mapper.Map<ImageDTO>(image);
        }

        /// <summary>
        /// Updates an image for a specific entity.
        /// </summary>
        public async Task UpdateImageAsync(ImageDTO imageDto)
        {
            var existingImage = await _context.Images.FindAsync(imageDto.ImageId);
            if (existingImage == null)
            {
                throw new Exception("Image not found.");
            }

            if (imageDto.ImageData == null)
            {
                throw new ArgumentException("Image data cannot be null.");
            }

            _mapper.Map(imageDto, existingImage);
            existingImage.UpdatedAt = DateTime.UtcNow;

            _context.Images.Update(existingImage);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes an image from the database.
        /// </summary>
        public async Task DeleteImageAsync(Guid imageId)
        {
            var image = await _context.Images.FindAsync(imageId);
            if (image == null)
            {
                throw new Exception("Image not found.");
            }

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Validates if the file is an acceptable image format.
        /// </summary>
        public bool IsValidImageFile(string filePath)
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            string fileExtension = Path.GetExtension(filePath).ToLower();
            return validExtensions.Contains(fileExtension);
        }

        /// <summary>
        /// Converts binary image data to a System.Drawing.Image object (used for UI handling in the PL).
        /// </summary>
        public DrawingImage ConvertToImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                throw new ArgumentException("Image data is invalid or empty.");
            }

            using (var ms = new MemoryStream(imageData))
            {
#if WINDOWS
                    return DrawingImage.FromStream(ms);
#else
                throw new PlatformNotSupportedException("Image conversion is only supported on Windows.");
#endif
            }
        }

        /// <summary>
        /// Adds an image for a user.
        /// </summary>
        public async Task<Guid> AddUserImageAsync(Guid userId, byte[] imageData)
        {
            return await InsertImageAsync(userId, "Patient", imageData);
        }

        /// <summary>
        /// Adds an image for a doctor.
        /// </summary>
        public async Task<Guid> AddDoctorImageAsync(Guid doctorId, byte[] imageData)
        {
            return await InsertImageAsync(doctorId, "Doctor", imageData);
        }

        /// <summary>
        /// Adds an image for a product.
        /// </summary>
        public async Task<Guid> AddProductImageAsync(Guid productId, byte[] imageData)
        {
            return await InsertImageAsync(productId, "Product", imageData);
        }

        /// <summary>
        /// Adds an image for another entity type.
        /// </summary>
        public async Task<Guid> AddOtherImageAsync(Guid entityId, byte[] imageData)
        {
            return await InsertImageAsync(entityId, "Other", imageData);
        }

        public async Task<ImageDTO> GetImageByEntityIdAsync(Guid entityId)
        {
            var image = await _context.Images.FirstOrDefaultAsync(i => i.EntityId == entityId);
            if (image == null)
            {
                throw new Exception("Image not found.");
            }

            return _mapper.Map<ImageDTO>(image);
        }

        public async Task<Guid> AddOrUpdateUserImageAsync(Guid userId, byte[] imageData)
        {
            var existingImage = await _context.Images.FirstOrDefaultAsync(i => i.EntityId == userId && i.EntityType == "Patient");
            Image newImage = null;

            if (existingImage != null)
            {
                // Update the existing image
                existingImage.ImageData = imageData;
                existingImage.UpdatedAt = DateTime.UtcNow;
                _context.Images.Update(existingImage);
            }
            else
            {
                // Insert a new image
                newImage = new Image
                {
                    ImageId = Guid.NewGuid(),
                    EntityId = userId,
                    EntityType = "Patient",
                    ImageData = imageData,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Images.Add(newImage);
            }

            await _context.SaveChangesAsync();
            return existingImage?.ImageId ?? newImage.ImageId;
        }

        public async Task<Guid> AddOrUpdateDoctorImageAsync(Guid doctorId, byte[] imageData)
        {
            var existingImage = await _context.Images.FirstOrDefaultAsync(i => i.EntityId == doctorId && i.EntityType == "Doctor");
            Image newImage = null;

            if (existingImage != null)
            {
                // Update the existing image
                existingImage.ImageData = imageData;
                existingImage.UpdatedAt = DateTime.UtcNow;
                _context.Images.Update(existingImage);
            }
            else
            {
                // Insert a new image
                newImage = new Image
                {
                    ImageId = Guid.NewGuid(),
                    EntityId = doctorId,
                    EntityType = "Doctor",
                    ImageData = imageData,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Images.Add(newImage);
            }

            await _context.SaveChangesAsync();
            return existingImage?.ImageId ?? newImage.ImageId;
        }
    }
}

