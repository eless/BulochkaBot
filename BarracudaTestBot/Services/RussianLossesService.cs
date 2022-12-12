using System.Text.Json;
using System.Reflection;
using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

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

public class RussianLossesData
{
    public string units = string.Empty;
    public List<string> stickers = new List<string>();
    public List<string> animations = new List<string>();
}

public class Limit
{
    public int limit { get; set; }
    public string smile { get; set; }
    public string animation { get; set; }
    public string sticker { get; set; }
}

public class RussianLossesService
{
    private readonly List<Limit> _goodRussiansLimit = new List<Limit>() {
        new Limit() { limit = 1500, smile = "🔥🔥🔥 💪👊💪 💥💥💥", sticker = "CAACAgIAAxkBAAEBZWljTVG3uiQ6EpmPJNLPCMQYqgHKpAAC0R0AAtz9eUiSMtzqMNIUsioE" },
        new Limit() { limit = 1000, smile = "🤖💪👊" },
        new Limit() { limit = 750, smile = "🥳💪" },
        new Limit() { limit = 500, smile = "🎉" },
    };
    private readonly List<Limit> _russianTanksLimit = new List<Limit>() {
        new Limit() { limit = 20, smile = "💥🙉", animation = "https://media.giphy.com/media/AgaXMCnoSbNHa/giphy.gif" },
    };

    public async Task<RussianLossesData> GetData()
    {
        RussianLossesData data = new RussianLossesData();
        try
        {
            var losses = await new HttpClient()
                .GetFromJsonAsync<Root>("https://russianwarship.rip/api/v1/statistics/latest");

            if (string.IsNullOrEmpty(losses!.message) || losses.message != "The data were fetched successfully.")
            {
                return data;
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
                        for(int i = 0; i < _goodRussiansLimit.Count; i++)
                        {
                            if (change >= _goodRussiansLimit[i].limit)
                            {
                                str.Append(_goodRussiansLimit[i].smile);
                                if (!string.IsNullOrEmpty(_goodRussiansLimit[i].animation))
                                {
                                    data.animations.Add(_goodRussiansLimit[i].animation);
                                } else if (!string.IsNullOrEmpty(_goodRussiansLimit[i].sticker))
                                {
                                    data.stickers.Add(_goodRussiansLimit[i].sticker);
                                }
                                break;
                            }
                        }
                    } else if (stat.Name == "tanks") {
                        for (int i = 0; i < _russianTanksLimit.Count; i++)
                        {
                            if (change >= _russianTanksLimit[i].limit)
                            {
                                str.Append(_russianTanksLimit[i].smile);
                                if (!string.IsNullOrEmpty(_russianTanksLimit[i].animation))
                                {
                                    data.animations.Add(_russianTanksLimit[i].animation);
                                }
                                break;
                            }
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
            data.units = builder.ToString();
            return data;
        }
        catch (HttpRequestException hre)
        {
            Console.WriteLine(hre);
            return data;
        }
    }
}