using AutoMapper;
using DataTransferO;
using Serilog;
using System;
using System.Threading.Tasks;

namespace BusinessLL.AdminBLL
{
    public class DoctorManagementService
    {
        private readonly DoctorService _doctorService;
        private readonly UserService _userService;
        private readonly IMapper _mapper;

        public DoctorManagementService(DoctorService doctorService, UserService userService, IMapper mapper)
        {
            _doctorService = doctorService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task AddDoctorAsync(UserDTO userDto, DoctorDetailDTO doctorDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userDto.Email))
                {
                    throw new ArgumentException("Email cannot be empty.");
                }

                if (!_userService.IsValidEmail(userDto.Email))
                {
                    throw new ArgumentException("Invalid email format.");
                }

                userDto.UserId = Guid.NewGuid();
                userDto.PasswordHash = _userService.HashPassword("123be ");
                userDto.IsActive = true;
                userDto.IsDeleted = false;
                userDto.CreatedAt = DateTime.Now;

                Log.Information("UserDTO before saving: {@UserDto}", userDto);
                await _userService.CreateUserAsync(userDto);

                doctorDto.DoctorId = Guid.NewGuid();
                doctorDto.UserId = userDto.UserId;
                doctorDto.Phone = userDto.PhoneNumber;

                Log.Information("DoctorDetailDTO before saving: {@DoctorDto}", doctorDto);
                await _doctorService.CreateDoctorAsync(doctorDto);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add doctor");
                throw;
            }
        }

        public async Task UpdateDoctorAsync(DoctorDetailDTO doctorDto)
        {
            try
            {
                // Ensure the User exists before updating DoctorDetail
                var user = await _userService.GetUserByIdAsync(doctorDto.UserId);
                if (user == null)
                {
                    throw new Exception("User not found. Please ensure the user exists before updating the doctor.");
                }

                doctorDto.Phone = user.PhoneNumber; // Ensure the PhoneNumber is set

                await _doctorService.UpdateDoctorAsync(doctorDto);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update doctor");
                throw;
            }
        }
    }
}
