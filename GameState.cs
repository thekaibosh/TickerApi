public class GameState
{
  public string? Time { get; init; }
  public string? Quarter { get; init; }
  public string? Home { get; init; }
  public string? Away { get; init; }
  public string? home_team_name { get; set; }
  public string? away_team_name { get; set; }
  private string? _ticker;
  public string? Ticker
  {
    get => _ticker;
    set => _ticker = value;
  }

  public void UpdateTicker()
  {
    var ordinal = Quarter switch
      {
        "1" => "1st",
        "2" => "2nd",
        _ => $"{Quarter}th"

      };
      string timeLeft = "00:00";
      if (float.TryParse(Time, out float floatVal))
      {
        int truncatedValue = (int)floatVal;


        int minutes = truncatedValue / 60;
        int seconds = truncatedValue % 60;
        if (seconds > 0)
        {
          timeLeft = $"{minutes}:{seconds}";
        }
      }
      Ticker = $"{home_team_name} {Home} {away_team_name} {Away} {timeLeft} {ordinal}";
  }
}