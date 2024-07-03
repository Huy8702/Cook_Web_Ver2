using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;
using CookWeb_Ver2.Data;
using CookWeb_Ver2.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.PortableExecutable;
using CookWeb_Ver2.Ultilities;

public class Program
{
    // Add your static public variable here

    public static bool flagReady = true;
    public static bool[] isPauseCook = new bool[3] { false, false, false };
    public static TcpClient? Client;
    public static string? statusConnect;
    public static bool isConnected;
    public static Timer? timer;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var connectionString = builder.Configuration.GetConnectionString("DataContext") ?? throw new InvalidOperationException("Connection string 'DataContext' not found.");
        
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = false;
        });
        builder.Services.AddControllersWithViews();
        //builder.Services.AddHostedService<TCPSenderService>();

        var app = builder.Build();

        #region Code phần tạo một task background gửi dữ liệu theo luồng
        CancellationToken stoppingToken = new CancellationToken();
        // Invoke your task here
        Task.Run(async () =>
        {
            //Toc do truyen tin dvt: ms
            int speed = 300;

            try
            {
                Program.Client = new TcpClient("192.168.0.7", 302);

                if (Program.Client.Connected)
                {
                    Program.statusConnect = "Connection successfully!";
                    Program.isConnected = true;
                }
                else
                {
                    // Log the fact that the connection attempt failed
                    Console.WriteLine("Failed to connect to PLC");
                    Program.statusConnect = "Failed to connect to PLC";
                    Program.isConnected = false;
                }
            }
            catch (SocketException ex)
            {
                // Log the socket exception for debugging purposes
                Console.WriteLine($"SocketException: {ex.Message}");
                Program.statusConnect = $"SocketException: {ex.Message}";
                Program.isConnected = false;
            }
            catch (Exception ex)
            {
                // Log other exceptions for debugging purposes
                Console.WriteLine($"An error occurred: {ex.Message}");
                Program.statusConnect = $"An error occurred: {ex.Message}";
                Program.isConnected = false;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                if (Program.Client != null && Program.isConnected)
                {
                    using (var scope = app.Services.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var machines = dbContext.Machines.Where(p => p.MachineTypeId == 2).OrderBy(n => n.MachineId).ToList();

                        if (machines[0].IsCooking)
                        {
                            // Gửi dữ liệu qua TCP
                            await Program.Client.GetStream().WriteAsync(IoTManager.dataMachine01, 0, IoTManager.dataMachine01.Length, stoppingToken);
                            await Task.Delay(speed, stoppingToken);
                        }
                        if (machines[1].IsCooking)
                        {
                            // Gửi dữ liệu qua TCP
                            await Program.Client.GetStream().WriteAsync(IoTManager.dataMachine02, 0, IoTManager.dataMachine02.Length, stoppingToken);
                            await Task.Delay(speed, stoppingToken);
                        }
                        if (machines[2].IsCooking)
                        {
                            // Gửi dữ liệu qua TCP
                            await Program.Client.GetStream().WriteAsync(IoTManager.dataMachine03, 0, IoTManager.dataMachine03.Length, stoppingToken);
                            await Task.Delay(speed, stoppingToken);
                        }
                        if (!Program.flagReady)
                        {
                            IoTManager.counterCommon++;
                            IoTManager.counterCommon %= 3;
                        }
                    }
                }
            }


        }, stoppingToken);
        #endregion

        #region read data from plc
        CancellationToken stoppingTokenRead = new CancellationToken();
        Task.Run(async () =>
        {
            //Mang de nhan du lieu
            byte[] read_buffer = new byte[14];

            SetFrameDataTobeSent _frameDataTobeSent = new SetFrameDataTobeSent();

            while (!stoppingTokenRead.IsCancellationRequested)
            {
                if (Program.Client != null && Program.Client.Connected)
                {
                    NetworkStream streamRead = Program.Client.GetStream();

                    int bytesRead = await streamRead.ReadAsync(read_buffer, 0, read_buffer.Length);

                    // Check if any bytes were read
                    if (bytesRead > 0 && _frameDataTobeSent.VerifyChecksum(read_buffer) && read_buffer[2] == 0x01 && read_buffer[4] == 0x0c)
                    {
                        byte updatedMachineIndex = (byte)(read_buffer[0] - 1);
                        //update status of each machine when a data received from plc

                        if (checkDataReceived(read_buffer, 37, updatedMachineIndex))
                            IoTManager.IsNLCall[updatedMachineIndex] = true;


                        else if (checkDataReceived(read_buffer, 1, updatedMachineIndex) && !IoTManager.isCalib[updatedMachineIndex])
                            IoTManager.isCalib[updatedMachineIndex] = true;


                        else if (checkDataReceived(read_buffer, 34, updatedMachineIndex))
                            IoTManager.isCallTP[updatedMachineIndex] = true;


                        else if (checkDataReceived(read_buffer, 40, updatedMachineIndex))
                            IoTManager.TraTPDone[updatedMachineIndex] = true;


                        else if (checkDataReceived(read_buffer, 43, updatedMachineIndex))
                            IoTManager.NL_traDone[updatedMachineIndex] = true;


                        else if (checkDataReceived(read_buffer, 1, updatedMachineIndex) && IoTManager.XitRuaDone[updatedMachineIndex] &&
                        IoTManager.TraTPDone[updatedMachineIndex] && IoTManager.NL_traDone[updatedMachineIndex])
                            IoTManager.CalibEndDone[updatedMachineIndex] = true;


                        else if (checkDataReceived(read_buffer, 46, updatedMachineIndex) && IoTManager.TraTPDone[updatedMachineIndex])
                            IoTManager.XitRuaDone[updatedMachineIndex] = true;
                    }
                }
            }
        }, stoppingTokenRead);
        #endregion

        //Timer check món gọi vào một hàm trong iotmanager
        timer = new System.Threading.Timer(TimerCallback, app, TimeSpan.Zero, TimeSpan.FromSeconds(2));

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
    private static void TimerCallback(object? state)
    {
        if (state != null)
        {
            WebApplication? app = state as WebApplication;
            if (app != null)
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var iotManager = new IoTManager(dbContext);
                    iotManager.CheckOrders();
                }
            }
        }
    }

    private static bool checkDataReceived(byte[] data, int actionCodeIndex, byte machineIndex)
    {
        byte[] ref_frame = new byte[18];

        switch (machineIndex)
        {
            case 0:
                ref_frame = IoTManager.dataMachine01;
                break;
            case 1:
                ref_frame = IoTManager.dataMachine02;
                break;
            case 2:
                ref_frame = IoTManager.dataMachine03;
                break;
        }

        return data[6] == (actionCodeIndex + machineIndex) && ref_frame[4] == data[6] && data[8] == 0x01;
    }
}
