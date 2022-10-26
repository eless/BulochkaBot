using System.Text.Json;
using System.Reflection;
using System.Text;

namespace BarracudaTestBot.Services;

public class Data
{
    public DateTime date { get; set; }
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

public class RussianLossesService
{
    private const int GOOD_RUSSIANS_COUNT_LIMIT = 450;
    public async Task<string> GetData()
    {
        try
        {
            var losses = await new HttpClient()
                .GetFromJsonAsync<Root>("https://russianwarship.rip/api/v1/statistics/latest")!;

            if (string.IsNullOrEmpty(losses.message) || losses.message != "The data were fetched successfully.")
            {
                return string.Empty;
            }
            var date = losses.data.date.ToString("dd/MM/yyyy");
            var builder = new StringBuilder($"Втрати на {date}{Environment.NewLine}");

            List<string> statName = new List<string>() {
                "русні", "скрєпних танків", "бойових броньованих машин", "артилерійських систем", "РСЗВ", "аналоговнєтних пво",
                "літаків", "гелікоптерів", "БПЛА оперативно\\-тактичного рівня", "крилатих ракет", "кораблі\\/катери",
                "автомобільної техніки та автоцистерн", "спеціальна техніка"
            };

            List<int> stats = new List<int>();
            foreach (PropertyInfo stat in losses.data.stats.GetType().GetProperties())
            {
                var res = stat.GetValue(losses.data.stats);
                stats.Add(Convert.ToInt32(res));
            }

            List<int> increase = new List<int>();
            foreach (PropertyInfo stat in losses.data.increase.GetType().GetProperties())
            {
                var res = stat.GetValue(losses.data.increase);
                increase.Add(Convert.ToInt32(res));
            }

            for (int i = 0; i < statName.Count(); i++)
            {
                builder.Append($"{statName[i]}: *{stats[i]}*");
                if (increase[i] > 0)
                {
                    builder.Append($" \\+ \\(*{increase[i]}*\\)");
                }
                if (statName[i] == "русні")
                {
                    builder.Append(" мальчіков в трусіках");
                    if (increase[i] > GOOD_RUSSIANS_COUNT_LIMIT)
                    {
                        builder.Append("🎉");
                    }
                }
                builder.AppendLine();
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