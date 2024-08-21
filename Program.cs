using System.Reflection.Metadata.Ecma335;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/ticker", async (string[] devices) =>
{
  if (devices == null || devices.Length == 0)
  {
    return Results.BadRequest("No devices specified.");
  }

  using HttpClient client = new HttpClient
  {
    BaseAddress = new Uri("https://nestcloud-api.scorebird.com/v2/widgets/broadcast")
  };
  client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

  var parameters = "?sport=football&serial=";

  // Create a list to hold tasks
  var tasks = devices.Select(device =>
  {
    // Append device serial to parameters
    var url = parameters + device;
    return client.GetAsync(url);
  });

  var scorebirds = new List<GameState>();
  try
  {
    // Await all the tasks
    var responses = await Task.WhenAll(tasks);
    foreach (var response in responses)
    {
      var responseBody = await response.Content.ReadAsStringAsync();
      using (JsonDocument doc = JsonDocument.Parse(responseBody))
      {
        JsonElement root = doc.RootElement;

        // Extract the display_name from schedule.home_school
        string? homeSchoolDisplayName = null;
        if (root.TryGetProperty("schedule", out JsonElement scheduleElement) &&
            scheduleElement.TryGetProperty("home_school", out JsonElement homeSchoolElement) &&
            homeSchoolElement.TryGetProperty("display_name", out JsonElement displayNameElement))
        {
          homeSchoolDisplayName = displayNameElement.GetString();
        }

        if (root.TryGetProperty("game_state", out JsonElement gameStateElement))
        {
          // Deserialize the 'game_state' object into the GameState model
          var state = JsonSerializer.Deserialize<GameState>(gameStateElement.GetRawText());
          if (state != null)
          {
            state.home_team_name = state.home_team_name ?? homeSchoolDisplayName ?? null;
            state.away_team_name = state.away_team_name ?? "Guest";
            scorebirds.Add(state);
          }
        }
      }
    }

    //remove birds that don't have a good score
    var goodbirds = scorebirds.Where(sb => sb.Away != null && sb.Home != null).ToList<GameState>();
    //compute the ticker string
    goodbirds.ForEach(b => b.UpdateTicker());
    return new XmlResult<List<GameState>>(goodbirds);
  }
  catch (Exception ex)
  {
    return Results.Problem(ex.Message);
  }
})
.WithName("GetTicker")
.WithOpenApi();

app.Run();