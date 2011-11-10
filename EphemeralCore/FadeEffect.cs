using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace Ephemeral
{
    public static class FadeEffect
    {
        public static void BeginFade(Form form, double fromOpacity, double toOpacity, double durationMsec, Action endCallback = null)
        {
            Thread fadeThread = new Thread(new ThreadStart(delegate
            {
                DoFade(form, fromOpacity, toOpacity, durationMsec, endCallback);
            }));

            fadeThread.IsBackground = true;
            fadeThread.Start();
        }

        public static void Fade(Form form, double fromOpacity, double toOpacity, double durationMsec)
        {
            int direction = (fromOpacity > toOpacity) ? -1 : 1;

            double step = 0.01d;
            int steps = (int)(Math.Abs(fromOpacity - toOpacity) / step);
            double sleepDuration = durationMsec / steps;

            for (double opacity = fromOpacity; (-direction * opacity) > (-direction * toOpacity); opacity += (direction * step))
            {
                form.Opacity = opacity;
                
                Application.DoEvents();
                Sleep.SleepMsec(sleepDuration);
            }
        } 

        static void DoFade(Form form, double fromOpacity, double toOpacity, double durationMsec, Action endCallback)
        {
            form.BeginInvoke(new Action(delegate
            {
                Fade(form, fromOpacity, toOpacity, durationMsec);

                if (endCallback != null)
                {
                    endCallback();
                }
            }));
        }
    }

    static class Sleep
    {
        public static void SleepMsec(double durationMsec)
        {
            double frequencyMsec = Stopwatch.Frequency / 1000d;
            long tickDuration = (long)(frequencyMsec * durationMsec);
            SleepTicks(tickDuration);
        }

        public static void SleepTicks(long tickDuration)
        {
            Stopwatch.Reset();
            Stopwatch.Start();

            long ticks = Stopwatch.ElapsedTicks;
            while ((Stopwatch.ElapsedTicks - ticks) < tickDuration)
            {
            }
        }

        static Stopwatch Stopwatch = new Stopwatch();
    }
}
