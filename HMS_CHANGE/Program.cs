using AutoMapper;
using BusinessLL;
using DataAL.Models;
using HMS_CHANGE.Dashboard;
using HMS_CHANGE.Patient;
using HMS_CHANGE.Patient.Account_Setting;
using HMS_CHANGE.Patient.Appointment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.Http;
using HMS_CHANGE.Doctor;
using BusinessLL.AdminBLL;
using HMS_CHANGE.Admin;

namespace HMS_CHANGE
{
    internal static class Program
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        [STAThread]
        static void Main()
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting application");
                ConfigureServices();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var mainBoard = ServiceProvider!.GetRequiredService<MainBoard>();
                Application.Run(mainBoard);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            // Register DbContext with Scoped lifetime
            services.AddDbContext<HmsAContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("HMS_A")),
                ServiceLifetime.Scoped);

            // Register DbContextFactory
            services.AddDbContextFactory<HmsAContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("HMS_A")));

            services.AddAutoMapper(typeof(AutoMapperProfile));

            services.AddTransient<AuditLogService>();

            services.AddTransient<UserService>(provider =>
            {
                var context = provider.GetRequiredService<HmsAContext>();
                var mapper = provider.GetRequiredService<IMapper>();
                var auditLogService = provider.GetRequiredService<AuditLogService>();
                return new UserService(context, mapper, auditLogService);
            });

            services.AddTransient<MainBoard>();
            services.AddTransient<HMS_CHANGE.Dashboard.SignUp>();
            services.AddTransient<DashBoardAdmin>();
            services.AddTransient<DashBoardDoctor>();
            services.AddTransient<DashBoardPatient_GetStart>();

            services.AddTransient<ImageService>(provider =>
            {
                var context = provider.GetRequiredService<HmsAContext>();
                var mapper = provider.GetRequiredService<IMapper>();
                return new ImageService(context, mapper);
            });

            services.AddSingleton<EmailService>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var gmailAddress = configuration["Gmail:Address"];
                var gmailPassword = configuration["Gmail:Password"];

                if (string.IsNullOrEmpty(gmailAddress) || string.IsNullOrEmpty(gmailPassword))
                {
                    throw new InvalidOperationException("Gmail credentials are missing in the configuration. Please check appsettings.json.");
                }

                return new EmailService(gmailAddress, gmailPassword);
            });

            services.AddTransient<DoctorService>(provider =>
            {
                var contextFactory = provider.GetRequiredService<IDbContextFactory<HmsAContext>>();
                var mapper = provider.GetRequiredService<IMapper>();
                var doctorService = new DoctorService(contextFactory, mapper);
                // Initialize availability asynchronously
                var startTime = new TimeSpan(7, 0, 0); // 7:00 AM
                var endTime = new TimeSpan(17, 0, 0); // 5:00 PM
                var slotDuration = TimeSpan.FromMinutes(30); // 30-minute slots
                var utcOffset = 0; // Assuming UTC+7 for Vietnam time zone
                Task.Run(async () =>
                {
                    try
                    {
                        await doctorService.GenerateAvailabilityForAllDoctorsAsync(startTime, endTime, slotDuration, utcOffset);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error generating doctor availability");
                    }
                }).Wait();
                return doctorService;
            });

            services.AddTransient<AppointmentService>(provider =>
            {
                var contextFactory = provider.GetRequiredService<IDbContextFactory<HmsAContext>>();
                var validationService = provider.GetRequiredService<AppointmentValidationService>();
                var mapper = provider.GetRequiredService<IMapper>();
                return new AppointmentService(contextFactory, validationService, mapper);
            });

            services.AddTransient<AppointmentValidationService>();

            services.AddTransient<TypeInVerifyToken>();
            services.AddTransient<ResetPassword>();
            services.AddTransient<ForgetPass>();
            services.AddTransient<BookAppointment>();
            services.AddTransient<UpcomingApp>();
            services.AddTransient<RescheduleAppointment>();
            services.AddTransient<PdfGenerationService>();
            services.AddTransient<PdfViewerForm>();
            services.AddTransient<ForwardService>();
            services.AddTransient<AccountSettingDashboard>(provider =>
            {
                var userService = provider.GetRequiredService<UserService>();
                var imageService = provider.GetRequiredService<ImageService>();
                var serviceProvider = provider.GetRequiredService<IServiceProvider>();
                return new AccountSettingDashboard(userService, imageService, serviceProvider);
            });

            services.AddTransient<BillingApp>();
            services.AddTransient<PaymentAndBillingService>();
            services.AddTransient<PaymentByApp>();
            services.AddTransient<SePayService>();

            // Register HospitalStatisticService
            services.AddTransient<HospitalStatisticService>();

            services.AddTransient<DetailsAppointment>();
            services.AddTransient<CheckUpService>();
            services.AddTransient<OrderAppointment>();
            services.AddTransient<AccountDoc>();
            services.AddTransient<DoctorManagementService>();

            services.AddTransient<DepartmentAssign>();
            services.AddTransient<AppointmentStats>();
            services.AddTransient<RevenuStats>();
            services.AddTransient<AccountAdmin>();

            // Inside ConfigureServices method
            //services.AddSingleton<ISePayService, SePayService>();
            //services.AddHttpClient<ISePayService, SePayService>(); // If additional configuration is needed

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}
