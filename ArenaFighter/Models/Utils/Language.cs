using System.Collections.Generic;

namespace ArenaFighter.Models.Utils
{
    public enum Genders {
        Male,
        Female,
        Other
    }

    public static class Language
    {
        private static Dictionary<Genders, string> subjectPronouns = new Dictionary<Genders, string>() {
            [Genders.Male] = "he",
            [Genders.Female] = "she",
            [Genders.Other] = "they"
        };

        private static Dictionary<Genders, string> objectPronouns = new Dictionary<Genders, string>() {
            [Genders.Male] = "him",
            [Genders.Female] = "her",
            [Genders.Other] = "them"
        };

        private static Dictionary<Genders, string> possessiveAdjectives = new Dictionary<Genders, string>() {
            [Genders.Male] = "his",
            [Genders.Female] = "her",
            [Genders.Other] = "their"
        };

        private static Dictionary<Genders, string> possessivePronouns = new Dictionary<Genders, string>() {
            [Genders.Male] = "his",
            [Genders.Female] = "hers",
            [Genders.Other] = "theirs"
        };

        public static string SubjectPronoun(BaseCharacter character) {
            return SubjectPronoun(character.Gender);
        }

        public static string SubjectPronoun(Genders gender) {
            return subjectPronouns[gender];
        }

        public static string ObjectPronoun(BaseCharacter character) {
            return ObjectPronoun(character.Gender);
        }

        public static string ObjectPronoun(Genders gender) {
            return objectPronouns[gender];
        }

        public static string PossessiveAdjective(BaseCharacter character) {
            return PossessiveAdjective(character.Gender);
        }

        public static string PossessiveAdjective(Genders gender) {
            return possessiveAdjectives[gender];
        }

        public static string PossessivePronoun(BaseCharacter character) {
            return PossessivePronoun(character.Gender);
        }

        public static string PossessivePronoun(Genders gender) {
            return possessivePronouns[gender];
        }


    }
}