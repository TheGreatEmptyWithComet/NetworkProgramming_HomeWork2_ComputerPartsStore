using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreApp
{
    public class ComputerPart : NotifyPropertyChangeHandler
    {
        public string Name { get; set; }

        private double price;
        public double Price
        {
            get => price;
            set
            {
                price = value;
                NotifyPropertyChanged(nameof(Price));
            }
        }
    }
}
