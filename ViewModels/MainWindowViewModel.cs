using Avalonia.Interactivity;
using Numerical_Integration.Models;
using ReactiveUI;
using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;

namespace Numerical_Integration.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        // Text inputs
        private string _inputFunction = "";
        public string InputFunction
        {
            get => _inputFunction;
            set => this.RaiseAndSetIfChanged(ref _inputFunction, value);
        }

        private string _inputXStart = "";
        public string InputXStart
        {
            get => _inputXStart;
            set => this.RaiseAndSetIfChanged(ref _inputXStart, value);
        }

        private string _inputXEnd = "";
        public string InputXEnd
        {
            get => _inputXEnd;
            set => this.RaiseAndSetIfChanged(ref _inputXEnd, value);
        }

        private string _inputN = "";
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

        private bool _showBestestIntegral;
        public bool ShowBestestIntegral
        {
            get => _showBestestIntegral;
            set => this.RaiseAndSetIfChanged(ref _showBestestIntegral, value);
        }
        // Button
        public ReactiveCommand<Unit, FunctionInput> FunctionButton { get; }
        public ReactiveCommand<Unit, Unit> TogglePopupCommand { get; }

        public MainWindowViewModel()
        {
            TogglePopupCommand = ReactiveCommand.Create(() =>
            {
                IsPopupOpen = !IsPopupOpen;
            });

            FunctionButton = ReactiveCommand.Create(() =>
            {
                int.TryParse(InputXStart, out int xStart);
                int.TryParse(InputXEnd, out int xEnd);
                int.TryParse(InputN, out int N);

                var input = new FunctionInput
                {
                    Function = InputFunction,
                    XStart = xStart,
                    XEnd = xEnd,
                    N = N
                };

                Debug.WriteLine($"VM emitted: {InputFunction}, {xStart}, {xEnd}, {N}");
                return input;
            });
        }
    }
}
