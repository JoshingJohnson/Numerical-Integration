using Numerical_Integration.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Numerical_Integration.ViewModels
{
    public class FunctionDimensionViewModel : ReactiveObject
    {
        private string _function = "";
        public string Function
        {
            get => _function;
            set => this.RaiseAndSetIfChanged(ref _function, value);
        }

        private int _xStart;
        public int XStart
        {
            get => _xStart;
            set => this.RaiseAndSetIfChanged(ref _xStart, value);
        }

        private int _xEnd;
        public int XEnd
        {
            get => _xEnd;
            set => this.RaiseAndSetIfChanged(ref _xEnd, value);
        }

        public FunctionInput ToFunctionInput()
        {
            return new FunctionInput
            {
                Function = this.Function,
                XStart = this.XStart,
                XEnd = this.XEnd
            };
        }
    }
}
