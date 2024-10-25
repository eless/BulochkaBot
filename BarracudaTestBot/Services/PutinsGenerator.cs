
using BarracudaTestBot;

namespace BarracudaTestBot.Services
{
    public class PutinGenerator
    {
        private readonly List<string> wordLast = [
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
        ];

        private readonly List<string> adjectives = [
            "мерзенний",
            "паскудний",
            "в попу йобаний",
            "на пляшці сидівший",
            "хуй сосавший",
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
        ];

        public string GenerateName(string name = "путін")
        {
            System.Text.StringBuilder builder = new($"{name} ");
            int adjectivesCount = Utilities.GetRandomNumber(1, 3);
            Random rng = new();
            builder.Append(string.Join(" ", adjectives.OrderBy(x => rng.Next()).Take(adjectivesCount)));
            builder.Append($" {wordLast[Utilities.GetRandomNumber(0, wordLast.Count())]}");
            return builder.ToString();
        }
    }
}