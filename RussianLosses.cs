using System.Text.Json;

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
            string format = "{0}: {1} \\+ \\({2}\\){3}";
            builder.AppendFormat(format, "русні", losses.data.stats.personnel_units, losses.data.increase.personnel_units, " мальчіков в трусіках\n");
            builder.AppendFormat(format, "скрєпних танків", losses.data.stats.tanks, losses.data.increase.tanks, "\n");
            builder.AppendFormat(format, "бойових броньованих машин", losses.data.stats.armoured_fighting_vehicles, losses.data.increase.armoured_fighting_vehicles, "\n");
            builder.AppendFormat(format, "артилерійських систем", losses.data.stats.artillery_systems, losses.data.increase.artillery_systems, "\n");
            builder.AppendFormat(format, "РСЗВ", losses.data.stats.mlrs, losses.data.increase.mlrs, "\n");
            builder.AppendFormat(format, "аналоговнєтних пво", losses.data.stats.aa_warfare_systems, losses.data.increase.aa_warfare_systems, "\n");
            builder.AppendFormat(format, "літаків", losses.data.stats.planes, losses.data.increase.planes, "\n");
            builder.AppendFormat(format, "гелікоптерів", losses.data.stats.helicopters, losses.data.increase.helicopters, "\n");
            builder.AppendFormat(format, "БПЛА оперативно\\-тактичного рівня", losses.data.stats.uav_systems, losses.data.increase.uav_systems, "\n");
            builder.AppendFormat(format, "крилатих ракет", losses.data.stats.cruise_missiles, losses.data.increase.cruise_missiles, "\n");
            builder.AppendFormat(format, "кораблі\\/катери", losses.data.stats.warships_cutters, losses.data.increase.warships_cutters, "\n");
            builder.AppendFormat(format, "автомобільної техніки та автоцистерн", losses.data.stats.vehicles_fuel_tanks, losses.data.increase.vehicles_fuel_tanks, "\n");
            builder.AppendFormat(format, "спеціальна техніка", losses.data.stats.special_military_equip, losses.data.increase.special_military_equip, "\n");
            return builder.ToString();
        }
        catch (HttpRequestException hre)
        {
            Console.WriteLine(hre);
            return string.Empty;
        }
    }
}