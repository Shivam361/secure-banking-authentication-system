using Microsoft.ML.Data;
using System;

namespace SecureBankingApp.Models
{
    public class TransactionData
    {
        public float Amount { get; set; }
    }

    public class TransactionPrediction
    {
        // Vector size must match IidSpikeEstimator output (usually 3):
        // [0] Alert (0 or 1)
        // [1] Raw score
        // [2] P-Value
        [VectorType(3)]
        public double[]? Prediction { get; set; }
    }
}
