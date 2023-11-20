using System.Reflection;
using System.Text;
using System.Globalization;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;

namespace BarracudaTestBot.Services;

public class Data
{
    public required DateTime date { get; set; }
    public required int day { get; set; }
    public required string resource { get; set; }
    public required Stats stats { get; set; }
    public required Increase increase { get; set; }
}

public class Increase
{
    public required int personnel_units { get; set; }
    public required int tanks { get; set; }
    public required int armoured_fighting_vehicles { get; set; }
    public required int artillery_systems { get; set; }
    public required int mlrs { get; set; }
    public required int aa_warfare_systems { get; set; }
    public required int planes { get; set; }
    public required int helicopters { get; set; }
    public required int vehicles_fuel_tanks { get; set; }
    public required int warships_cutters { get; set; }
    public required int cruise_missiles { get; set; }
    public required int uav_systems { get; set; }
    public required int special_military_equip { get; set; }
    public required int atgm_srbm_systems { get; set; }
}

// Used https://json2csharp.com/ to generate classes from json
public class Root
{
    public required string message { get; set; }
    public required Data data { get; set; }
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

public class LimitData
{
    public double Limit { get; set; }
    public required string Smile { get; set; }
    public List<string> Animation = new ();
    public List<string> Sticker = new ();
    public required string Caption { get; set; }
}

public class RussianLossesService
{
    private HttpClient _httpClient;
    private TelemetryClient _telemetry;
    public RussianLossesService(HttpClient httpClient, TelemetryClient telemetry)
    {
        _httpClient = httpClient;
        _telemetry = telemetry;
    }

    private readonly Dictionary<string, LimitData> statlimitsInfo = new Dictionary<string, LimitData>
    {
        ["personnel_units"] = 
            new LimitData() {
                Caption = "русні",
                Smile = "🔥", Animation = { "https://64.media.tumblr.com/1d112324be4bf9251352b3dd4d9546df/c9a1751f8d44ebf2-74/s400x600/df1fc5490d6a4b9ecf50cd25ebac0cd48e038fce.gif",
                                            "https://media.tenor.com/1OX3Uc7IgkMAAAAM/oof-military.gif", "https://media2.giphy.com/media/2w6I6nCyf5rmy5SHBy/giphy.gif",
                                            "https://media2.giphy.com/media/Oj7yTCLSZjSt2JMwi2/giphy.gif" } },
        ["tanks"] = new LimitData { Caption = "скрєпних танків",  Smile = "💥", Animation = { "https://media.giphy.com/media/AgaXMCnoSbNHa/giphy.gif" } },
        ["armoured_fighting_vehicles"] = new LimitData() { Caption = "брон\\. машин", Smile = "🔥" },
        ["artillery_systems"] = new LimitData { Caption = "арт\\. систем", Smile = "🔥", Animation = { "https://i.makeagif.com/media/2-27-2021/2nmnM0.gif" } },
        ["mlrs"] = new LimitData { Caption = "РСЗВ", Smile = "🔥", Animation = { "https://i.ucrazy.ru/files/pics/2014.07/1404321857_3.gif" } },
        ["aa_warfare_systems"] = new LimitData { Caption = "аналоговнєтних ппо", Smile = "🔥", Animation = { "https://i.makeagif.com/media/9-25-2015/eLgg4N.gif" } },
        ["planes"] = new LimitData {
            Caption = "вєчнольотних літаків",
            Smile = "🔥", Animation = { "https://thumbs.gfycat.com/BaggySarcasticCarpenterant-max-1mb.gif",
                                        "https://media.giphy.com/media/7SIcw2yfQdfeJgP29f/giphy-downsized-large.gif",
                                        "https://media.giphy.com/media/Qtz7JZFyhhEXaRk7kT/giphy.gif"} },
        ["helicopters"] = new LimitData { Caption = "гелікоптерів", Smile = "🔥", Animation = { "https://i.gifer.com/HGjG.gif", "https://i.gifer.com/3Y7s.gif" } },
        ["vehicles_fuel_tanks"] = new LimitData { Caption = "авто та цистерни", Smile = "🔥"  },
        ["warships_cutters"] = new LimitData { Caption = "кораблі/катери", Smile = "🔥", Animation = {"https://media.tenor.com/bhAAVRUg_igAAAAM/fail-as-a-team-team-fail.gif" } },
        ["cruise_missiles"] = new LimitData { Caption = "крилатих ракет", Smile = "🔥" },
        ["uav_systems"] = new LimitData {
            Caption = "БПЛА",
            Smile = "🔥", Animation = { "https://media.tenor.com/aDV3obO5gAIAAAAd/plane-toy-plane.gif", 
                                        "https://thumbs.gfycat.com/GiddyQuickCardinal-max-1mb.gif" } },
        ["special_military_equip"] = new LimitData { Caption = "спецтехніка", Smile = "🔥" },
        ["atgm_srbm_systems"] = new LimitData { Caption = "ОТРК", Smile = "🔥" },
    };

    private async Task SetLimits(Root losses)
    {
        var previouslosses = new List<Data>{ losses.data };

        const int STATISTICS_PERIOD = 7;
        var averageIncrease = new Stats();
        var requestDate = losses.data.date;

        for (int i = 0; i < STATISTICS_PERIOD; i++)
        {
            requestDate = requestDate.AddDays(-1);
            var requestDateStr = requestDate.Date.ToString("yyyy-MM-dd");
            var res = await _httpClient.GetFromJsonAsync<Root>($"https://russianwarship.rip/api/v1/statistics/{requestDateStr}");
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
            statlimitsInfo[stat.Name].Limit = change / STATISTICS_PERIOD;
            //_telemetry.TrackTrace($"{stat.Name} : {change / STATISTICS_PERIOD}");
        }
    }

    public async Task<RussianLossesData> GetData()
    {
        var data = new RussianLossesData();
        try
        {
            var losses = await _httpClient.GetFromJsonAsync<Root>("https://russianwarship.rip/api/v1/statistics/latest");

            if (string.IsNullOrEmpty(losses!.message) || losses.message != "The data were fetched successfully.")
            {
                return data;
            }
            var date = losses.data.date.ToString("dd/MM/yy", CultureInfo.CreateSpecificCulture("en-US"));
            var builder = new StringBuilder($"Втрати на {date}{Environment.NewLine}");
            await SetLimits(losses);

            var stats = new List<string>();

            var increase = new List<string>();
            double coeficient = 0;
            var significantStatGif = String.Empty;
            var statsInfoType = losses.data.stats.GetType();
            foreach (PropertyInfo stat in losses.data.increase.GetType().GetProperties())
            {
                if (statlimitsInfo.TryGetValue(stat.Name, out var item))
                {
                    stats.Add($"{item.Caption}: *{statsInfoType.GetProperty(stat.Name)!.GetValue(losses.data.stats)}*");
                    var change = Convert.ToInt32(stat.GetValue(losses.data.increase));
                    var str = new StringBuilder();
                    if (change != 0)
                    {
                        str.Append($" \\+ \\(*{change}*\\)");
                        if (change >= item.Limit)
                        {
                            str.Append(item.Smile);
                            if (item.Animation.Any())
                            {
                                var random = new Random();
                                var index = random.Next(item.Animation.Count);
                                if (item.Limit == 0)
                                {
                                    data.animations.Add(item.Animation[index]);
                                }
                                else
                                {
                                    var coef = change / item.Limit;
                                    if (coef > coeficient)
                                    {
                                        coeficient = coef;
                                        significantStatGif = item.Animation[index];
                                    }
                                }
                            }
                        }
                    }
                    increase.Add(str.ToString());
                }
            }

            if (!string.IsNullOrEmpty(significantStatGif))
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
        catch (Exception ex)
        {
            _telemetry.TrackException(ex);
            return data;
        }
    }
}