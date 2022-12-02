using System.Text.Json;
using System.Reflection;
using System.Text;
using System.Globalization;

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
    private const int GOOD_RUSSIANS_COUNT_LIMIT_1 = 500;
    private const int GOOD_RUSSIANS_COUNT_LIMIT_2 = 750;
    private const int GOOD_RUSSIANS_COUNT_LIMIT_3 = 1000;
    private const int RUSSIAN_TANKS_LIMIT_1 = 20;

    public async Task<(string, string)> GetData()
    {
        try
        {
            var sticker = string.Empty;
            var losses = await new HttpClient()
                .GetFromJsonAsync<Root>("https://russianwarship.rip/api/v1/statistics/latest");

            if (string.IsNullOrEmpty(losses!.message) || losses.message != "The data were fetched successfully.")
            {
                return (string.Empty, sticker);
            }
            var date = losses.data.date.ToString("dd/MM/yy", CultureInfo.CreateSpecificCulture("en-US"));
            var builder = new StringBuilder($"Втрати на {date}{Environment.NewLine}");

            Dictionary<string, string> statNameDictionary = new Dictionary<string, string> {
                ["personnel_units"] = "русні",
                ["tanks"] = "скрєпних танків",
                ["armoured_fighting_vehicles"] = "брон\\. машин",
                ["artillery_systems"] = "арт\\. систем",
                ["mlrs"] = "РСЗВ",
                ["aa_warfare_systems"] = "аналоговнєтних ппо",
                ["planes"] = "вєчнольотних літаків",
                ["helicopters"] = "гелікоптерів",
                ["vehicles_fuel_tanks"] = "авто та цистерни",
                ["warships_cutters"] = "кораблі/катери",
                ["cruise_missiles"] = "крилатих ракет",
                ["uav_systems"] = "БПЛА",
                ["special_military_equip"] = "спецтехніка",
                ["atgm_srbm_systems"] = "ОТРК",
            };

            var stats = new List<string>();
            foreach (PropertyInfo stat in losses.data.stats.GetType().GetProperties()) {
                stats.Add($"{statNameDictionary[stat.Name]}: *{stat.GetValue(losses.data.stats)}*");
            }

            var increase = new List<string>();
            foreach (PropertyInfo stat in losses.data.increase.GetType().GetProperties()) {
                var change = Convert.ToInt32(stat.GetValue(losses.data.increase));
                var str = new StringBuilder();
                if (change != 0) {
                    str.Append($" \\+ \\(*{change}*\\)");
                    if (stat.Name == "personnel_units") {
                        if (change >= GOOD_RUSSIANS_COUNT_LIMIT_3) {
                            str.Append("🤖💪👊");
                        } else if (change >= GOOD_RUSSIANS_COUNT_LIMIT_2) {
                            str.Append("🥳💪");
                        } else if (change >= GOOD_RUSSIANS_COUNT_LIMIT_1) {
                            str.Append("🎉");
                        }
                    } else if (stat.Name == "tanks") {
                        if (change >= RUSSIAN_TANKS_LIMIT_1) {
                            str.Append("💥🙉");
                            sticker = "русні пизда";
                        }
                    }
                }
                increase.Add(str.ToString());
            }

            for (int i = 0; i < stats.Count(); i++) {
                builder.Append(stats[i]);
                builder.Append(increase[i]);
                builder.AppendLine();
            }
            return (builder.ToString(), sticker);
        }
        catch (HttpRequestException hre)
        {
            Console.WriteLine(hre);
            return (string.Empty, string.Empty);
        }
    }
}