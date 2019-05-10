using System;
using System.Collections.Generic;

using Humanizer;

namespace ArenaFighter.Models.Utils {
    public enum Genders {
        Male,
        Female,
        Non_Binary
    }

    public static class GenderExtensions {
        public static string ToFriendlyString(this Genders g) {
            return g.ToString().Hyphenate();
        }
    }

    public static class Language {
        private static Dictionary<Genders, string> subjectPronouns = new Dictionary<Genders, string>() {
            [Genders.Male] = "he", [Genders.Female] = "she", [Genders.Non_Binary] = "they"
        };

        private static Dictionary<Genders, string> objectPronouns = new Dictionary<Genders, string>() {
            [Genders.Male] = "him", [Genders.Female] = "her", [Genders.Non_Binary] = "them"
        };

        private static Dictionary<Genders, string> possessiveAdjectives = new Dictionary<Genders, string>() {
            [Genders.Male] = "his", [Genders.Female] = "her", [Genders.Non_Binary] = "their"
        };

        private static Dictionary<Genders, string> possessivePronouns = new Dictionary<Genders, string>() {
            [Genders.Male] = "his", [Genders.Female] = "hers", [Genders.Non_Binary] = "theirs"
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

        public static string PickRandomString(string[] alternatives) {
            return alternatives[DiceRoller.Next(0, alternatives.Length)];
        }

        public static string PadLinesLeft(string text, int padding, string customPadding = " ") {
            return "".PadLeft(padding - 2, customPadding[0]) + customPadding[customPadding.Length - 1] + " " + text.Replace("\n", ("\n" + "".PadLeft(padding - 2, customPadding[0]) + customPadding[customPadding.Length - 1]));
        }

        public static string PadLines(string text, int totalPadding, string customLeftPadding = "->", string customRightPadding = "<-", string separator = " ") {
            string changedText = text;
            if (text.Length == 0) {
                return "";
            }
            int separatorLengthTotal = separator.Length * 2;
            int padding = totalPadding / 2;
            if (padding <= separatorLengthTotal) {
                padding = Math.Max(padding, 0);
                return "".PadLeft(padding) + changedText + "".PadLeft(padding);
            }
            if (text[text.Length - 1] != '\n') {
                changedText = text + separator + customRightPadding[0] + "".PadLeft(padding - separatorLengthTotal, customRightPadding[customRightPadding.Length - 1]);
            }
            changedText = changedText.Replace("\n", (separator + customRightPadding[0] + "".PadLeft(padding - separatorLengthTotal, customRightPadding[customRightPadding.Length - 1]) + "\n" + "".PadLeft(padding - separatorLengthTotal, customLeftPadding[0]) + customLeftPadding[customLeftPadding.Length - 1]));
            return "".PadLeft(padding - separatorLengthTotal, customLeftPadding[0]) + customLeftPadding[customLeftPadding.Length - 1] + separator + changedText;
        }
    }
}
