using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klient.Models
{
    public class MyImage : Image
    {
        public void SetBounds(Rect bounds)
        {
            Bounds = bounds;
        }
    }
}
