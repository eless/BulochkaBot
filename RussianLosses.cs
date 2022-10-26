using System.Text.Json;
using System.Reflection;

namespace RusLosses;

public class Data
{
    public string date { get; set; }
    public int day { get; set; }
    public string resource { get; set; }
    public Stats stats { get; set; }
    public Increase increase { get; set; }
}

public class Increase
{
    public int personnel_units { get; set; }
    public int tanks { get; set; }
    public int armoured_fighting_vehicles { get; set; }
    public int artillery_systems { get; set; }
    public int mlrs { get; set; }
    public int aa_warfare_systems { get; set; }
    public int planes { get; set; }
    public int helicopters { get; set; }
    public int vehicles_fuel_tanks { get; set; }
    public int warships_cutters { get; set; }
    public int cruise_missiles { get; set; }
    public int uav_systems { get; set; }
    public int special_military_equip { get; set; }
    public int atgm_srbm_systems { get; set; }
}

// Used https://json2csharp.com/ to generate classes from json
public class Root
{
    public string message { get; set; }
    public Data data { get; set; }
}

public class Stats
{
    public int personnel_units { get; set; }
    public int tanks { get; set; }
    public int armoured_fighting_vehicles { get; set; }
    public int artillery_systems { get; set; }
    public int mlrs { get; set; }
    public int aa_warfare_systems { get; set; }
    public int planes { get; set; }
    public int helicopters { get; set; }
    public int vehicles_fuel_tanks { get; set; }
    public int warships_cutters { get; set; }
    public int cruise_missiles { get; set; }
    public int uav_systems { get; set; }
    public int special_military_equip { get; set; }
    public int atgm_srbm_systems { get; set; }
}

public class Losses
{
    public async Task<string> GetData()
    {
        using var client = new HttpClient();
        try
        {
            var content = await client.GetStringAsync("https://russianwarship.rip/api/v1/statistics/latest");
            Root losses = JsonSerializer.Deserialize<Root>(content);

            if (string.IsNullOrEmpty(losses.message) || losses.message != "The data were fetched successfully.") {
                return string.Empty;
            }
            System.Text.StringBuilder builder = new System.Text.StringBuilder("");

            List<string> date = new List<string>();
            date = losses.data.date.Split('-').ToList();
            builder.AppendFormat("Втрати на {0}\\.{1}\\.{2}\n", date[2], date[1], date[0]);

            List<string> statName = new List<string>() {
                "русні", "скрєпних танків", "бойових броньованих машин", "артилерійських систем", "РСЗВ", "аналоговнєтних пво",
                "літаків", "гелікоптерів", "БПЛА оперативно\\-тактичного рівня", "крилатих ракет", "кораблі\\/катери",
                "автомобільної техніки та автоцистерн", "спеціальна техніка"
            };

            List<int> stats = new List<int>();
            foreach(PropertyInfo stat in losses.data.stats.GetType().GetProperties()) {
                var res = stat.GetValue(losses.data.stats);
                stats.Add(Convert.ToInt32(res));
            }

            List<int> increase = new List<int>();
            foreach(PropertyInfo stat in losses.data.increase.GetType().GetProperties()) {
                var res = stat.GetValue(losses.data.increase);
                increase.Add(Convert.ToInt32(res));
            }

            for (int i = 0; i < statName.Count(); i++) {
                builder.Append($"{statName[i]}: ");
                builder.Append(stats[i]);
                builder.Append(increase[i] > 0 ? $" \\+ \\({increase[i]}\\)" : "");
                builder.Append(statName[i] ==  "русні" ? " мальчіков в трусіках\n" : "\n"); 
            }
            return builder.ToString();
        }
        catch (HttpRequestException hre)
        {
            Console.WriteLine(hre);
            return string.Empty;
        }
    }
}