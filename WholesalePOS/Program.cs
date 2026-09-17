
using WholesalePOS.Api.Middleware;
using WholesalePOS.Application;
using WholesalePOS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

//MVC
builder.Services.AddControllers();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Application Layers
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Later we will also add
//builder.Services.AddProblemDetails();

//Later we will add
//app.UseExceptionHandler();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
