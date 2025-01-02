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


            CreateMap<Department, DepartmentDTO>().ReverseMap();

            CreateMap<DoctorDetail, DoctorDetailDTO>().ReverseMap();

            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted ?? false))
                .ReverseMap()
                .ForMember(dest => dest.Email, opt => opt.Ignore()); // Prevent overwriting Email

            CreateMap<UserDTO, User>()
                .ForMember(dest => dest.Email, opt => opt.Ignore()) // Ensure Email isn't modified
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Patient, PatientDTO>().ReverseMap();

            CreateMap<Payment, PaymentDTO>().ReverseMap();

            CreateMap<ResetToken, ResetTokenDTO>().ReverseMap();

            CreateMap<DoctorAvailability, DoctorAvailabilityDTO>()
                .ForMember(dest => dest.UtcOffset, opt => opt.MapFrom(src => src.UtcOffset))
                .ReverseMap();

            CreateMap<Image, ImageDTO>().ReverseMap();

            CreateMap<PharmacyProduct, PharmacyProductDTO>().ReverseMap();

            CreateMap<Billing, BillingDTO>().ReverseMap();

            CreateMap<Order, OrderDTO>().ReverseMap();


            CreateMap<MedicalRecord, MedicalRecordDTO>().ReverseMap();

            CreateMap<PrescriptionDetail, PrescriptionDetailDTO>().ReverseMap();

            CreateMap<Disease, DiseaseDTO>().ReverseMap();

        }
    }
}
