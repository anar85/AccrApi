using AccrServicesApi.BackgroundServices;
using AccrServicesApi.Data.JobRepository;
using AccrServicesApi.Data.Logs;
using AccrServicesApi.Middleware;
using AccrServicesApi.Models.Settings;
using AccrServicesApi.Services.ApiClient;
using AccrServicesApi.Services.JobService;
using AccrServicesApi.Services.JwtService;
using AccrServicesApi.Services.ViewService;
using NLog.Extensions.Logging;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Logging.ClearProviders();
builder.Host.UseNLog();
//builder.Logging.AddNLog();
builder.Services.AddControllers();
	//	.AddJsonOptions(o =>
	//	{
	//		o.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
	//	});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<CbarApiSettings>(builder.Configuration.GetSection("CbarApiSettings"));
builder.Services.AddScoped<RequestResponseLoggingMiddleware>();
builder.Services.AddScoped<ISaveLogs, SaveLogs>();
builder.Services.AddSingleton<IPayloadLogReader, PayloadLogReader>();
builder.Services.AddScoped<IAccrJobRepository, AccrJobRepository>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IUiJobsService, UiJobsService>();
builder.Services.AddSingleton<IJwtGenerator, JwtGeneratorService>();
//builder.Services.AddHttpClient<IApiClientService, ApiClientService>();
builder.Services.AddHttpClient<IApiClientService, ApiClientService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});
//builder.Services.AddScoped<IApiClientService, ApiClientService>();

builder.Services.AddSingleton<IExportDispatcherGate, ExportDispatcherGate>();
builder.Services.AddHostedService<ExportDispatcherWorker>(); //helelik kommente atdim jenkinsde islemesin

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();
