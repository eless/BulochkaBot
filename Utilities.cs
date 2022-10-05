using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Text.Json;

namespace BarracudaTestBot
{
    public static class Utilities
    {
        public static async Task<Message> SendText(this ITelegramBotClient botClient,
            int? replyTo,
            long chatId,
            string text,
            CancellationToken cancellationToken, ParseMode? parseMode = ParseMode.MarkdownV2) => 
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: text,
                    parseMode: parseMode,
                    replyToMessageId: replyTo,
                    cancellationToken: cancellationToken);

        public static int GetRandomNumber(int from, int to) => new Random().Next(from, to);

    }

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

                if (losses.message != "The data were fetched successfully.") {
                    return string.Empty;
                }
                string report = "русні: " + losses.data.stats.personnel_units.ToString() + " \\(\\+" +
                                losses.data.increase.personnel_units.ToString() + " мальчіков в трусіках\\)\n";
                report += "скрєпних танків: " + losses.data.stats.tanks.ToString() + "\\(\\+" + losses.data.increase.tanks.ToString() + "\\)\n";
                report += "бойових броньованих машин: " + losses.data.stats.armoured_fighting_vehicles.ToString() + "\\(\\+" + losses.data.increase.armoured_fighting_vehicles.ToString() + "\\)\n";
                report += "артилерійських систем: " + losses.data.stats.artillery_systems.ToString() + "\\(\\+" + losses.data.increase.artillery_systems.ToString() + "\\)\n";
                report += "РСЗВ: " + losses.data.stats.mlrs.ToString() + "\\(\\+" + losses.data.increase.mlrs.ToString() + "\\)\n";
                report += "аналоговнєтних пво : " + losses.data.stats.aa_warfare_systems.ToString() + "\\(\\+" + losses.data.increase.aa_warfare_systems.ToString() + "\\)\n";
                report += "літаків: " + losses.data.stats.planes.ToString() + "\\(\\+" + losses.data.increase.planes.ToString() + "\\)\n";
                report += "гелікоптерів: " + losses.data.stats.helicopters.ToString() + "\\(\\+" + losses.data.increase.helicopters.ToString() + "\\)\n";
                report += "БПЛА оперативно\\-тактичного рівня: " + losses.data.stats.uav_systems.ToString() + "\\(\\+" + losses.data.increase.uav_systems.ToString() + "\\)\n";
                report += "крилатих ракет: " + losses.data.stats.cruise_missiles.ToString() + "\\(\\+" + losses.data.increase.cruise_missiles.ToString() + "\\)\n";
                report += "кораблі\\/катери: " + losses.data.stats.warships_cutters.ToString() + "\\(\\+" + losses.data.increase.warships_cutters.ToString() + "\\)\n";
                report += "автомобільної техніки та автоцистерн: " + losses.data.stats.vehicles_fuel_tanks.ToString() + "\\(\\+" + losses.data.increase.vehicles_fuel_tanks.ToString() + "\\)\n";
                report += "спеціальна техніка: " + losses.data.stats.special_military_equip.ToString() + "\\(\\+" + losses.data.increase.special_military_equip.ToString() + "\\)\n";
                return report;
            }
            catch (HttpRequestException hre)
            {
                Console.WriteLine(hre);
                return string.Empty;
            }
        }
    }

    public class PutinGenerator
    {
        private List<string> wordLast = new List<string>() {
            "підріла",
            "підр",
            "піздабол",
            "лох",
            "підарас",
            "пітух",
            "овцейоб",
            "дегенерат",
            "підофіл",
            "мішок з гівном",
            "пєтушара",
            "виблядок",
            "ублюдок",
            "крисюк",
            "мракобєс",
            "любітєль громкіх басов",
            "довбойоб",
            "валізоходець",
            "гном",
            "хуєсос",
        };

        private List<string> adjectives = new List<string>() {
            "мерзенний",
            "паскудний",
            "в попу йобаний",
            "на пляшці сидівший",
            "хуй ковтавший",
            "обдрочений",
            "обісраний",
            "тупорилий",
            "обісцаний",
            "лаптєногий",
            "сучий",
            "пиздливий",
            "огидний",
            "йобаний",
            "паскудний",
            "чмошний",
            "кончений",
            "бридкий",
            "гнилий",
            "хворий",
            "йобнутий",
            "хуйовий",
            "скрєпний",
        };

        public string GetName()
        {
            var putinsName = "путін ";
            int adjectivesCount = Utilities.GetRandomNumber(1, 3);
            Random rng = new Random();
            putinsName += string.Join(" ", adjectives.OrderBy(x => rng.Next()).Take(adjectivesCount));
            putinsName += " " + wordLast[Utilities.GetRandomNumber(0, wordLast.Count())];
            return putinsName;
        }
    }
}

