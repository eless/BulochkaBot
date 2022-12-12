using System.Text.Json;
using System.Reflection;
using System.Text;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using System.Collections.ObjectModel;
using System.Numerics;
using static System.Net.WebRequestMethods;

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
    public List<string> stickers = new ();
    public List<string> animations = new ();
}

public class Limit
{
    public int limit { get; set; }
    public string smile { get; set; }
    public List<string> animation = new ();
    public List<string> sticker = new ();
}

public class RussianLossesService
{
    private readonly Dictionary<string, List<Limit>> limits = new Dictionary<string, List<Limit>>
    {
        ["personnel_units"] = new List<Limit>() {
            new Limit() { limit = 1500, smile = "🔥🔥🔥 💪👊💪 💥💥💥", sticker = {"CAACAgIAAxkBAAEBZWljTVG3uiQ6EpmPJNLPCMQYqgHKpAAC0R0AAtz9eUiSMtzqMNIUsioE"} },
            new Limit() { limit = 1000, smile = "🤖💪👊" , animation = {"https://media2.giphy.com/media/2w6I6nCyf5rmy5SHBy/giphy.gif", "https://media2.giphy.com/media/Oj7yTCLSZjSt2JMwi2/giphy.gif"} },
            new Limit() { limit = 750, smile = "🥳💪", animation = {"https://media.tenor.com/1OX3Uc7IgkMAAAAM/oof-military.gif"} },
            new Limit() { limit = 500, smile = "🎉", animation = { "https://64.media.tumblr.com/1d112324be4bf9251352b3dd4d9546df/c9a1751f8d44ebf2-74/s400x600/df1fc5490d6a4b9ecf50cd25ebac0cd48e038fce.gif" } },
        },
        ["tanks"] = new List<Limit>() {
            new Limit() { limit = 20, smile = "💥🙉", animation = {"https://media.giphy.com/media/AgaXMCnoSbNHa/giphy.gif" } },
        },
        ["planes"] = new List<Limit>() {
            new Limit() { limit = 1, smile = "🔥", animation = {"https://thumbs.gfycat.com/BaggySarcasticCarpenterant-max-1mb.gif", "CgACAgIAAxkBAAEBhZ1jl6GT6PHvhErUV6D4CNtO3Se38gAClCQAAuCTwEi7wz132XMHDCsE" } },
        },
        ["warships_cutters"] = new List<Limit>() {
            new Limit() { limit = 1, smile = "🔥", animation = {"https://media.tenor.com/bhAAVRUg_igAAAAM/fail-as-a-team-team-fail.gif" } },
        },
        ["uav_systems"] = new List<Limit>() {
            new Limit() { limit = 7, smile = "🔥", animation = {"https://media.tenor.com/aDV3obO5gAIAAAAd/plane-toy-plane.gif", "https://thumbs.gfycat.com/GiddyQuickCardinal-max-1mb.gif" } },
        },
        ["helicopters"] = new List<Limit>() {
            new Limit() { limit = 3, smile = "🔥", animation = { "https://i.gifer.com/HGjG.gif", "https://i.gifer.com/3Y7s.gif" } },
        },
        ["mlrs"] = new List<Limit>() {
            new Limit() { limit = 2, smile = "🔥", animation = { "https://i.ucrazy.ru/files/pics/2014.07/1404321857_3.gif" } },
        },
        ["artillery_systems"] = new List<Limit>() {
            new Limit() { limit = 5, smile = "🔥", animation = { "https://i.makeagif.com/media/2-27-2021/2nmnM0.gif" } },
        },
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
                    limits.TryGetValue(stat.Name, out List<Limit> limitsList);
                    if (limitsList != null)
                    {
                        foreach (var item in limitsList)
                        {
                            if (change >= item.limit)
                            {
                                str.Append(item.smile);
                                var random = new Random();
                                if (item.animation.Any())
                                {
                                    int index = random.Next(item.animation.Count);
                                    data.animations.Add(item.animation[index]);
                                }
                                if (item.sticker.Any())
                                {
                                    int index = random.Next(item.sticker.Count);
                                    data.stickers.Add(item.sticker[index]);
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