using VipaPDFMerge.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<GraphTokenService>();
builder.Services.AddSingleton<GraphOneDriveService>();
builder.Services.AddSingleton<PdfMergeService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Ok(new
{
    service = "VipaPDFMerge",
    status = "Running",
    endpoint = "POST /api/merge-pdf",
    swagger = "/swagger"
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    utc = DateTime.UtcNow
}));

app.MapControllers();

app.Run();
