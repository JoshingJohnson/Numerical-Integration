using Avalonia.Interactivity;
using Numerical_Integration.Models;
using ReactiveUI;
using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Numerical_Integration.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public ObservableCollection<FunctionDimensionViewModel> FunctionDimensions { get; } = new();

        private int _dimensionCount = 2;
        public int DimensionCount
        {
            get => _dimensionCount;
            set
            {
                int whole = Math.Max(2, (int)Math.Floor((double)value)); // Ensure a whole number >= 2
                if (_dimensionCount != whole)
                {
                    this.RaiseAndSetIfChanged(ref _dimensionCount, whole);
                    UpdateFunctionDimensions(); // Update the expanders
                }
            }
        }

        private string _inputN = "10";
        public string InputN
        {
            get => _inputN;
            set => this.RaiseAndSetIfChanged(ref _inputN, value);
        }
        // Manages the pop up for the drop boxes
        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set => this.RaiseAndSetIfChanged(ref _isPopupOpen, value);
        }
        // Dropable check boxes
        private bool _showYaxis = true;
        public bool ShowYaxis
        {
            get => _showYaxis;
            set => this.RaiseAndSetIfChanged(ref _showYaxis, value);
        }
        private bool _showXaxis = true;
        public bool ShowXaxis
        {
            get => _showXaxis;
            set => this.RaiseAndSetIfChanged(ref _showXaxis, value);
        }
        
        private bool _showFunction = true;
        public bool ShowFunction
        {
            get => _showFunction;
            set => this.RaiseAndSetIfChanged(ref _showFunction, value);
        }

        private bool _showRectangle;
        public bool ShowRectangle
        {
            get => _showRectangle;
            set => this.RaiseAndSetIfChanged(ref _showRectangle, value);
        }

        private bool _showRectangleError;
        public bool ShowRectangleError
        {
            get => _showRectangleError;
            set => this.RaiseAndSetIfChanged(ref _showRectangleError, value);
        }

        private bool _showRectangleErrorLog;
        public bool ShowRectangleErrorLog
        {
            get => _showRectangleErrorLog;
            set => this.RaiseAndSetIfChanged(ref _showRectangleErrorLog, value);
        }

        private bool _showTrapezoid;
        public bool ShowTrapezoid
        {
            get => _showTrapezoid;
            set => this.RaiseAndSetIfChanged(ref _showTrapezoid, value);
        }
        private bool _showTrapezoidError;
        public bool ShowTrapezoidError
        {
            get => _showTrapezoidError;
            set => this.RaiseAndSetIfChanged(ref _showTrapezoidError, value);
        }

        private bool _showTrapezoidErrorLog;
        public bool ShowTrapezoidErrorLog
        {
            get => _showTrapezoidErrorLog;
            set => this.RaiseAndSetIfChanged(ref _showTrapezoidErrorLog, value);
        }

        private bool _showSimpson;
        public bool ShowSimpson
        {
            get => _showSimpson;
            set => this.RaiseAndSetIfChanged(ref _showSimpson, value);
        }
        private bool _showSimpsonError;
        public bool ShowSimpsonError
        {
            get => _showSimpsonError;
            set => this.RaiseAndSetIfChanged(ref _showSimpsonError, value);
        }

        private bool _showSimpsonErrorLog;
        public bool ShowSimpsonErrorLog
        {
            get => _showSimpsonErrorLog;
            set => this.RaiseAndSetIfChanged(ref _showSimpsonErrorLog, value);
        }

        private bool _showHitMiss;
        public bool ShowHitMiss
        {
            get => _showHitMiss;
            set => this.RaiseAndSetIfChanged(ref _showHitMiss, value);
        }
        private bool _showHitMissError;
        public bool ShowHitMissError
        {
            get => _showHitMissError;
            set => this.RaiseAndSetIfChanged(ref _showHitMissError, value);
        }

        private bool _showHitMissErrorLog;
        public bool ShowHitMissErrorLog
        {
            get => _showHitMissErrorLog;
            set => this.RaiseAndSetIfChanged(ref _showHitMissErrorLog, value);
        }

        private bool _showTraditional;
        public bool ShowTraditional
        {
            get => _showTraditional;
            set => this.RaiseAndSetIfChanged(ref _showTraditional, value);
        }
        private bool _showTraditionalError;
        public bool ShowTraditionalError
        {
            get => _showTraditionalError;
            set => this.RaiseAndSetIfChanged(ref _showTraditionalError, value);
        }

        private bool _showTraditionalErrorLog;
        public bool ShowTraditionalErrorLog
        {
            get => _showTraditionalErrorLog;
            set => this.RaiseAndSetIfChanged(ref _showTraditionalErrorLog, value);
        }

        private bool _showBestestIntegral;
        public bool ShowBestestIntegral
        {
            get => _showBestestIntegral;
            set => this.RaiseAndSetIfChanged(ref _showBestestIntegral, value);
        }

        private bool _showRectangleExplanation;
        public bool ShowRectangleExplanation
        {
            get => _showRectangleExplanation;
            set => this.RaiseAndSetIfChanged(ref _showRectangleExplanation, value);
        }

        private bool _showTrapeziaExplanation;
        public bool ShowTrapeziaExplanation
        {
            get => _showTrapeziaExplanation;
            set => this.RaiseAndSetIfChanged(ref _showTrapeziaExplanation, value);
        }

        private bool _showMonteCarloExplanation;
        public bool ShowMonteCarloExplanation
        {
            get => _showMonteCarloExplanation;
            set => this.RaiseAndSetIfChanged(ref _showMonteCarloExplanation, value);
        }
        private bool _showSimsponExplanation;
        public bool ShowSimpsonExplanation
        {
            get => _showSimsponExplanation;
            set => this.RaiseAndSetIfChanged(ref _showSimsponExplanation, value);
        }

        public ReactiveCommand<Unit, FunctionInput[]> FunctionButton { get; }
        public ReactiveCommand<Unit, Unit> TogglePopupCommand { get; }

        public MainWindowViewModel()
        {
            TogglePopupCommand = ReactiveCommand.Create(() =>
            {
                IsPopupOpen = !IsPopupOpen;
            });
            UpdateFunctionDimensions();
            FunctionButton = ReactiveCommand.Create(() =>
            {
                FunctionInput[] allInputs = FunctionDimensions.Select(fd => fd.ToFunctionInput()).ToArray();
                return allInputs;
            });
        }

        private void UpdateFunctionDimensions()
        {
            while (FunctionDimensions.Count < DimensionCount - 1)
                FunctionDimensions.Add(new FunctionDimensionViewModel());

            while (FunctionDimensions.Count > DimensionCount - 1)
                FunctionDimensions.RemoveAt(FunctionDimensions.Count - 1);
        }
    }
}
