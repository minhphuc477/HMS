using AutoMapper;
using DataAL.Models;
using DataTransferO;

namespace BusinessLL
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AppointmentDTO, Appointment>()
                .ForMember(dest => dest.AppointmentDate,
                           opt => opt.MapFrom(src => DateTime.Parse(src.FormattedAppointmentDate)))
                .ForMember(dest => dest.UtcOffset, opt => opt.MapFrom(src => src.UtcOffset))
                .ForMember(dest => dest.Department, opt => opt.Ignore()) // Prevent User table insertion
                .ForMember(dest => dest.Patient, opt => opt.Ignore())    // Prevent User table insertion
                .ForMember(dest => dest.Doctor, opt => opt.Ignore())     // Prevent User table insertion
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId)) // Directly map PatientId
                .ReverseMap();

            CreateMap<DoctorDetailDTO, DoctorGridViewDTO>()
                .ForMember(dest => dest.DoctorId, opt => opt.MapFrom(src => src.DoctorId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.UserId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.User.DateOfBirth))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialization))
                .ForMember(dest => dest.Qualification, opt => opt.MapFrom(src => src.Qualification))
                .ForMember(dest => dest.ExperienceYears, opt => opt.MapFrom(src => src.ExperienceYears))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.DepartmentName))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User.IsActive))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.User.IsDeleted));

            CreateMap<UserDTO, DoctorGridViewDTO>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted));

            CreateMap<Department, DepartmentDTO>().ReverseMap();

            CreateMap<DoctorDetail, DoctorDetailDTO>()
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ReverseMap();

            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted ?? false))
                .ReverseMap()
                .ForMember(dest => dest.Email, opt => opt.Ignore()); // Prevent overwriting Email

            CreateMap<UserDTO, User>()
                 .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                 .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                 .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                 .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                 .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                 .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                 .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                 .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                 .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                 .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.PasswordHash))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Patient, PatientDTO>().ReverseMap();

            CreateMap<Payment, PaymentDTO>().ReverseMap();

            CreateMap<ResetToken, ResetTokenDTO>().ReverseMap();

            CreateMap<DoctorAvailability, DoctorAvailabilityDTO>()
                .ForMember(dest => dest.UtcOffset, opt => opt.MapFrom(src => src.UtcOffset))
                .ReverseMap();

            CreateMap<Image, ImageDTO>().ReverseMap();

            CreateMap<PharmacyProduct, PharmacyProductDTO>()
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.Stock))
                .ReverseMap();

            CreateMap<Billing, BillingDTO>().ReverseMap();

            CreateMap<Order, OrderDTO>()
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails))
                .ForMember(dest => dest.Department, opt => opt.Ignore()) // Add this line to map Department
                .ReverseMap();

            CreateMap<OrderDetail, OrderDetailDTO>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
                .ReverseMap();

            CreateMap<MedicalRecord, MedicalRecordDTO>().ReverseMap();


            CreateMap<PrescriptionDetail, PrescriptionDetailDTO>().ReverseMap();

            CreateMap<Disease, DiseaseDTO>().ReverseMap();

            CreateMap<Department, DepartmentDTO>().ReverseMap();

            CreateMap<Disease, DiseaseDTO>().ReverseMap();

            CreateMap<Doctor, DoctorDTO>().ReverseMap();
        }
    }
}
