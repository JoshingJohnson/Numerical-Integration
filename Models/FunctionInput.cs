using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Numerical_Integration.Models
{
    public class FunctionInput
    {
        public string Function { get; set; } = "";
        public int XStart { get; set; }
        public int XEnd { get; set; }
        public int N { get; set; }
    }
    public class FunctionValues
    {
        public double[] FunctionX { get; set; } // Holds the function values
        public double[] FunctionY { get; set; }
        public double[] RectangleEstimate { get; set; } // Holds the numerical integration values of different techniques
        public double[] TrapezoidEstimate { get; set; }
        public double[] SimpsonEstimate { get; set; }
        public double[] HitMissMonteCarloEstimate { get; set; }
        public double[] TraditonalMonteCarloEstimate { get; set; }
        public double[] RectangleEstimateError { get; set; } // Holds the error against N of different numerical integration techniques
        public double[] TrapezoidEstimateError { get; set; }
        public double[] SimpsonEstimateError { get; set; }
        public double[] HitMissMonteCarloEstimateError { get; set; }
        public double[] TraditonalMonteCarloEstimateError { get; set; }
        public double[] RectangleEstimateErrorLog { get; set; } // Holds the error with log applied
        public double[] TrapezoidEstimateErrorLog { get; set; }
        public double[] SimpsonEstimateErrorLog { get; set; }
        public double[] HitMissMonteCarloEstimateErrorLog { get; set; }
        public double[] TraditonalMonteCarloEstimateErrorLog { get; set; }
        public double[] GivenN { get; set; } // Holds the values of N
        public double[] StepsForGivenN { get; set; }
        public double bestestMostAccurateIntegralValue { get; set; }
    }
}
