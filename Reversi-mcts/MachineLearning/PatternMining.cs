using System;
using System.Collections.Generic;

namespace Reversi_mcts.MachineLearning
{
    [Serializable]
    public class PatternMining
    {
        public readonly PatternShape PatternShape;

        // why use int as dictionary key: https://stackoverflow.com/a/5743520/11898496
        public readonly Dictionary<int, float> Gamma; // min: 0.01, max: 100

        [NonSerialized] public readonly Dictionary<int, ushort> Win; // tử số: Wi

        [NonSerialized]
        public readonly Dictionary<int, ushort> Candidate; // số lần xuất hiện của 1 pattern trong game record

        [NonSerialized] public readonly Dictionary<int, float> GammaDenominator; // Mẫu số: SUM(Cij/E)

        public PatternMining(PatternShape patternShape)
        {
            PatternShape = patternShape;

            Gamma = new Dictionary<int, float>();
            Win = new Dictionary<int, ushort>();
            Candidate = new Dictionary<int, ushort>();
            GammaDenominator = new Dictionary<int, float>();
        }
    }

    public static class PatternMiningExt
    {
        private static int Key(int patternCode, int cellIndex, int player)
        {
            // int: patternCode [0 -> 3^patternLength]
            // int: cellIndex [0 -> patternLength]
            // byte: player {0, 1}

            // NOTE: patternLength < 64
            // why use concatenated key: https://stackoverflow.com/a/11909025/11898496
            return patternCode * 1000 + cellIndex * 10 + player;
        }

        // ------------------------------- Gamma ------------------------------- 
        public static float GetGamma(this PatternMining p, int patternCode, int cellIndex, int player)
        {
            var hasValue = p.Gamma.TryGetValue(Key(patternCode, cellIndex, player), out float value);
            return hasValue ? value : 1f; // default gamma value is 1f
        }

        public static void SetGamma(this PatternMining p, int patternCode, int cellIndex, int player, float value)
        {
            p.Gamma[Key(patternCode, cellIndex, player)] = value;
        }

        // ------------------------------- Win -------------------------------
        public static ushort GetWin(this PatternMining p, int patternCode, int cellIndex, int player)
        {
            var hasValue = p.Win.TryGetValue(Key(patternCode, cellIndex, player), out ushort value);
            return hasValue ? value : (ushort) 0; // default Wi value is 0
        }

        public static void IncWin(this PatternMining p, int patternCode, int cellIndex, int player)
        {
            var key = Key(patternCode, cellIndex, player);
            if (!p.Win.ContainsKey(key)) p.Win[key] = 0; // default value of Wi is 0
            p.Win[key]++;
        }

        // ------------------------------- Candidate -------------------------------
        public static ushort GetCandidate(this PatternMining p, int patternCode, int cellIndex, int player)
        {
            var hasValue = p.Candidate.TryGetValue(Key(patternCode, cellIndex, player), out ushort value);
            return hasValue ? value : (ushort) 0; // default Wi Candidate is 0
        }

        public static void IncCandidate(this PatternMining p, int patternCode, int cellIndex, int player)
        {
            var key = Key(patternCode, cellIndex, player);
            if (!p.Candidate.ContainsKey(key)) p.Candidate[key] = 0; // default value of Candidate is 0
            p.Candidate[key]++;
        }

        // ------------------------------- GammaDenominator -------------------------------
        public static float GetGammaDenominator(this PatternMining p, int patternCode, int cellIndex, int player)
        {
            var hasValue = p.GammaDenominator.TryGetValue(Key(patternCode, cellIndex, player), out float value);
            return hasValue ? value : 0f; // default GammaDenominator value is 0
        }

        public static void SetGammaDenominator(this PatternMining p, int patternCode, int cellIndex, int player,
            float value)
        {
            p.GammaDenominator[Key(patternCode, cellIndex, player)] = value;
        }

        public static void IncGammaDenominator(this PatternMining p, int patternCode, int cellIndex, int player,
            float value)
        {
            var key = Key(patternCode, cellIndex, player);
            if (!p.GammaDenominator.ContainsKey(key))
                p.GammaDenominator[key] = 0; // default value of Game Denominator is 0
            p.GammaDenominator[key] += value;
        }
    }
}