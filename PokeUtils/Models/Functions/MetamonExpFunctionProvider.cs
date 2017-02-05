using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace PokeUtils.Models.Functions
{
    public class MetamonExpFunctionProvider : IFunctionProvider
    {
        public MetamonExpFunctionProvider(string userName)
        {
            this.UserName = userName;
        }
        public int CurrentExp { get; set; } = -1;
        public Personarities RequredPersonarity { get; set; } = Personarities.Unknown;
        public string UserName { get; }

        public string GetReplyMessage(string message)
        {
            var regex = new Regex("(.*?)なメタモンがほしい");
            var match = regex.Match(message);
            if (match.Success)
            {
                return GetReplyMessageFromRequiredPersonarity(match.Groups[1].Value);
            }

            uint exp = 0;
            if(uint.TryParse(message, out exp))
            {
                this.CurrentExp = (int)exp;
                return GetReplyMessageFromCurrentExp(exp);
            }
            return null;
        }

        private string GetReplyMessageFromCurrentExp(uint exp)
        {
            // 現在の性格を取得。
            var personarity = PersonalityUtils.GetPersonarityFromExp(exp);
            var messages = new List<string>
                {
                    $"それは {personarity.PersonarityToString()} な メタモンみたいだね.",
                };
            // ほしい性格がわかっていれば、それに合わせたアドバイス
            if (this.RequredPersonarity != Personarities.Unknown)
            {
                var remain = GetNecessaryExpToRequiredPersonarity(this.RequredPersonarity, (int)exp);
                if (remain != 0)
                {
                    messages.Add($"あと {GetNecessaryExpToRequiredPersonarity(this.RequredPersonarity, (int)exp)} 経験を積むと、 {RequredPersonarity.PersonarityToString()} な メタモンになるんじゃない？");
                }
            }
            return string.Join("\r\n", messages);
        }

        public static int GetNecessaryExpToRequiredPersonarity(Personarities requiredPersonarity, int currentExp)
        {
            var val = currentExp % 25;
            var remain = (int)requiredPersonarity - val;
            if(remain < 0)
            {
                return 25 + remain;
            } else
            {
                return remain;
            }
        }

        private string GetReplyMessageFromRequiredPersonarity(string value)
        {
            var personarity = PersonalityUtils.GetPersonarityFromString(value);
            if (personarity == Personarities.Unknown)
            {
                return "きいたことない性格ですね...";
            }
            else
            {
                this.RequredPersonarity = personarity;
                var messages = new List<string> { $"{personarity.PersonarityToString()} な メタモンがほしいんですね。" };
                // 経験値を覚えていれば、アドバイス。
                if(this.CurrentExp != -1)
                {
                    messages.Add($"さっきのメタモンだと、あと {GetNecessaryExpToRequiredPersonarity(personarity, this.CurrentExp)} 経験を積むとよさそうだね。");
                }
                return string.Join("\r\n", messages);
            }
        }
    }

    public enum Personarities
    {
        Ganbaruya = 0,
        Samisigari,
        Yuukan,
        Ijippari,
        Yancha,
        Zubutoi,
        Sunao,
        Nonki,
        Wanpaku,
        Noutenki,
        Okubyou,
        Sekkachi,
        Majime,
        Youki,
        Mujaki,
        Hikaeme,
        Ottori,
        Reisei,
        Tereya,
        Ukkariya,
        Odayaka,
        Otonashii,
        Namaiki,
        Shincho,
        Kimagure = 24,
        Unknown,
    }

    public static class PersonalityUtils
    {
        public static Personarities GetPersonarityFromString(string str)
        {
            switch (str)
            {
                case "がんばりや": return Personarities.Ganbaruya;
                case "さみしがり": return Personarities.Samisigari;
                case "ゆうかん": return Personarities.Yuukan;
                case "いじっぱり": return Personarities.Ijippari;
                case "やんちゃ": return Personarities.Yancha;
                case "ずぶとい": return Personarities.Zubutoi;
                case "すなお": return Personarities.Sunao;
                case "のんき": return Personarities.Nonki;
                case "わんぱく": return Personarities.Wanpaku;
                case "のうてんき": return Personarities.Noutenki;
                case "おくびょう": return Personarities.Okubyou;
                case "せっかち": return Personarities.Sekkachi;
                case "まじめ": return Personarities.Majime;
                case "ようき": return Personarities.Youki;
                case "むじゃき": return Personarities.Mujaki;
                case "ひかえめ": return Personarities.Hikaeme;
                case "おっとり": return Personarities.Ottori;
                case "れいせい": return Personarities.Reisei;
                case "てれや": return Personarities.Tereya;
                case "うっかりや": return Personarities.Ukkariya;
                case "おだやか": return Personarities.Odayaka;
                case "おとなしい": return Personarities.Otonashii;
                case "なまいき": return Personarities.Namaiki;
                case "しんちょう": return Personarities.Shincho;
                case "きまぐれ": return Personarities.Kimagure;
                default:
                    return Personarities.Unknown;
            }
        }
        public static string PersonarityToString(this Personarities self)
        {
            switch (self)
            {
                case Personarities.Ganbaruya: return "がんばりや";
                case Personarities.Hikaeme: return "ひかえめ";
                case Personarities.Ijippari: return "いじっぱり";
                case Personarities.Kimagure: return "きまぐれ";
                case Personarities.Majime: return "まじめ";
                case Personarities.Mujaki: return "むじゃき";
                case Personarities.Namaiki: return "なまいき";
                case Personarities.Nonki: return "のんき";
                case Personarities.Noutenki: return "のうてんき";
                case Personarities.Odayaka: return "おだやか";
                case Personarities.Okubyou: return "おくびょう";
                case Personarities.Otonashii: return "おとなしい";
                case Personarities.Ottori: return "おっとり";
                case Personarities.Reisei: return "れいせい";
                case Personarities.Samisigari: return "さみしがり";
                case Personarities.Sekkachi: return "せっかち";
                case Personarities.Shincho: return "しんちょう";
                case Personarities.Sunao: return "すなお";
                case Personarities.Tereya: return "てれや";
                case Personarities.Ukkariya: return "うっかりや";
                case Personarities.Wanpaku: return "わんぱく";
                case Personarities.Yancha: return "やんちゃ";
                case Personarities.Youki: return "ようき";
                case Personarities.Yuukan: return "ゆうかん";
                case Personarities.Zubutoi: return "ずぶとい";
                case Personarities.Unknown:
                default: return "ふめい";
            }
        }

        internal static Personarities GetPersonarityFromExp(uint exp)
        {
            return (Personarities)(exp % 25);

        }
    }
}