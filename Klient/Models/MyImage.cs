using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using Klient.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Klient.Models
{
    public class MyImage : Image
    {
        public Vector2 vector;
        public bool chosen;
        private bool Chosen
        {
            get { return chosen; }
            set
            {
                if (chosen != value)
                {
                    chosen = value;
                    UpdateVisualState();
                }

            }
        }
        private bool canChange;
        public MyImage(Vector2 vector, bool yours)
        {
            this.vector = vector;
            chosen = false;
            canChange = yours;

            PointerPressed += OnPointerPressed;
        }
        private async void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (!canChange)
            {
                return;
            }

            chosen = !chosen;
            Debug.WriteLine(chosen);

            double to = 0;
            double from = 0;
            if (chosen)
            {
                to = 200;
                from = Height;
            }
            else
            {
                to = 160;
                from = Height;
            }
            double fps = 60;
            double sec = 1;

            double numberOfFrames = Math.Round(fps * sec);

            double shift = (to - from) / numberOfFrames;

            int delay = Convert.ToInt32(Math.Round((1000 / fps) / 11 * 6));

            for (int i = 0; i < numberOfFrames; i++)
            {
                Height += shift;
                await Task.Delay(delay);
            }
            if (chosen && Height != 200)
            {
                Height = 200;
            }
            else if (!chosen && Height != 160)
            {
                Height = 160;
            }
        }
        public async void UpdateVisualState()
        {
            double to = 0;
            double from = 0;
            if (chosen)
            {
                to = 200;
                from = Height;
            }
            else
            {
                to = 160;
                from = Height;
            }
            double fps = 60;
            double sec = 1;

            double numberOfFrames = Math.Round(fps * sec);

            double shift = (to - from) / numberOfFrames;

            int delay = Convert.ToInt32(Math.Round((1000 / fps) / 11 * 6));

            for (int i = 0; i < numberOfFrames; i++)
            {
                Height += shift;
                await Task.Delay(delay);
            }
            if (chosen && Height != 200)
            {
                Height = 200;
            }
            else if(!chosen && Height != 160)
            {
                Height = 160;
            }
        }
        public async void Move(Vector2 from, Vector2 to, double seconds, MyImage image, bool remove)
        {
            double fps = 60;

            double numberOfFrames = Math.Round(fps * seconds);

            double shiftX = (to.X - from.X) / numberOfFrames;
            double shiftY = (to.Y - from.Y) / numberOfFrames;

            //int delay = Convert.ToInt32(Math.Round((1000 / fps) / 11 * 6));

            Canvas.SetBottom(image, from.Y);
            Canvas.SetLeft(image, from.X);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000 / fps);
            int i = 0;

            timer.Tick += (sender, e) =>
            {
                double newBottom = Canvas.GetBottom(image) + shiftY;
                double newLeft = Canvas.GetLeft(image) + shiftX;
                Canvas.SetBottom(image, newBottom);
                Canvas.SetLeft(image, newLeft);
                image.InvalidateVisual();
                i++;
                if(i == numberOfFrames)
                {
                    timer.Stop();
                    if (remove)
                    {
                        GameView.canvas.Children.Remove(image);
                    }
                }
            };

            timer.Start();

            image.vector.X = (float)Canvas.GetLeft(image);
            image.vector.Y = (float)Canvas.GetBottom(image);
        }
    }
}
