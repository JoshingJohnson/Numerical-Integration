using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Rendering.Composition;
using Numerical_Integration.Models;
using Numerical_Integration.ViewModels;
using ReactiveUI;
using ScottPlot;
using ScottPlot.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace Numerical_Integration.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContextChanged += MainWindow_DataContextChanged;
        }

        public FunctionValues HeldValues = new FunctionValues(); // This will hold our values accross all the functions nicely

        private void MainWindow_DataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.FunctionButton.Subscribe(functionInputs =>
                {
                    foreach (var fi in functionInputs)
                    {
                        int xStart = fi.XStart;
                        int xEnd = fi.XEnd;
                        string func = fi.Function;

                        double[] x = new double[xEnd - xStart + 1];
                        for (int i = 0; i < x.Length; i++)
                            x[i] = xStart + i;

                        HeldValues.FunctionX = x;
                        HeldValues.FunctionY = EvaluateFunction(x, func);

                        int n = int.TryParse(vm.InputN, out int NValue) ? NValue : 1;
                        NumericalIntegration(xStart, xEnd, func, n);
                    }

                    UpdatePlot(vm);
                });
                Observable.Merge(
                    vm.WhenAnyValue(x => x.ShowXaxis).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowYaxis).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowFunction).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowRectangle).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowRectangleError).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowRectangleErrorLog).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowTrapezoid).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowTrapezoidError).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowTrapezoidErrorLog).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowSimpson).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowSimpsonError).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowSimpsonErrorLog).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowHitMiss).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowHitMissError).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowHitMissErrorLog).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowTraditional).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowTraditionalError).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowTraditionalErrorLog).Select(_ => Unit.Default),
                    vm.WhenAnyValue(x => x.ShowBestestIntegral).Select(_ => Unit.Default)
                ).Subscribe(_ => UpdatePlot(vm));
            }
        }

        private void UpdatePlot(MainWindowViewModel vm)
        {
            FunctionPlot.Plot.Clear();
            if (vm.ShowXaxis) FunctionPlot.Plot.Add.HorizontalLine(0, color: Colors.Gray, width: 1);
            if (vm.ShowYaxis) FunctionPlot.Plot.Add.VerticalLine(0, color: Colors.Gray, width: 1);
            if (vm.ShowFunction && HeldValues.FunctionX != null) FunctionPlot.Plot.Add.Scatter(HeldValues.FunctionX, HeldValues.FunctionY);
            if (vm.ShowRectangle) FunctionPlot.Plot.Add.Scatter(HeldValues.GivenN, HeldValues.RectangleEstimate);
            if (vm.ShowRectangleError) FunctionPlot.Plot.Add.Scatter(HeldValues.GivenN, HeldValues.RectangleEstimateError);
            if (vm.ShowRectangleErrorLog) FunctionPlot.Plot.Add.Scatter(HeldValues.StepsForGivenN, HeldValues.RectangleEstimateErrorLog);
            if (vm.ShowTrapezoid) FunctionPlot.Plot.Add.Scatter(HeldValues.GivenN, HeldValues.TrapezoidEstimate);
            if (vm.ShowTrapezoidError) FunctionPlot.Plot.Add.Scatter(HeldValues.GivenN, HeldValues.TrapezoidEstimateError);
            if (vm.ShowTrapezoidErrorLog) FunctionPlot.Plot.Add.Scatter(HeldValues.StepsForGivenN, HeldValues.TrapezoidEstimateErrorLog);
            if (vm.ShowSimpson) FunctionPlot.Plot.Add.Scatter(HeldValues.GivenN, HeldValues.SimpsonEstimate);
            if (vm.ShowSimpsonError) FunctionPlot.Plot.Add.Scatter(HeldValues.GivenN, HeldValues.SimpsonEstimateError);
            if (vm.ShowSimpsonErrorLog) FunctionPlot.Plot.Add.Scatter(HeldValues.StepsForGivenN, HeldValues.SimpsonEstimateErrorLog);
            if (vm.ShowHitMiss) FunctionPlot.Plot.Add.Scatter(HeldValues.GivenN, HeldValues.HitMissMonteCarloEstimate);
            if (vm.ShowHitMissError) FunctionPlot.Plot.Add.Scatter(HeldValues.GivenN, HeldValues.HitMissMonteCarloEstimateError);
            if (vm.ShowHitMissErrorLog) FunctionPlot.Plot.Add.Scatter(HeldValues.StepsForGivenN, HeldValues.HitMissMonteCarloEstimateErrorLog);
            if (vm.ShowTraditional) FunctionPlot.Plot.Add.Scatter(HeldValues.GivenN, HeldValues.TraditonalMonteCarloEstimate);
            if (vm.ShowTraditionalError) FunctionPlot.Plot.Add.Scatter(HeldValues.GivenN, HeldValues.TraditonalMonteCarloEstimateError);
            if (vm.ShowTraditionalErrorLog) FunctionPlot.Plot.Add.Scatter(HeldValues.StepsForGivenN, HeldValues.TraditonalMonteCarloEstimateErrorLog);
            if (vm.ShowBestestIntegral) FunctionPlot.Plot.Add.HorizontalLine(HeldValues.bestestMostAccurateIntegralValue);
            FunctionPlot.Plot.Axes.AutoScale();
            FunctionPlot.Refresh();
        }

        // Logic for evaluating functions

        // Splits a function string into top-level terms by + or - (but not inside parentheses)
        static List<string> FunctionSplitter(string input)
        {
            List<string> terms = new();
            int depth = 0;
            int lastSplit = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '(') depth++;
                else if (c == ')') depth--;
                else if ((c == '+' || c == '-') && depth == 0 && i != 0)
                {
                    terms.Add(input.Substring(lastSplit, i - lastSplit).Trim());
                    lastSplit = i;
                }
            }

            // Add final term
            if (lastSplit < input.Length)
                terms.Add(input.Substring(lastSplit).Trim());

            return terms;
        }

        // Finds the closing bracket matching an opening at startIndex
        static int FindCloseBracket(int startIndex, string s)
        {
            int depth = 0;
            for (int i = startIndex; i < s.Length; i++)
            {
                if (s[i] == '(') depth++;
                else if (s[i] == ')')
                {
                    depth--;
                    if (depth == 0)
                        return i;
                }
            }
            throw new ArgumentException("Unmatched parenthesis in expression.");
        }

        // Main evaluation function
        static double[] EvaluateFunction(double[] x, string functionString)
        {
            double[] y = new double[x.Length];
            Array.Fill(y, 0.0);

            var separatedTerms = FunctionSplitter(functionString);

            foreach (string term in separatedTerms)
            {
                double[] termValues = EvaluateTerm(x, term);
                for (int i = 0; i < x.Length; i++)
                    y[i] += termValues[i];
            }

            return y;
        }

        // Evaluates a single term
        static double[] EvaluateTerm(double[] x, string term)
        {
            double[] y = new double[x.Length];
            Regex numberCheck = new(@"^[0-9]*\.?[0-9]+$");
            int startCharacter = 0;
            switch (term[0])
            {
                case ('-'):
                    startCharacter = 1;
                    Array.Fill(y, -1.0);
                    break;
                case ('+'):
                    startCharacter = 1;
                    goto default;
                default:
                    Array.Fill(y, 1.0);
                    break;

            }


            for (int i = startCharacter; i < term.Length;)
            {
                char c = term[i];

                // Skip whitespace
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                // Handle numbers
                if (char.IsDigit(c) || (c == '.' && i + 1 < term.Length && char.IsDigit(term[i + 1])))
                {
                    int startOfNumber = i;
                    while (i < term.Length && (char.IsDigit(term[i]) || term[i] == '.'))
                        i++;

                    double num = double.Parse(term.Substring(startOfNumber, i - startOfNumber));
                    for (int pos = 0; pos < x.Length; pos++)
                        y[pos] *= num;

                    continue;
                }

                // Handle variables (x)
                if (c == 'x')
                {
                    if (i + 1 < term.Length && term[i + 1] == '^')
                    {
                        int expStart = i + 2;
                        if (expStart < term.Length && term[expStart] == '(')
                        {
                            int expEnd = FindCloseBracket(expStart, term);
                            string inner = term.Substring(expStart + 1, expEnd - expStart - 1);
                            double[] innerVal = EvaluateFunction(x, inner);
                            for (int pos = 0; pos < x.Length; pos++)
                                y[pos] *= Math.Pow(x[pos], innerVal[pos]);
                            i = expEnd + 1;
                        }
                        else
                        {
                            int numStart = expStart;
                            while (numStart < term.Length && (char.IsDigit(term[numStart]) || term[numStart] == '.'))
                                numStart++;
                            double power = double.Parse(term.Substring(expStart, numStart - expStart));
                            for (int pos = 0; pos < x.Length; pos++)
                                y[pos] *= Math.Pow(x[pos], power);
                            i = numStart;
                        }
                    }
                    else
                    {
                        for (int pos = 0; pos < x.Length; pos++)
                            y[pos] *= x[pos];
                        i++;
                    }
                    continue;
                }

                // Handle e constant (e or e^...)
                if (c == 'e')
                {
                    if (i + 1 < term.Length && term[i + 1] == '^')
                    {
                        int expStart = i + 2;
                        if (expStart < term.Length && term[expStart] == '(')
                        {
                            int expEnd = FindCloseBracket(expStart, term);
                            string inner = term.Substring(expStart + 1, expEnd - expStart - 1);
                            double[] innerVal = EvaluateFunction(x, inner);
                            for (int pos = 0; pos < x.Length; pos++)
                                y[pos] *= Math.Pow(Math.E, innerVal[pos]);
                            i = expEnd + 1;
                        }
                        else
                        {
                            int numStart = expStart;
                            while (numStart < term.Length && (char.IsDigit(term[numStart]) || term[numStart] == '.'))
                                numStart++;
                            double power = double.Parse(term.Substring(expStart, numStart - expStart));
                            for (int pos = 0; pos < x.Length; pos++)
                                y[pos] *= Math.Pow(Math.E, power);
                            i = numStart;
                        }
                    }
                    else
                    {
                        for (int pos = 0; pos < x.Length; pos++)
                            y[pos] *= Math.E;
                        i++;
                    }
                    continue;
                }

                // Handle pi constant
                if (i+1 < term.Length && term.Substring(i,2) == "pi")
                {
                    if (i + 2 < term.Length && term[i + 2] == '^')
                    {
                        int expStart = i + 3;
                        if (expStart < term.Length && term[expStart] == '(')
                        {
                            int expEnd = FindCloseBracket(expStart, term);
                            string inner = term.Substring(expStart + 1, expEnd - expStart - 1);
                            double[] innerVal = EvaluateFunction(x, inner);
                            for (int pos = 0; pos < x.Length; pos++)
                                y[pos] *= Math.Pow(Math.PI, innerVal[pos]);
                            i = expEnd + 1;
                        }
                        else
                        {
                            int numStart = expStart;
                            while (numStart < term.Length && (char.IsDigit(term[numStart]) || term[numStart] == '.'))
                                numStart++;
                            double power = double.Parse(term.Substring(expStart, numStart - expStart));
                            for (int pos = 0; pos < x.Length; pos++)
                                y[pos] *= Math.Pow(Math.PI, power);
                            i = numStart;
                        }
                    }
                    else
                    {
                        for (int pos = 0; pos < x.Length; pos++)
                            y[pos] *= Math.PI;
                        i+=2;
                    }
                    continue;
                }

                // Handle log()
                if (i+2 < term.Length && term.Substring(i,3) == "log")
                {
                    int open = i + 3;
                    int close = FindCloseBracket(open, term);
                    string inner = term.Substring(open + 1, close - open - 1);
                    double[] innerVal = EvaluateFunction(x, inner);
                    for (int pos = 0; pos < x.Length; pos++)
                        y[pos] *= Math.Log10(innerVal[pos]);
                    i = close + 1;
                    continue;
                }

                // Handle ln()
                if (i+1 < term.Length && term.Substring(i,2) == "ln")
                {
                    int open = i + 2;
                    int close = FindCloseBracket(open, term);
                    string inner = term.Substring(open + 1, close - open - 1);
                    double[] innerVal = EvaluateFunction(x, inner);
                    for (int pos = 0; pos < x.Length; pos++)
                        y[pos] *= Math.Log(innerVal[pos]);
                    i = close + 1;
                    continue;
                }
               
                // Handle functions sin(), cos(), tan()
                if (i + 2 < term.Length && term.Substring(i, 3) == "sin")
                {
                    int open = i + 3;
                    int close = FindCloseBracket(open, term);
                    string inner = term.Substring(open + 1, close - open - 1);
                    double[] innerVal = EvaluateFunction(x, inner);
                    for (int pos = 0; pos < x.Length; pos++)
                        y[pos] *= Math.Sin(innerVal[pos]);
                    i = close + 1;
                    continue;
                }

                if (i + 2 < term.Length && term.Substring(i, 3) == "cos")
                {
                    int open = i + 3;
                    int close = FindCloseBracket(open, term);
                    string inner = term.Substring(open + 1, close - open - 1);
                    double[] innerVal = EvaluateFunction(x, inner);
                    for (int pos = 0; pos < x.Length; pos++)
                        y[pos] *= Math.Cos(innerVal[pos]);
                    i = close + 1;
                    continue;
                }

                if (i + 2 < term.Length && term.Substring(i, 3) == "tan")
                {
                    int open = i + 3;
                    int close = FindCloseBracket(open, term);
                    string inner = term.Substring(open + 1, close - open - 1);
                    double[] innerVal = EvaluateFunction(x, inner);
                    for (int pos = 0; pos < x.Length; pos++)
                        y[pos] *= Math.Tan(innerVal[pos]);
                    i = close + 1;
                    continue;
                }

                // Handle brackets directly: ( ... )
                if (c == '(')
                {
                    int close = FindCloseBracket(i, term);
                    string inner = term.Substring(i + 1, close - i - 1);
                    double[] innerVal = EvaluateFunction(x, inner);
                    for (int pos = 0; pos < x.Length; pos++)
                        y[pos] *= innerVal[pos];
                    i = close + 1;
                    continue;
                }

                // Handle division
                if (c == '/')
                {
                    i++;
                    if (i < term.Length && term[i] == '(')
                    {
                        int close = FindCloseBracket(i, term);
                        string inner = term.Substring(i + 1, close - i - 1);
                        double[] innerVal = EvaluateFunction(x, inner);
                        for (int pos = 0; pos < x.Length; pos++)
                            y[pos] /= innerVal[pos];
                        i = close + 1;
                    }
                    continue;
                }
            }
            return y;
        }

        private void NumericalIntegration(double xStart, double xEnd, string functionString, int N)
        {
            double step = 0, midStep = 0, currentStepSum = 0, nextStepSum = 0, midStepSum = 0, XHolder = 0, YHolder = 0, hitMissMonteCarloHits = 0, traditionalMonteCarloHits = 0;
            double xRange = xEnd - xStart;
            double[] rectangleNint = new double[N], rectangleNintError = new double[N], trapeziaNint = new double[N], trapeziaNintError = new double[N], hitMissMonteCarloNint = new double[N], hitMissMonteCarloNintError = new double[N], traditionalMonteCarloNint = new double[N], traditionalMonteCarloNintError = new double[N], simpsonRuleNint = new double[N], simpsonRuleNintError = new double[N], nArray = new double[N], stepArray = new double[N];
            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;
            Random rng = new Random();
            for (int i = 0; i < 40000; i++) // Might need to tweak this value of samples, could have user input, could not
            {
                XHolder = xStart + (xRange) * rng.NextDouble();
                YHolder = (EvaluateFunction(new[] { XHolder }, functionString)[0]);
                if (YHolder < min) min = YHolder;
                if (YHolder > max) max = YHolder;
            }
            if (min > 0) min = 0;
            if (max < 0) max = 0;
            double monteCarloBoxArea = (max - min) * (xRange);
            for (int i = 0; i < 40000; i++) // Might need tweaking, could do user input
            {
                if ((max - min) * rng.NextDouble() < (EvaluateFunction(new[] { xRange * rng.NextDouble() + xStart }, functionString)[0] - min)) hitMissMonteCarloHits++;
            }
            double bestestIntegral = (hitMissMonteCarloHits / 40000) * monteCarloBoxArea + min * xRange;
            for (int n = 1; n <= N; n++)
            {
                currentStepSum = 0;
                midStepSum = 0;
                hitMissMonteCarloHits = 0;
                traditionalMonteCarloHits = 0;
                step = xRange / n;
                stepArray[n - 1] = Math.Log10(step);
                midStep = step / 2;

                for (int k = 0; k < n; k++)
                {
                    double x = xStart + k * step;
                    YHolder = EvaluateFunction(new[] { x }, functionString)[0];
                    currentStepSum += YHolder;
                    midStepSum += EvaluateFunction(new[] { x + midStep }, functionString)[0];
                    XHolder = (max - min) * rng.NextDouble();
                    if (XHolder < (YHolder - min)) hitMissMonteCarloHits++;
                    if (XHolder < (EvaluateFunction(new[] { xRange * rng.NextDouble() + xStart }, functionString)[0] - min)) traditionalMonteCarloHits++;
                }

                nextStepSum = currentStepSum - EvaluateFunction(new[] { xStart }, functionString)[0] + EvaluateFunction(new[] { xEnd }, functionString)[0];

                rectangleNint[n - 1] = currentStepSum * step;
                rectangleNintError[n - 1] = Math.Abs(rectangleNint[n - 1] - bestestIntegral);

                // Trapezoid using average of left and right endpoint sums
                trapeziaNint[n - 1] = ((currentStepSum + nextStepSum) / 2.0) * step;
                trapeziaNintError[n - 1] = Math.Abs(trapeziaNint[n - 1] - bestestIntegral);

                // Simpson
                if (n % 2 == 0)
                {
                    double simpsonSum = 0.0;
                    for (int k = 0; k <= n; k++)
                    {
                        double x = xStart + k * step;
                        double y = EvaluateFunction(new[] { x }, functionString)[0];

                        if (k == 0 || k == n)
                            simpsonSum += y;
                        else if (k % 2 == 1)
                            simpsonSum += 4.0 * y;
                        else
                            simpsonSum += 2.0 * y;
                    }
                    simpsonRuleNint[n - 1] = simpsonSum * (step / 3.0);
                }
                else
                {
                    // Does trapezoid for odd n
                    simpsonRuleNint[n - 1] = trapeziaNint[n - 1];
                }
                simpsonRuleNintError[n - 1] = Math.Abs(simpsonRuleNint[n - 1] - bestestIntegral);

                hitMissMonteCarloNint[n - 1] = (hitMissMonteCarloHits / (double)n) * monteCarloBoxArea + min * xRange;
                hitMissMonteCarloNintError[n - 1] = Math.Abs(hitMissMonteCarloNint[n - 1] - bestestIntegral);
                traditionalMonteCarloNint[n - 1] = (traditionalMonteCarloHits / (double)n) * monteCarloBoxArea + min * xRange;
                traditionalMonteCarloNintError[n - 1] = Math.Abs(traditionalMonteCarloNint[n - 1] - bestestIntegral);
                nArray[n - 1] = n;
            }
            HeldValues.RectangleEstimate = rectangleNint;
            HeldValues.RectangleEstimateError = (double[])rectangleNintError.Clone();
            HeldValues.TrapezoidEstimate = trapeziaNint;
            HeldValues.TrapezoidEstimateError = (double[])trapeziaNintError.Clone();
            HeldValues.SimpsonEstimate = simpsonRuleNint;
            HeldValues.SimpsonEstimateError = (double[])simpsonRuleNintError.Clone();
            HeldValues.HitMissMonteCarloEstimate = hitMissMonteCarloNint;
            HeldValues.HitMissMonteCarloEstimateError = (double[])hitMissMonteCarloNintError.Clone();
            HeldValues.TraditonalMonteCarloEstimate = traditionalMonteCarloNint;
            HeldValues.TraditonalMonteCarloEstimateError = (double[])traditionalMonteCarloNintError.Clone();
            HeldValues.GivenN = nArray;
            HeldValues.StepsForGivenN = stepArray;
            HeldValues.bestestMostAccurateIntegralValue = bestestIntegral;
            // I don't want more arrays, so I am going to reuse those first errors arrays for the log and just have another loop
            for (int n = 0; n < N; n++)
            {
                rectangleNintError[n] = Math.Log10(Math.Max(rectangleNintError[n], 1e-300));
                trapeziaNintError[n] = Math.Log10(Math.Max(trapeziaNintError[n], 1e-300));
                simpsonRuleNintError[n] = Math.Log10(Math.Max(simpsonRuleNintError[n], 1e-300));
                hitMissMonteCarloNintError[n] = Math.Log10(Math.Max(hitMissMonteCarloNintError[n], 1e-300));
                traditionalMonteCarloNintError[n] = Math.Log10(Math.Max(traditionalMonteCarloNintError[n], 1e-300));
            }
            HeldValues.RectangleEstimateErrorLog = rectangleNintError;
            HeldValues.TrapezoidEstimateErrorLog = trapeziaNintError;
            HeldValues.SimpsonEstimateErrorLog = simpsonRuleNintError;
            HeldValues.HitMissMonteCarloEstimateErrorLog = hitMissMonteCarloNintError;
            HeldValues.TraditonalMonteCarloEstimateErrorLog = traditionalMonteCarloNintError;
        }
    }
}