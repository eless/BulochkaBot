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

    public static Stats operator - (Stats a, Stats b)
    {
        var res = new Stats();
        res.personnel_units = a.personnel_units - b.personnel_units;
        res.tanks = a.tanks - b.tanks;
        res.armoured_fighting_vehicles = a.armoured_fighting_vehicles - b.armoured_fighting_vehicles;
        res.artillery_systems = a.artillery_systems - b.artillery_systems;
        res.mlrs = a.mlrs - b.mlrs;
        res.aa_warfare_systems = a.aa_warfare_systems - b.aa_warfare_systems;
        res.planes = a.planes - b.planes;
        res.helicopters = a.helicopters - b.helicopters;
        res.vehicles_fuel_tanks = a.vehicles_fuel_tanks - b.vehicles_fuel_tanks;
        res.warships_cutters = a.warships_cutters - b.warships_cutters;
        res.cruise_missiles = a.cruise_missiles - b.cruise_missiles;
        res.uav_systems = a.uav_systems - b.uav_systems;
        res.special_military_equip = a.special_military_equip - b.special_military_equip;
        res.atgm_srbm_systems = a.atgm_srbm_systems - b.atgm_srbm_systems;
        return res;
    }

    public static Stats operator + (Stats a, Stats b)
    {
        var res = new Stats();
        res.personnel_units = a.personnel_units + b.personnel_units;
        res.tanks = a.tanks + b.tanks;
        res.armoured_fighting_vehicles = a.armoured_fighting_vehicles + b.armoured_fighting_vehicles;
        res.artillery_systems = a.artillery_systems + b.artillery_systems;
        res.mlrs = a.mlrs + b.mlrs;
        res.aa_warfare_systems = a.aa_warfare_systems + b.aa_warfare_systems;
        res.planes = a.planes + b.planes;
        res.helicopters = a.helicopters + b.helicopters;
        res.vehicles_fuel_tanks = a.vehicles_fuel_tanks + b.vehicles_fuel_tanks;
        res.warships_cutters = a.warships_cutters + b.warships_cutters;
        res.cruise_missiles = a.cruise_missiles + b.cruise_missiles;
        res.uav_systems = a.uav_systems + b.uav_systems;
        res.special_military_equip = a.special_military_equip + b.special_military_equip;
        res.atgm_srbm_systems = a.atgm_srbm_systems + b.atgm_srbm_systems;
        return res;
    }
}

public class RussianLossesData
{
    public string units = string.Empty;
    public List<string> stickers = new ();
    public List<string> animations = new ();
}

public class Limit
{
    public double limit { get; set; }
    public string smile { get; set; }
    public List<string> animation = new ();
    public List<string> sticker = new ();
}

public class RussianLossesService
{
    private readonly Dictionary<string, Limit> statlimitsInfo = new Dictionary<string, Limit>
    {
        ["personnel_units"] = 
            new Limit() { smile = "🔥", animation = { "https://64.media.tumblr.com/1d112324be4bf9251352b3dd4d9546df/c9a1751f8d44ebf2-74/s400x600/df1fc5490d6a4b9ecf50cd25ebac0cd48e038fce.gif",
                                                      "https://media.tenor.com/1OX3Uc7IgkMAAAAM/oof-military.gif", "https://media2.giphy.com/media/2w6I6nCyf5rmy5SHBy/giphy.gif",
                                                      "https://media2.giphy.com/media/Oj7yTCLSZjSt2JMwi2/giphy.gif"} },
        ["tanks"] = new Limit { smile = "💥", animation = { "https://media.giphy.com/media/AgaXMCnoSbNHa/giphy.gif" } },
        ["armoured_fighting_vehicles"] = new Limit() { smile = "🔥", animation = { "" } },
        ["artillery_systems"] = new Limit { smile = "🔥", animation = { "https://i.makeagif.com/media/2-27-2021/2nmnM0.gif" } },
        ["mlrs"] = new Limit { smile = "🔥", animation = { "https://i.ucrazy.ru/files/pics/2014.07/1404321857_3.gif" } },
        ["aa_warfare_systems"] = new Limit { smile = "🔥", animation = { "https://i.makeagif.com/media/9-25-2015/eLgg4N.gif" } },
        ["planes"] = new Limit { smile = "🔥", animation = { "https://thumbs.gfycat.com/BaggySarcasticCarpenterant-max-1mb.gif",
                                                              "https://media.giphy.com/media/7SIcw2yfQdfeJgP29f/giphy-downsized-large.gif",
                                                              "https://media.giphy.com/media/Qtz7JZFyhhEXaRk7kT/giphy.gif"} },
        ["helicopters"] = new Limit { smile = "🔥", animation = { "https://i.gifer.com/HGjG.gif", "https://i.gifer.com/3Y7s.gif" } },
        ["vehicles_fuel_tanks"] = new Limit { smile = "🔥"},
        ["warships_cutters"] = new Limit { smile = "🔥", animation = {"https://media.tenor.com/bhAAVRUg_igAAAAM/fail-as-a-team-team-fail.gif" } },
        ["cruise_missiles"] = new Limit { smile = "🔥"},
        ["uav_systems"] = new Limit { smile = "🔥", animation = {"https://media.tenor.com/aDV3obO5gAIAAAAd/plane-toy-plane.gif", 
                                                                   "https://thumbs.gfycat.com/GiddyQuickCardinal-max-1mb.gif" } },
        ["special_military_equip"] = new Limit { smile = "🔥"},
        ["atgm_srbm_systems"] = new Limit { smile = "🔥"},
    };

    private async void setLimits(Root losses)
    {
        List<Data> previouslosses = new List<Data>();
        previouslosses.Add(losses.data);

        const int STATISTICS_PERIOD = 7;
        var averageIncrease = new Stats();
        var requestDate = losses.data.date;

        for (int i = 0; i < STATISTICS_PERIOD; i++)
        {
            requestDate = requestDate.AddDays(-1);
            var requestDateStr = requestDate.Date.ToString("yyyy-MM-dd");
            var res = new HttpClient().GetFromJsonAsync<Root>($"https://russianwarship.rip/api/v1/statistics/{requestDateStr}").Result;
            if (res != null)
            {
                previouslosses.Add(res.data);
                if (i != 0)
                {
                    averageIncrease += previouslosses[i - 1].stats - previouslosses[i].stats;
                }
            }
        }

        foreach (PropertyInfo stat in averageIncrease.GetType().GetProperties())
        {
            var change = Convert.ToDouble(stat.GetValue(averageIncrease));
            statlimitsInfo[stat.Name].limit = change / STATISTICS_PERIOD;
            System.Diagnostics.Trace.WriteLine($"{stat.Name} : {change / STATISTICS_PERIOD}");
        }
    }

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
            setLimits(losses);

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
            double coeficient = 0;
            var significantStatGif = String.Empty;

            foreach (PropertyInfo stat in losses.data.increase.GetType().GetProperties()) {
                var change = Convert.ToInt32(stat.GetValue(losses.data.increase));
                var str = new StringBuilder();
                if (change != 0)
                {
                    str.Append($" \\+ \\(*{change}*\\)");
                    statlimitsInfo.TryGetValue(stat.Name, out Limit item);
                    if (item != null)
                    {
                        if (change >= item.limit)
                        {
                            str.Append(item.smile);
                            
                            var random = new Random();
                            var index = random.Next(item.animation.Count);
                            if (item.limit == 0)
                            {
                                data.animations.Add(item.animation[index]);
                            }
                            else
                            {
                                var coef = change / item.limit;
                                if (coef > coeficient)
                                {
                                    coeficient = coef;
                                    significantStatGif = item.animation[index];
                                }
                            }
                        }
                    }
                }
                increase.Add(str.ToString());
            }

            if (!String.IsNullOrEmpty(significantStatGif))
            {
                data.animations.Add(significantStatGif);
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