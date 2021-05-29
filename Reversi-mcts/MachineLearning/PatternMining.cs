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
        public static int Key(int patternCode, int cellIndex, int player)
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
            var hasValue = p.Gamma.TryGetValue(key, out var value);
            return hasValue ? value : 1f; // default gamma value is 1f
        }

        public static void SetGamma(this PatternMining p, int key, float value)
        {
            p.Gamma[key] = value;
        }
        
        // ------------------------------- Win -------------------------------
        public static ushort GetWin(this PatternMining p, int key)
        {
            var hasValue = p.Win.TryGetValue(key, out var value);
            return hasValue ? value :  (ushort) 0; // default Wi value is 0
        }
        
        public static void AddWin(this PatternMining p, int key)
        {
            p.Win[key] = (ushort) (p.GetWin(key) + 1);
        }
        
        // ------------------------------- Candidate -------------------------------
        public static ushort GetCandidate(this PatternMining p, int patternCode, int cellIndex, int player)
        {
            return p.GetCandidate(Key(patternCode, cellIndex, player));
        }
        
        public static ushort GetCandidate(this PatternMining p, int key)
        {
            var hasValue = p.Candidate.TryGetValue(key, out var value);
            return hasValue ? value : (ushort) 0; // default Wi Candidate is 0
        }
        
        public static void AddCandidate(this PatternMining p, int key)
        {
            p.Candidate[key] = (ushort) (p.GetCandidate(key) + 1);
        }
        
        // ------------------------------- GammaDenominator -------------------------------
        public static float GetGammaDenominator(this PatternMining p, int key)
        {
            var hasValue = p.GammaDenominator.TryGetValue(key, out var value);
            return hasValue ? value : 0f;
        }
        
        public static void ResetGammaDenominator(this PatternMining p, int key)
        {
            p.GammaDenominator.Remove(key);
        }
        
        public static void AddGammaDenominator(this PatternMining p, int patternCode, int cellIndex, int player,
            float value)
        {
            var key = Key(patternCode, cellIndex, player);
            p.GammaDenominator[key] = p.GetGammaDenominator(key) + value;
        }
    }
}