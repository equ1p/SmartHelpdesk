using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Scalar.AspNetCore;
using SmartHelpdesk.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDocumentStore>(sp =>
{
    var config = builder.Configuration.GetSection("RavenDb");
    var urls = config.GetSection("Urls").Get<string[]>()!;
    var database = config.GetValue<string>("DatabaseName") ?? "SmartHelpdesk";

    var store = new DocumentStore
    {
        Urls = urls,
        Database = database,
    };

    store.Initialize();

    IndexCreation.CreateIndexes(typeof(Program).Assembly, store);

    return store;
});

builder.Services.AddTransient<DatabaseSeeder>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
