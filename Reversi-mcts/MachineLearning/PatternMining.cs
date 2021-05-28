using System;
using System.Collections.Generic;

namespace Reversi_mcts.MachineLearning
{
    [Serializable]
    public class PatternMining
    {
        public PatternShape PatternShape;

        // why use int as dictionary key: https://stackoverflow.com/a/5743520/11898496
        public Dictionary<int, float> Gamma; // min: 0.01, max: 100
        [NonSerialized] public Dictionary<int, ushort> Win; // tử số: Wi
        [NonSerialized] public Dictionary<int, ushort> Candidate; // số lần xuất hiện của 1 pattern trong game record
        [NonSerialized] public Dictionary<int, float> GammaDenominator; // Mẫu số: SUM(Cij/E)

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
            return p.GetGamma(Key(patternCode, cellIndex, player));
        }
        
        public static float GetGamma(this PatternMining p, int key)
        {
            var hasValue = p.Gamma.TryGetValue(key, out _);
            if (!hasValue) p.Gamma[key] = 1f; // default gamma value is 1f
            return p.Gamma[key];
        }
        
        public static void SetGamma(this PatternMining p, int patternCode, int cellIndex, int player, float value)
        {
            p.Gamma[Key(patternCode, cellIndex, player)] = value;
        }
        
        // ------------------------------- Win -------------------------------
        public static ushort GetWin(this PatternMining p, int patternCode, int cellIndex, int player)
        {
            return p.GetWin(Key(patternCode, cellIndex, player));
        }
        
        public static ushort GetWin(this PatternMining p, int key)
        {
            var hasValue = p.Win.TryGetValue(key, out _);
            if (!hasValue) p.Win[key] = 0; // default Wi value is 0
            return p.Win[key];
        }
        
        public static void AddWin(this PatternMining p, int patternCode, int cellIndex, int player)
        {
            var key = Key(patternCode, cellIndex, player);
            p.Win[key] = (ushort) (p.GetWin(key) + 1);
        }
        
        // ------------------------------- Candidate -------------------------------
        public static ushort GetCandidate(this PatternMining p, int patternCode, int cellIndex, int player)
        {
            return p.GetCandidate(Key(patternCode, cellIndex, player));
        }
        
        public static ushort GetCandidate(this PatternMining p, int key)
        {
            var hasValue = p.Candidate.TryGetValue(key, out _);
            if (!hasValue) p.Candidate[key] = 0; // default Wi Candidate is 0
            return p.Candidate[key];
        }
        
        public static void AddCandidate(this PatternMining p, int patternCode, int cellIndex, int player)
        {
            var key = Key(patternCode, cellIndex, player);
            p.Candidate[key] = (ushort) (p.GetCandidate(key) + 1);
        }
        
        // ------------------------------- GammaDenominator -------------------------------
        public static float GetGammaDenominator(this PatternMining p, int patternCode, int cellIndex, int player)
        {
            return p.GetGammaDenominator(Key(patternCode, cellIndex, player));
        }
        
        public static float GetGammaDenominator(this PatternMining p, int key)
        {
            var hasValue = p.GammaDenominator.TryGetValue(key, out _);
            if (!hasValue) p.GammaDenominator[key] = 0f; // default GammaDenominator value is 0
            return p.GammaDenominator[key];
        }
        
        public static void SetGammaDenominator(this PatternMining p, int patternCode, int cellIndex, int player,
            float value)
        {
            p.GammaDenominator[Key(patternCode, cellIndex, player)] = value;
        }
        
        public static void AddGammaDenominator(this PatternMining p, int patternCode, int cellIndex, int player,
            float value)
        {
            var key = Key(patternCode, cellIndex, player);
            p.GammaDenominator[key] = p.GetGammaDenominator(key) + value;
        }
    }
}