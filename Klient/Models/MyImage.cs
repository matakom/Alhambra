using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Klient.Models
{
    public class MyImage : Image
    {
        public Vector2 vector;
        public bool chosen;
        public MyImage(Vector2 vector)
        {
            this.vector = vector;
        }
    }
}
